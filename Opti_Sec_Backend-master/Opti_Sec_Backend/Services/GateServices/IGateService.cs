using Opti_Sec_Backend.Contracts.Gates;

namespace Opti_Sec_Backend.Services.GateServices;

public interface IGateService
{
    Task<Result<GateResponse>> CreateAsync(string userId, GateRequest request, CancellationToken cancellationToken);
    Task<Result<IEnumerable<GateResponseAll>>> GetAllAsync(string userId, CancellationToken cancellationToken);
    Task<Result<GateResponse>> GetByIdAsync(string userId, int id, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(string userId, int id, GateRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(string userId, int id, CancellationToken cancellationToken);
    Task<Result<int>> CountGatesAsync(string userId,CancellationToken cancellationToken);
}
