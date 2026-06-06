using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Opti_Sec_Backend.Contracts.Roles;
using Opti_Sec_Backend.Services.RoleServices;

namespace Opti_Sec_Backend.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromQuery] bool includeDisabled)
    {
        var roles = await _roleService.GetAllAsync(includeDisabled);

        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var result = await _roleService.GetAsync(id);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.AddAsync(request, cancellationToken);

        return result.IsSuccess ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        var roles = await _roleService.UpdateAsync(id, request, cancellationToken);

        return roles.IsSuccess ? NoContent() : roles.ToProblem();
    }

    [HttpPut("{id}/Toggle")]
    public async Task<IActionResult> Toggle([FromRoute] string id)
    {
        var roles = await _roleService.ToggleAsync(id);

        return roles.IsSuccess ? NoContent() : roles.ToProblem();
    }
}
