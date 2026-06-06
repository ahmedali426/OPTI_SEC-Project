using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Services.EmergencyServices;

public interface IEmergencyService
{
    Task<EmergencyEvent> TriggerEmergencyAsync(int gateId, EmergencyType type, string description,
        EmergencySeverity severity = EmergencySeverity.Critical, int? sessionId = null, CancellationToken ct = default);
    Task<Result> ResolveEmergencyAsync(int emergencyId, string userId, CancellationToken ct = default);
}
