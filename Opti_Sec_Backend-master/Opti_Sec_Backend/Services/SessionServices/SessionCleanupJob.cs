using Hangfire;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Services.SessionServices;

public class SessionCleanupJob(
    ApplicationDbContext context,
    ILogger<SessionCleanupJob> logger)
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<SessionCleanupJob> _logger = logger;

    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Sweeps the database for gate sessions that have been in a non-terminal state
    /// for longer than 5 minutes and marks them as Expired.
    /// Scheduled as a Hangfire recurring job running every minute.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task ExpireStaleSessionsAsync()
    {
        var cutoff = DateTime.UtcNow - SessionTimeout;

        // Find all sessions that are still pending (non-terminal) and started before the cutoff
        var staleSessions = await _context.GateSessions
            .Where(s => s.Result == SessionResult.Pending
                     && s.StartedAt < cutoff)
            .ToListAsync();

        if (staleSessions.Count == 0)
            return;

        foreach (var session in staleSessions)
        {
            session.Status = SessionStatus.Expired;
            session.Result = SessionResult.Expired;
            session.CompletedAt = DateTime.UtcNow;
            session.FailureReason = "Session expired due to inactivity (exceeded 5-minute timeout)";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Session cleanup: Expired {Count} stale gate session(s) older than {Timeout} minutes",
            staleSessions.Count,
            SessionTimeout.TotalMinutes);
    }
}
