using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Abstractions;
using Opti_Sec_Backend.Contracts.Mobile;
using Opti_Sec_Backend.Extensions;
using Opti_Sec_Backend.Persistence;
using Opti_Sec_Backend.Services.DeviceCommandServices;
using Opti_Sec_Backend.Services.EmergencyServices;
using Opti_Sec_Backend.Services.NotificationServices;
using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client")]
public class MobileCommandsController(
    ApplicationDbContext context,
    IDeviceCommandService deviceCommandService,
    IEmergencyService emergencyService,
    INotificationService notificationService) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDeviceCommandService _deviceCommandService = deviceCommandService;
    private readonly IEmergencyService _emergencyService = emergencyService;
    private readonly INotificationService _notificationService = notificationService;

    /// <summary>
    /// Get real-time status of all gates belonging to the current user.
    /// </summary>
    [HttpGet("gates/status")]
    public async Task<IActionResult> GetGatesStatus(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var gates = await _context.Gates
            .Where(g => g.Client.UserId == userId && !g.IsDeleted)
            .Select(g => new GateStatusResponse(
                g.Id,
                g.Name,
                g.Status.ToString(),
                g.GateSessions
                    .Where(s => s.Result == SessionResult.Pending)
                    .OrderByDescending(s => s.StartedAt)
                    .Select(s => s.SessionToken.ToString())
                    .FirstOrDefault(),
                g.DeviceCommands
                    .Any(dc => dc.Type == CommandType.ActivateBuzzer
                            && dc.Status != CommandStatus.Acknowledged),
                g.EmergencyEvents.Count(e => !e.IsResolved),
                null
            ))
            .ToListAsync(ct);

        return Ok(gates);
    }

    /// <summary>
    /// Get session history for all gates belonging to the current user.
    /// </summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessionHistory(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = User.GetUserId();

        var sessions = await _context.GateSessions
            .Where(s => s.Gate.Client.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SessionHistoryResponse(
                s.Id,
                s.SessionToken,
                s.Status.ToString(),
                s.Result.ToString(),
                s.Member != null ? $"{s.Member.FName} {s.Member.LName}" : null,
                s.IsSilentAlarm,
                s.StartedAt,
                s.CompletedAt,
                s.PasswordPassed,
                s.AIPassed,
                s.FingerprintPassed
            ))
            .ToListAsync(ct);

        return Ok(sessions);
    }

    /// <summary>
    /// Get all emergency events for the current user's gates.
    /// </summary>
    [HttpGet("emergencies")]
    public async Task<IActionResult> GetEmergencies(
        [FromQuery] bool activeOnly = false, CancellationToken ct = default)
    {
        var userId = User.GetUserId();

        var query = _context.EmergencyEvents
            .Include(e => e.Gate)
            .Where(e => e.Gate.Client.UserId == userId);

        if (activeOnly)
            query = query.Where(e => !e.IsResolved);

        var emergencies = await query
            .OrderByDescending(e => e.OccurredAt)
            .Select(e => new EmergencyResponse(
                e.Id,
                e.GateId,
                e.Gate.Name,
                e.Type.ToString(),
                e.Severity.ToString(),
                e.Description,
                e.BuzzerActivated,
                e.OccurredAt,
                e.IsResolved,
                e.ResolvedAt
            ))
            .ToListAsync(ct);

        return Ok(emergencies);
    }

    /// <summary>
    /// Resolve an emergency event and optionally stop the buzzer.
    /// </summary>
    [HttpPost("emergencies/{emergencyId}/resolve")]
    public async Task<IActionResult> ResolveEmergency(int emergencyId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _emergencyService.ResolveEmergencyAsync(emergencyId, userId!, ct);

        if (!result.IsSuccess)
            return result.ToProblem();

        return Ok(new { resolved = true });
    }

    /// <summary>
    /// Stop the buzzer at a specific gate.
    /// </summary>
    [HttpPost("gates/{gateId}/stop-buzzer")]
    public async Task<IActionResult> StopBuzzer(int gateId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _deviceCommandService.SendStopBuzzerAsync(gateId, userId!, ct);

        await _notificationService.SendAsync(
            NotificationType.BuzzerStopped, gateId,
            "Buzzer Stopped", $"Buzzer stopped at Gate {gateId} by user",
            ct: ct);

        return Ok(new { stopped = true });
    }

    /// <summary>
    /// Manually open a gate from the mobile app.
    /// </summary>
    [HttpPost("gates/{gateId}/open")]
    public async Task<IActionResult> OpenGate(int gateId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _deviceCommandService.SendOpenGateAsync(gateId, userId, ct: ct);

        await _notificationService.SendAsync(
            NotificationType.GateOpened, gateId,
            "Gate Opened", $"Gate {gateId} opened remotely",
            ct: ct);

        return Ok(new { opened = true });
    }

    /// <summary>
    /// Register or update the FCM push notification token.
    /// </summary>
    [HttpPost("register-fcm-token")]
    public async Task<IActionResult> RegisterFcmToken(
        [FromBody] RegisterFcmTokenRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await _context.Users.FindAsync(new object[] { userId! }, ct);
        if (user is null)
            return NotFound();

        user.FcmToken = request.FcmToken;
        await _context.SaveChangesAsync(ct);

        return Ok(new { registered = true });
    }

    /// <summary>
    /// Get notification history for the current user.
    /// </summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = User.GetUserId();

        var notifications = await _context.Notifications
            .Include(n => n.Gate)
            .Where(n => n.RecipientUserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationHistoryResponse(
                n.Id,
                n.Type.ToString(),
                n.Priority.ToString(),
                n.Title,
                n.Body,
                n.IsSent,
                n.CreatedAt,
                n.SentAt,
                n.GateId,
                n.Gate != null ? n.Gate.Name : null
            ))
            .ToListAsync(ct);

        return Ok(notifications);
    }
}
