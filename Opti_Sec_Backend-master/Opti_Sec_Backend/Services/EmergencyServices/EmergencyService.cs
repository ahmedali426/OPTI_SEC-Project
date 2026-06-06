using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Services.EmergencyServices;

public class EmergencyService(ApplicationDbContext context) : IEmergencyService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<EmergencyEvent> TriggerEmergencyAsync(int gateId, EmergencyType type, string description,
        EmergencySeverity severity = EmergencySeverity.Critical, int? sessionId = null, CancellationToken ct = default)
    {
        var emergency = new EmergencyEvent
        {
            GateId = gateId,
            Type = type,
            Severity = severity,
            Description = description,
            BuzzerActivated = true,
            BuzzerDurationSeconds = 30,
            GateSessionId = sessionId
        };

        _context.EmergencyEvents.Add(emergency);
        await _context.SaveChangesAsync(ct);

        return emergency;
    }

    public async Task<Result> ResolveEmergencyAsync(int emergencyId, string userId, CancellationToken ct = default)
    {
        var emergency = await _context.EmergencyEvents.FindAsync([emergencyId], ct);
        if (emergency is null)
            return Result.Failure(EmergencyErrors.NotFound);

        if (emergency.IsResolved)
            return Result.Failure(EmergencyErrors.AlreadyResolved);

        emergency.IsResolved = true;
        emergency.ResolvedAt = DateTime.UtcNow;
        emergency.ResolvedByUserId = userId;

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
