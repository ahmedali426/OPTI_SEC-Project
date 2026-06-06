using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Contracts.Device;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Services.DeviceCommandServices;

public class DeviceCommandService(
    ApplicationDbContext context,
    ILogger<DeviceCommandService> logger) : IDeviceCommandService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<DeviceCommandService> _logger = logger;

    public async Task<DeviceCommand> SendOpenGateAsync(int gateId, string? userId = null, int? sessionId = null, CancellationToken ct = default)
    {
        return await CreateAndSendCommandAsync(gateId, CommandType.OpenGate,
            new { openGate = true },
            userId != null ? CommandSource.MobileApp : CommandSource.Backend,
            userId, sessionId, ct);
    }

    public async Task<DeviceCommand> SendActivateBuzzerAsync(int gateId, int durationSeconds = 30, int? sessionId = null, CancellationToken ct = default)
    {
        return await CreateAndSendCommandAsync(gateId, CommandType.ActivateBuzzer,
            new { activateBuzzer = true, buzzerDuration = durationSeconds },
            CommandSource.Backend, null, sessionId, ct);
    }

    public async Task<DeviceCommand> SendStopBuzzerAsync(int gateId, string userId, CancellationToken ct = default)
    {
        return await CreateAndSendCommandAsync(gateId, CommandType.StopBuzzer,
            new { stopBuzzer = true },
            CommandSource.MobileApp, userId, null, ct);
    }

    public async Task<Result> AcknowledgeCommandAsync(int commandId, CancellationToken ct = default)
    {
        var command = await _context.DeviceCommands.FindAsync([commandId], ct);
        if (command is null)
            return Result.Failure(DeviceErrors.CommandNotFound);

        command.Status = CommandStatus.Acknowledged;
        command.AcknowledgedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<IEnumerable<PendingCommandResponse>> GetPendingCommandsAsync(int gateId, CancellationToken ct = default)
    {
        // Fetch all commands that the device has not yet acknowledged
        var pendingCommands = await _context.DeviceCommands
            .Where(c => c.GateId == gateId
                     && c.Status != CommandStatus.Acknowledged
                     && c.Status != CommandStatus.Failed)
            .OrderBy(c => c.IssuedAt)
            .ToListAsync(ct);

        // Mark any Pending commands as Sent now that the device is picking them up
        foreach (var cmd in pendingCommands.Where(c => c.Status == CommandStatus.Pending))
        {
            cmd.Status = CommandStatus.Sent;
        }

        if (pendingCommands.Any(c => c.Status == CommandStatus.Sent))
        {
            await _context.SaveChangesAsync(ct);
        }

        return pendingCommands.Select(c => new PendingCommandResponse(
            c.Id,
            c.Type.ToString(),
            c.PayloadJson,
            c.Source.ToString(),
            c.IssuedAt
        ));
    }

    private async Task<DeviceCommand> CreateAndSendCommandAsync(int gateId, CommandType type,
        object payload, CommandSource source, string? userId, int? sessionId, CancellationToken ct)
    {
        var command = new DeviceCommand
        {
            GateId = gateId,
            Type = type,
            PayloadJson = JsonSerializer.Serialize(payload),
            Source = source,
            IssuedByUserId = userId,
            GateSessionId = sessionId,
            Status = CommandStatus.Sent
        };

        _context.DeviceCommands.Add(command);
        await _context.SaveChangesAsync(ct);

        // TODO: Write to Firebase Realtime Database when Firebase Admin SDK is configured
        // await _firebase.Child($"gates/gate_{gateId}/commands").PatchAsync(payload);

        _logger.LogInformation("Device command sent: {Type} to Gate {GateId}, CommandId: {CommandId}",
            type, gateId, command.Id);

        return command;
    }
}
