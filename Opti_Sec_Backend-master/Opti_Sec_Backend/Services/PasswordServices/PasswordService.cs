using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Services.PasswordServices;

public class PasswordService(ApplicationDbContext context) : IPasswordService
{
    private readonly ApplicationDbContext _context = context;
    private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(5);

    public async Task<(PasswordStatus status, int attemptNumber)> ValidateAsync(
        int gateId, string password, string deviceId, CancellationToken ct = default)
    {
        var gate = await _context.Gates
            .FirstOrDefaultAsync(g => g.Id == gateId && g.DeviceId == deviceId && !g.IsDeleted, ct);

        if (gate is null)
        {
            return (PasswordStatus.GateNotFound, 0);
        }

        var passwordHash = password;

        // Check silent alarm first
        if (gate.SilentAlarmHash == passwordHash)
        {
            gate.FailedAttemptsCount = 0;
            gate.LastFailedAttemptAt = null;
            await _context.SaveChangesAsync(ct);

            await StoreAttemptAsync(gateId, deviceId, passwordHash, PasswordStatus.SilentAlarm, 0, ct);
            return (PasswordStatus.SilentAlarm, 0);
        }

        // Check normal password
        if (gate.PasswordHash == passwordHash)
        {
            gate.FailedAttemptsCount = 0;
            gate.LastFailedAttemptAt = null;
            await _context.SaveChangesAsync(ct);

            await StoreAttemptAsync(gateId, deviceId, passwordHash, PasswordStatus.Correct, 0, ct);
            return (PasswordStatus.Correct, 0);
        }

        // Wrong password — count recent failed attempts
        // Wrong password
        if (gate.LastFailedAttemptAt is null ||
           DateTime.UtcNow - gate.LastFailedAttemptAt > AttemptWindow)
        {
            gate.FailedAttemptsCount = 0;
        }

        // Increment failed attempts
        gate.FailedAttemptsCount++;
        gate.LastFailedAttemptAt = DateTime.UtcNow;

        var attemptNumber = gate.FailedAttemptsCount;
        var triggersEmergency = attemptNumber >= gate.MaxFailedAttempts;

        if (triggersEmergency)
        {
            gate.FailedAttemptsCount = 0;
            gate.LastFailedAttemptAt = null;
        }

        await _context.SaveChangesAsync(ct);

        await StoreAttemptAsync(
            gateId,
            deviceId,
            passwordHash,
            PasswordStatus.Wrong,
            attemptNumber,
            ct,
            triggersEmergency);

        return (PasswordStatus.Wrong, attemptNumber);
    }

    public async Task ResetFailedAttemptsAsync(int gateId, CancellationToken ct = default)
    {
        // We don't delete records (audit trail), but the window-based counting
        // naturally resets after AttemptWindow expires
        await Task.CompletedTask;
    }

    public async Task<int> GetRecentFailedAttemptsAsync(int gateId, TimeSpan window, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - window;
        return await _context.PasswordAttempts
            .CountAsync(x => x.GateId == gateId
                          && x.Status == PasswordStatus.Wrong
                          && x.AttemptedAt >= cutoff, ct);
    }

    private async Task StoreAttemptAsync(int gateId, string deviceId, string passwordHash,
        PasswordStatus status, int attemptNumber, CancellationToken ct, bool triggeredEmergency = false)
    {
        var attempt = new PasswordAttempt
        {
            GateId = gateId,
            DeviceId = deviceId,
            PasswordHashAttempt = passwordHash,
            Status = status,
            AttemptNumber = attemptNumber,
            TriggeredEmergency = triggeredEmergency
        };

        _context.PasswordAttempts.Add(attempt);
        await _context.SaveChangesAsync(ct);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
