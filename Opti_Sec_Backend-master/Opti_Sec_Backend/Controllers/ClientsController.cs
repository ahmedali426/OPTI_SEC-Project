using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Opti_Sec_Backend.Contracts.Clients;
using Opti_Sec_Backend.Services.ClientServices;

namespace Opti_Sec_Backend.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    private readonly IClientService _clientService = clientService;

    [HttpPost("")]
    public async Task<IActionResult> Create([FromForm] ClientRequest request,CancellationToken cancellationToken)
    {
        var result = await _clientService.CreateAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search,CancellationToken cancellationToken)
    {
        var result = await _clientService.GetAllAsync(search,cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [AllowAnonymous]

    [HttpGet("all-clients-ai")]
    public async Task<IActionResult> GetAllForAI(CancellationToken cancellationToken)
    {
        var result = await _clientService.GetAllForAIAsync(cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _clientService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromForm] UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _clientService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
    [HttpGet("clients-count")]
    public async Task<IActionResult> Count(CancellationToken cancellationToken)
    {
        var response = await _clientService.CountAsync(cancellationToken);

        return Ok(response.Value);
    }

}
