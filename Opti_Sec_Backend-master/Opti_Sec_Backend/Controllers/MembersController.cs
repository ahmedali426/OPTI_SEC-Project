using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Opti_Sec_Backend.Contracts.Members;
using Opti_Sec_Backend.Extensions;
using Opti_Sec_Backend.Services.MemberServices;

namespace Opti_Sec_Backend.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles ="Client")]
public class MembersController(IMemberService memberService) : ControllerBase
{
    private readonly IMemberService _memberService = memberService;

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] MemberRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _memberService.CreateAsync(userId!,request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _memberService.GetAllAsync(search,userId!, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [AllowAnonymous]
    [HttpGet("all-members-ai")]
    public async Task<IActionResult> GetAllForAI(CancellationToken cancellationToken)
    {
        var result = await _memberService.GetAllForAIAsync(cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _memberService.GetByIdAsync(userId!, id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromForm] MemberUpdateRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _memberService.UpdateAsync(userId!, id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _memberService.DeleteAsync(userId!, id, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("member-count")]
    public async Task<IActionResult> Count(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await _memberService.CountAsync(userId!,cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : response.ToProblem();
    }

    [HttpPost("set-fingerprint")]
    [AllowAnonymous]
    public async Task<IActionResult> SetFingerprint([FromBody] SetFingerPrintRequest request, CancellationToken cancellationToken)
    {
        var result = await _memberService.SetFingerprintAsync(request, cancellationToken);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}
