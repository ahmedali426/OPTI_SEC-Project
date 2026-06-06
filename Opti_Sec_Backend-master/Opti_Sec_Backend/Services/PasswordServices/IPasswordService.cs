using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Services.PasswordServices;

public interface IPasswordService
{
    Task<(PasswordStatus status, int attemptNumber)> ValidateAsync(int gateId, string password, string deviceId, CancellationToken ct = default);
    Task ResetFailedAttemptsAsync(int gateId, CancellationToken ct = default);
    Task<int> GetRecentFailedAttemptsAsync(int gateId, TimeSpan window, CancellationToken ct = default);
}
