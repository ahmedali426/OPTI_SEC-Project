using Opti_Sec_Backend.Contracts.Members;

namespace Opti_Sec_Backend.Services.MemberServices;

public interface IMemberService
{
    Task<Result<MemberResponse>> CreateAsync(string userid,MemberRequest request, CancellationToken cancellationToken);
    Task<Result<IEnumerable<GetAllMemberResponse>>> GetAllAsync(string? search, string userId,CancellationToken cancellationToken);
    Task<Result<IEnumerable<GetAllMemberForAI>>> GetAllForAIAsync(CancellationToken cancellationToken);
    Task<Result<MemberResponse>> GetByIdAsync(string userId, int id, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(string userId, int id, MemberUpdateRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(string userId, int id, CancellationToken cancellationToken);
    Task<Result> SetFingerprintAsync(SetFingerPrintRequest request, CancellationToken cancellationToken);

    Task<Result<int>> CountAsync(string userId, CancellationToken cancellationToken);
}
