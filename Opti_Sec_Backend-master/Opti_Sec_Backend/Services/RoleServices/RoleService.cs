using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Contracts.Roles;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Services.RoleServices;

public class RoleService(RoleManager<ApplicationRole> roleManager) : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    public async Task<Result<RoleDetailResponse>> AddAsync(RoleRequest request,CancellationToken cancellationToken = default)
    {
        var roleIsExists = await _roleManager.RoleExistsAsync(request.Name);

        if (roleIsExists)
            return Result.Failure<RoleDetailResponse>(RoleErrors.DuplicatedRole);

        var role = new ApplicationRole
        {
            Name = request.Name,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            var response = new RoleDetailResponse(role.Id, role.Name, role.IsDeleted);

            return Result.Success(response);
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisabled = false)
    {
        var roles = await _roleManager
           .Roles
           .Where(x => !x.IsDefault && (!x.IsDeleted || (includeDisabled.HasValue && includeDisabled.Value)))
           .Select(x => new RoleResponse(
               x.Id,
               x.Name!,
               x.IsDefault))
           .ToListAsync();

        return roles;
    }

    public async Task<Result<RoleDetailResponse>> GetAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if(role is null)
            return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

        var response = new RoleDetailResponse(role.Id,role.Name!,role.IsDeleted);

        return Result.Success<RoleDetailResponse>(response);

    }

    public async Task<Result> UpdateAsync(string id, RoleRequest request,CancellationToken cancellationToken = default)
    {

        var roleIsExists = await _roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id);

        if (roleIsExists)
            return Result.Failure<RoleDetailResponse>(RoleErrors.DuplicatedRole);

        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

        
        role.Name = request.Name;

        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ToggleAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
            return Result.Failure(RoleErrors.RoleNotFound);

        role.IsDeleted = !role.IsDeleted;

        await _roleManager.UpdateAsync(role);

        return Result.Success();
    }
}

