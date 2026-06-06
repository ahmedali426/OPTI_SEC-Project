using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Services.SessionServices;

public class SessionService(ApplicationDbContext context) : ISessionService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<GateSession> CreateSessionAsync(int gateId, string deviceId, CancellationToken ct = default)
    {
        var session = new GateSession
        {
            GateId = gateId,
            DeviceId = deviceId,
            Status = SessionStatus.PasswordPassed,
            CurrentStep = SessionStep.AI
        };

        _context.GateSessions.Add(session);
        await _context.SaveChangesAsync(ct);

        return session;
    }

    public async Task<GateSession?> GetByTokenAsync(Guid sessionToken, CancellationToken ct = default)
    {
        return await _context.GateSessions
            .Include(s => s.Gate)
                .ThenInclude(g => g.Client)
            .Include(s => s.Member)
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken, ct);
    }

    public async Task UpdateSessionStepAsync(int sessionId, SessionStep step, SessionStatus status, CancellationToken ct = default)
    {
        var session = await _context.GateSessions.FindAsync([sessionId], ct);
        if (session is null) return;

        session.CurrentStep = step;
        session.Status = status;

        if (step == SessionStep.AI)
            session.PasswordValidatedAt = DateTime.UtcNow;
        else if (step == SessionStep.Fingerprint)
            session.AIValidatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }

    public async Task CompleteSessionAsync(int sessionId, SessionResult result, string? failureReason = null, CancellationToken ct = default)
    {
        var session = await _context.GateSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.Result != SessionResult.Expired && x.Result != SessionResult.Granted, ct);
        if (session is null) return;

        session.Result = result;
        session.CompletedAt = DateTime.UtcNow;
        session.FailureReason = failureReason;
        session.Status = result == SessionResult.Granted
            ? SessionStatus.Completed
            : result == SessionResult.EmergencyTriggered
                ? SessionStatus.Emergency
                : SessionStatus.Failed;

        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<int>> GetClientGateIdsAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Gates
            .Where(g => g.Client.UserId == userId && !g.IsDeleted)
            .Select(g => g.Id)
            .ToListAsync(ct);
    }
}
