using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Services.SessionServices;

public interface ISessionService
{
    Task<GateSession> CreateSessionAsync(int gateId, string deviceId, CancellationToken ct = default);
    Task<GateSession?> GetByTokenAsync(Guid sessionToken, CancellationToken ct = default);
    Task UpdateSessionStepAsync(int sessionId, SessionStep step, SessionStatus status, CancellationToken ct = default);
    Task CompleteSessionAsync(int sessionId, SessionResult result, string? failureReason = null, CancellationToken ct = default);
    Task<IEnumerable<int>> GetClientGateIdsAsync(string userId, CancellationToken ct = default);
}
