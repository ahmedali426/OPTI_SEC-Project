using Opti_Sec_Backend.Contracts.Roles;

namespace Opti_Sec_Backend.Services.RoleServices;

public interface IRoleService
{
    public Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisabled = false);
    public Task<Result<RoleDetailResponse>> GetAsync(string id);

    public Task<Result<RoleDetailResponse>> AddAsync(RoleRequest request,CancellationToken cancellationToken = default);

    public Task<Result> UpdateAsync(string id, RoleRequest request,CancellationToken cancellationToken = default);

    Task<Result> ToggleAsync(string id);
}
