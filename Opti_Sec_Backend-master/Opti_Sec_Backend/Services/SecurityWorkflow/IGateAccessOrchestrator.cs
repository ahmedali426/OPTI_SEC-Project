using Opti_Sec_Backend.Contracts.AI;
using Opti_Sec_Backend.Contracts.Device;

namespace Opti_Sec_Backend.Services.SecurityWorkflow;

public interface IGateAccessOrchestrator
{
    Task<PasswordValidationResponse> ValidatePasswordAsync(PasswordValidationRequest request, CancellationToken ct = default);
    Task<AIRecognitionResultResponse> ProcessAIResultAsync(AIRecognitionResultRequest request, CancellationToken ct = default);
    Task<FingerprintVerificationResponse> VerifyFingerprintAsync(FingerprintVerificationRequest request, CancellationToken ct = default);
    Task<LaserIntrusionResponse> HandleLaserIntrusionAsync(LaserIntrusionRequest request, CancellationToken ct = default);
}
