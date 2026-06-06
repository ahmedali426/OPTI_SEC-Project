using Microsoft.AspNetCore.Mvc;
using Opti_Sec_Backend.Contracts.Device;
using Opti_Sec_Backend.Services.DeviceCommandServices;
using Opti_Sec_Backend.Services.SecurityWorkflow;

namespace Opti_Sec_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeviceController(
    IGateAccessOrchestrator orchestrator,
    IDeviceCommandService deviceCommandService) : ControllerBase
{
    private readonly IGateAccessOrchestrator _orchestrator = orchestrator;
    private readonly IDeviceCommandService _deviceCommandService = deviceCommandService;

    /// <summary>
    /// STEP 1: Device sends password for validation.
    /// Returns session token + next step commands.
    /// </summary>
    [HttpPost("validate-password")]
    public async Task<IActionResult> ValidatePassword(
        [FromBody] PasswordValidationRequest request, CancellationToken ct)
    {
        var result = await _orchestrator.ValidatePasswordAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// STEP 3: Device sends fingerprint for verification.
    /// Called after AI recognition confirms a member.
    /// </summary>
    [HttpPost("verify-fingerprint")]
    public async Task<IActionResult> VerifyFingerprint(
        [FromBody] FingerprintVerificationRequest request, CancellationToken ct)
    {
        var result = await _orchestrator.VerifyFingerprintAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Laser sensor triggered — intrusion detected.
    /// </summary>
    [HttpPost("laser-intrusion")]
    public async Task<IActionResult> LaserIntrusion(
        [FromBody] LaserIntrusionRequest request, CancellationToken ct)
    {
        var result = await _orchestrator.HandleLaserIntrusionAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Device polls for pending commands that need to be executed.
    /// The device should call this periodically (e.g., every 2-3 seconds) to pick up
    /// commands issued by the mobile app or the backend (open gate, stop buzzer, etc.).
    /// Commands transition from Pending → Sent when fetched by this endpoint.
    /// After execution, the device should call acknowledge-command for each command.
    /// </summary>
    [HttpGet("pending-commands")]
    public async Task<IActionResult> GetPendingCommands(
        [FromQuery] int gateId, CancellationToken ct)
    {
        var commands = await _deviceCommandService.GetPendingCommandsAsync(gateId, ct);
        return Ok(commands);
    }

    /// <summary>
    /// Periodic heartbeat from embedded device.
    /// </summary>
    [HttpPost("heartbeat")]
    public IActionResult Heartbeat([FromBody] DeviceHeartbeatRequest request)
    {
        return Ok(new { received = true, serverTime = DateTime.UtcNow });
    }

    /// <summary>
    /// Device acknowledges receipt of a command.
    /// </summary>
    [HttpPost("acknowledge-command")]
    public async Task<IActionResult> AcknowledgeCommand(
        [FromBody] CommandAcknowledgeRequest request, CancellationToken ct)
    {
        var result = await _deviceCommandService.AcknowledgeCommandAsync(request.CommandId, ct);
        return result.IsSuccess
            ? Ok(new { acknowledged = true })
            : result.ToProblem();
    }
}
