namespace Opti_Sec_Backend.Services.FingerprintServices;

public interface IFingerprintService
{
    Task<bool> VerifyAsync(int expectedMemberId, string fingerprintTemplate, CancellationToken ct = default);
}
