using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Opti_Sec_Backend.Contracts.AccessLogs;
using Opti_Sec_Backend.Extensions;
using Opti_Sec_Backend.Services.AccessLogServices;

namespace Opti_Sec_Backend.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client")]
public class AccessLogsController(IAccessLogService accessLogService) : ControllerBase
{
    private readonly IAccessLogService _accessLogService = accessLogService;

    [AllowAnonymous]
    [HttpPost("")]
    public async Task<IActionResult> CheckOrCreate([FromForm] AccessLogRequest request,CancellationToken cancellationToken)
    {
        var response = await _accessLogService.CheckOrCreateAsync(request, cancellationToken);

        return response.IsSuccess ? Ok() : response.ToProblem();
    }
   
    [HttpGet("authorized")]
    public async Task<IActionResult> AutorizedAccessLog(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var response = await _accessLogService.GetAuthorizedAccessLogAsync(userId,cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : response.ToProblem();
    }

    [HttpGet("unauthorized")]
    public async Task<IActionResult> UnAutorizedAccessLog(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var response = await _accessLogService.GetUnAuthorizedAccessLogAsync(userId, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : response.ToProblem();
    }
}
