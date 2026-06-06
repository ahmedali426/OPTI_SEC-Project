using System.Threading;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Contracts.AI;
using Opti_Sec_Backend.Contracts.Device;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;
using Opti_Sec_Backend.Services.DeviceCommandServices;
using Opti_Sec_Backend.Services.EmergencyServices;
using Opti_Sec_Backend.Services.FileServices;
using Opti_Sec_Backend.Services.FingerprintServices;
using Opti_Sec_Backend.Services.NotificationServices;
using Opti_Sec_Backend.Services.PasswordServices;
using Opti_Sec_Backend.Services.SessionServices;

namespace Opti_Sec_Backend.Services.SecurityWorkflow;

public class GateAccessOrchestrator(
    ApplicationDbContext context,
    IPasswordService passwordService,
    IFingerprintService fingerprintService,
    IEmergencyService emergencyService,
    ISessionService sessionService,
    INotificationService notificationService,
    IDeviceCommandService deviceCommandService,
    IFileService fileService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<GateAccessOrchestrator> logger) : IGateAccessOrchestrator
{
    private readonly ApplicationDbContext _context = context;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IFingerprintService _fingerprintService = fingerprintService;
    private readonly IEmergencyService _emergencyService = emergencyService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IDeviceCommandService _deviceCommandService = deviceCommandService;
    private readonly IFileService _fileService = fileService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<GateAccessOrchestrator> _logger = logger;

    private const int MaxAttempts = 3;
   
    // ══════════════════════════════════════════════
    //  STEP 1: PASSWORD VALIDATION
    // ══════════════════════════════════════════════
    public async Task<PasswordValidationResponse> ValidatePasswordAsync(
        PasswordValidationRequest request, CancellationToken ct = default)
    {
        var (status, attemptNumber) = await _passwordService
            .ValidateAsync(request.GateId, request.Password, request.DeviceId, ct);

        switch (status)
        {
            // CASE 0: Gate Not Found
            case PasswordStatus.GateNotFound:
                {
                    _logger.LogWarning("Gate not found: {GateId}", request.GateId);

                    return new PasswordValidationResponse(
                        Success: false,
                        SessionToken: null,
                        PasswordStatus: "GateNotFound",
                        NextStep: null,
                        AttemptNumber: 0,
                        RemainingAttempts: 0,
                        Emergency: false,
                        Commands: new DeviceCommandsDto());
                }
            // CASE 1: Correct Password
            case PasswordStatus.Correct:
                {
                    var session = await _sessionService
                        .CreateSessionAsync(request.GateId, request.DeviceId, ct);

                    await _notificationService.SendAsync(
                        NotificationType.PasswordSuccess,
                        request.GateId,
                        "Authorized Access",
                        $"Authorized password entered at Gate {request.GateId}",
                        ct: ct);

                    return new PasswordValidationResponse(
                        Success: true,
                        SessionToken: session.SessionToken,
                        PasswordStatus: "Correct",
                        NextStep: "CaptureImage",
                        AttemptNumber: 0,
                        RemainingAttempts: MaxAttempts,
                        Emergency: false,
                        Commands: new DeviceCommandsDto(
                            ActivateCamera: true,
                            DelaySeconds: 3));
                }

            // CASE 2: Silent Alarm
            case PasswordStatus.SilentAlarm:
                {
                    var session = await _sessionService
                        .CreateSessionAsync(request.GateId, request.DeviceId, ct);

                    var sessionEntity = await _context.GateSessions
                        .FindAsync([session.Id], ct);

                    if (sessionEntity is not null)
                    {
                        sessionEntity.IsSilentAlarm = true;
                        await _context.SaveChangesAsync(ct);
                    }

                    await _notificationService.SendSilentAlarmAsync(
                        request.GateId,
                        ct);

                    _logger.LogWarning(
                        "SILENT ALARM triggered at Gate {GateId} by device {DeviceId}",
                        request.GateId,
                        request.DeviceId);

                    // نفس رد الـ Correct بدون ما الجهاز يعرف
                    return new PasswordValidationResponse(
                        Success: true,
                        SessionToken: session.SessionToken,
                        PasswordStatus: "Correct",
                        NextStep: "CaptureImage",
                        AttemptNumber: 0,
                        RemainingAttempts: MaxAttempts,
                        Emergency: false,
                        Commands: new DeviceCommandsDto(
                            ActivateCamera: true,
                            DelaySeconds: 3));
                }

            // CASE 3: Wrong Password
            case PasswordStatus.Wrong:
                {
                    var remaining = MaxAttempts - attemptNumber;

                    if (attemptNumber >= MaxAttempts)
                    {
                        var emergency = await _emergencyService.TriggerEmergencyAsync(
                            request.GateId,
                            EmergencyType.PasswordBreach,
                            $"Multiple failed password attempts at Gate {request.GateId}",
                            ct: ct);

                        await _deviceCommandService.SendActivateBuzzerAsync(
                            request.GateId,
                            30,
                            ct: ct);

                        await _notificationService.SendEmergencyAlertAsync(
                            request.GateId,
                            EmergencyType.PasswordBreach,
                            $"Emergency! {MaxAttempts} failed password attempts at Gate {request.GateId}",
                            ct);

                        return new PasswordValidationResponse(
                            Success: false,
                            SessionToken: null,
                            PasswordStatus: "Wrong",
                            NextStep: null,
                            AttemptNumber: attemptNumber,
                            RemainingAttempts: 0,
                            Emergency: true,
                            Commands: new DeviceCommandsDto(
                                ActivateBuzzer: true,
                                BuzzerDurationSeconds: 30));
                    }

                    var priority = attemptNumber >= 2
                        ? NotificationPriority.High
                        : NotificationPriority.Normal;

                    await _notificationService.SendAsync(
                        NotificationType.WrongPassword,
                        request.GateId,
                        "Wrong Password",
                        $"Wrong password at Gate {request.GateId} (Attempt {attemptNumber}/{MaxAttempts})",
                        priority,
                        ct: ct);

                    return new PasswordValidationResponse(
                        Success: false,
                        SessionToken: null,
                        PasswordStatus: "Wrong",
                        NextStep: null,
                        AttemptNumber: attemptNumber,
                        RemainingAttempts: remaining,
                        Emergency: false,
                        Commands: new DeviceCommandsDto());
                }

            default:
                throw new InvalidOperationException(
                    $"Unknown password status: {status}");
         }
    
    }

    // ══════════════════════════════════════════════
    // STEP 2: AI RECOGNITION RESULT (Callback)
    // ══════════════════════════════════════════════
    public async Task<AIRecognitionResultResponse> ProcessAIResultAsync(
        AIRecognitionResultRequest request, CancellationToken ct = default)
    {
        var session = await _sessionService.GetByTokenAsync(request.SessionToken, ct);

        if (session is null)
        {
            return new AIRecognitionResultResponse(
                Received: false,
                Status: AIRecognitionStatus.InvalidSession,
                NextStep: null,
                MemberId: null,
                Message: "Session not found",
                Commands: new DeviceCommandsDto()
            );
        }

        string? imageName = null;

        if (request.ImageUrl is not null)
        {
            imageName = await _fileService.UploadImageAsync(request.ImageUrl, ct);
            session.CapturedImageName = imageName;
        }

        await _context.SaveChangesAsync(ct);

        // Log AI attempt
        var aiLog = new AIValidationLog
        {
            GateSessionId = session.Id,
            GateId = session.GateId,
            ImageUrl = session.CapturedImageName,
            IsAuthorized = request.IsAuthorized,
            ConfidenceScore = request.ConfidenceScore,
            MatchedMemberId = request.MatchedMemberId,
            AttemptNumber = session.AIAttemptCount + 1,
            ResponseTimeMs = request.ProcessingTimeMs,
            RespondedAt = DateTime.UtcNow
        };

        _context.AIValidationLogs.Add(aiLog);
        session.AIAttemptCount++;

        await _context.SaveChangesAsync(ct);

        // ══════════════════════════════════════
        // SUCCESS CASE
        // ══════════════════════════════════════
        if (request.IsAuthorized && request.ConfidenceScore >= 0.85)
        {
            session.AIPassed = true;
            session.AIValidatedAt = DateTime.UtcNow;
            session.AIConfidenceScore = request.ConfidenceScore;
            session.MemberId = request.MatchedMemberId;
            session.CurrentStep = SessionStep.Fingerprint;
            session.Status = SessionStatus.AIPassed;

            await _context.SaveChangesAsync(ct);

            await _notificationService.SendAsync(
                NotificationType.AIAuthorized,
                session.GateId,
                "Face Recognized",
                $"Face recognized at Gate {session.GateId}",
                ct: ct);

            return new AIRecognitionResultResponse(
                Received: true,
                Status: AIRecognitionStatus.Success,
                NextStep: "CaptureFingerprint",
                MemberId: request.MatchedMemberId,
                Message: "AI verification successful",
                Commands: new DeviceCommandsDto(
                    CaptureFingerprint: true,
                    ExpectedMemberId: request.MatchedMemberId
                )
            );
        }

        // ══════════════════════════════════════
        // MAX ATTEMPTS → EMERGENCY
        // ══════════════════════════════════════
        if (session.AIAttemptCount >= MaxAttempts)
        {
            _context.AccessLogs.Add(new AccessLog
            {
                GateId = session.GateId,
                MemberId = null,
                IsAuthorized = false,
                AccessMethod = AccessMethod.AI,
                GateSessionId = session.Id,
                ImageUrl = BuildImageUrl(session.CapturedImageName),
                CreatedById = session.Gate.Client.UserId
            });

            await _context.SaveChangesAsync(ct);

            await _emergencyService.TriggerEmergencyAsync(
                session.GateId,
                EmergencyType.AIFailed,
                $"Unrecognized person at Gate {session.GateId} after {MaxAttempts} attempts",
                sessionId: session.Id,
                ct: ct);

            await _deviceCommandService.SendActivateBuzzerAsync(
                session.GateId,
                30,
                session.Id,
                ct);

            await _sessionService.CompleteSessionAsync(
                session.Id,
                SessionResult.DeniedAI,
                "AI recognition failed after maximum attempts",
                ct);

            await _notificationService.SendEmergencyAlertAsync(
                session.GateId,
                EmergencyType.AIFailed,
                $"Unrecognized person at Gate {session.GateId}",
                ct);

            return new AIRecognitionResultResponse(
                Received: true,
                Status: AIRecognitionStatus.EmergencyTriggered,
                NextStep: null,
                MemberId: null,
                Message: "Maximum AI attempts reached - Emergency triggered",
                Commands: new DeviceCommandsDto(
                    ActivateBuzzer: true,
                    BuzzerDurationSeconds: 30
                )
            );
        }

        // ══════════════════════════════════════
        // RETRY CASE
        // ══════════════════════════════════════
        return new AIRecognitionResultResponse(
            Received: true,
            Status: AIRecognitionStatus.Retry,
            NextStep: "RetryCapture",
            MemberId: null,
            Message: "AI not confident - retry required",
            Commands: new DeviceCommandsDto(
                ActivateCamera: true,
                DelaySeconds: 3
            )
        );
    }
    // Verfiy fingerpprint
    public async Task<FingerprintVerificationResponse> VerifyFingerprintAsync(
    FingerprintVerificationRequest request, CancellationToken ct = default)
    {
        var session = await _sessionService.GetByTokenAsync(request.SessionToken, ct);

        if (session is null)
        {
            return new FingerprintVerificationResponse(
                Success: false,
                Status: FingerprintStatus.InvalidSession,
                AccessGranted: false,
                MemberName: null,
                AttemptNumber: 0,
                RemainingAttempts: 0,
                Emergency: false,
                Commands: new DeviceCommandsDto());
        }

        // Cross-validation
        if (session.MemberId != request.MemberId)
        {
            await _emergencyService.TriggerEmergencyAsync(
                session.GateId,
                EmergencyType.FingerprintFailed,
                "Fingerprint member mismatch",
                sessionId: session.Id,
                ct: ct);

            await _deviceCommandService.SendActivateBuzzerAsync(
                session.GateId, 30, session.Id, ct);

            await _sessionService.CompleteSessionAsync(
                session.Id,
                SessionResult.DeniedFingerprint,
                "Member mismatch",
                ct);

            return new FingerprintVerificationResponse(
                Success: false,
                Status: FingerprintStatus.MemberMismatch,
                AccessGranted: false,
                MemberName: null,
                AttemptNumber: MaxAttempts,
                RemainingAttempts: 0,
                Emergency: true,
                Commands: new DeviceCommandsDto(
                    ActivateBuzzer: true,
                    BuzzerDurationSeconds: 30));
        }

        var isMatch = await _fingerprintService.VerifyAsync(
            request.MemberId,
            request.FingerprintTemplate,
            ct);

        session.FingerprintAttemptCount++;

        if (isMatch)
        {
            session.FingerprintPassed = true;
            session.FingerprintValidatedAt = DateTime.UtcNow;

            await _sessionService.CompleteSessionAsync(
                session.Id,
                SessionResult.Granted,
                ct: ct);

            await _deviceCommandService.SendOpenGateAsync(session.GateId,sessionId: session.Id, ct: ct);

            var member = await _context.Members.FindAsync([request.MemberId], ct);

            return new FingerprintVerificationResponse(
                Success: true,
                Status: FingerprintStatus.Success,
                AccessGranted: true,
                MemberName: member != null ? $"{member.FName} {member.LName}" : null,
                AttemptNumber: session.FingerprintAttemptCount,
                RemainingAttempts: 0,
                Emergency: false,
                Commands: new DeviceCommandsDto(OpenGate: true));
        }

        // wrong fingerprint
        if (session.FingerprintAttemptCount >= MaxAttempts)
        {
            await _emergencyService.TriggerEmergencyAsync(
                session.GateId,
                EmergencyType.FingerprintFailed,
                "Max attempts reached",
                sessionId: session.Id,
                ct: ct);

            await _deviceCommandService.SendActivateBuzzerAsync(
                session.GateId, 30, session.Id, ct);

            await _sessionService.CompleteSessionAsync(
                session.Id,
                SessionResult.DeniedFingerprint,
                "Max attempts reached",
                ct);

            return new FingerprintVerificationResponse(
                Success: false,
                Status: FingerprintStatus.MaxAttemptsReached,
                AccessGranted: false,
                MemberName: null,
                AttemptNumber: MaxAttempts,
                RemainingAttempts: 0,
                Emergency: true,
                Commands: new DeviceCommandsDto(
                    ActivateBuzzer: true,
                    BuzzerDurationSeconds: 30));
        }

        return new FingerprintVerificationResponse(
            Success: false,
            Status: FingerprintStatus.WrongFingerprint,
            AccessGranted: false,
            MemberName: null,
            AttemptNumber: session.FingerprintAttemptCount,
            RemainingAttempts: MaxAttempts - session.FingerprintAttemptCount,
            Emergency: false,
            Commands: new DeviceCommandsDto());
    }

    // ══════════════════════════════════════════════
    //  LASER INTRUSION HANDLER
    // ══════════════════════════════════════════════
    public async Task<LaserIntrusionResponse> HandleLaserIntrusionAsync(
        LaserIntrusionRequest request, CancellationToken ct = default)
    {
        var emergency = await _emergencyService.TriggerEmergencyAsync(
            request.GateId, EmergencyType.LaserIntrusion,
            $"Intrusion detected at Gate {request.GateId}",
            EmergencySeverity.Critical, ct: ct);

        await _deviceCommandService.SendActivateBuzzerAsync(request.GateId, 30, ct: ct);

        // Create unauthorized access log
        var gate = await _context.Gates.Include(g => g.Client)
            .FirstOrDefaultAsync(g => g.Id == request.GateId, ct);

        if (gate is not null)
        {
            _context.AccessLogs.Add(new AccessLog
            {
                GateId = request.GateId,
                IsAuthorized = false,
                AccessMethod = AccessMethod.Laser,
                CreatedById = gate.Client.UserId
            });
            await _context.SaveChangesAsync(ct);
        }

        await _notificationService.SendEmergencyAlertAsync(
            request.GateId, EmergencyType.LaserIntrusion,
            $"Intrusion detected at Gate {request.GateId}!", ct);

        _logger.LogCritical("LASER INTRUSION at Gate {GateId}, Device {DeviceId}",
            request.GateId, request.DeviceId);

        return new LaserIntrusionResponse(
            Received: true, EmergencyId: emergency.Id,
            Commands: new DeviceCommandsDto(ActivateBuzzer: true, BuzzerDurationSeconds: 30));
    }

    private string BuildImageUrl(string? imageName)
    {
        if (string.IsNullOrEmpty(imageName))
            return $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext!.Request.Host}/Images/default.png";

        var request = _httpContextAccessor.HttpContext!.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        return $"{baseUrl}/Images/{imageName}";
    }
}
