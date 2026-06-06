using Opti_Sec_Backend.Contracts.Clients;

namespace Opti_Sec_Backend.Services.ClientServices;

public interface IClientService
{
    Task<Result<ClientResponse>> CreateAsync(ClientRequest request, CancellationToken cancellationToken);
    Task<Result<IEnumerable<GetAllClientResponse>>> GetAllAsync(string? search,CancellationToken cancellationToken);
    Task<Result<IEnumerable<GetAllClientResponseForAI>>> GetAllForAIAsync(CancellationToken cancellationToken);
    Task<Result<ClientResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, UpdateClientRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<Result<int>> CountAsync(CancellationToken cancellationToken);

}
