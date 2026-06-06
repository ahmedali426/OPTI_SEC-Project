using Microsoft.AspNetCore.Mvc;
using Opti_Sec_Backend.Contracts.AI;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;
using Opti_Sec_Backend.Services.SecurityWorkflow;

namespace Opti_Sec_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AICallbackController(
    IGateAccessOrchestrator orchestrator,
    ApplicationDbContext context) : ControllerBase
{
    private readonly IGateAccessOrchestrator _orchestrator = orchestrator;
    private readonly ApplicationDbContext _context = context;

    /// <summary>
    /// STEP 2: AI service sends face recognition result.
    /// Callback endpoint for the external AI recognition service.
    /// </summary>
    [HttpPost("recognition-result")]
    public async Task<IActionResult> RecognitionResult(
        [FromForm] AIRecognitionResultRequest request, CancellationToken ct)
    {
        var result = await _orchestrator.ProcessAIResultAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// AI service confirms that a member's face model has been trained/updated.
    /// </summary>
    [HttpPost("training-complete")]
    public async Task<IActionResult> TrainingComplete(
        [FromBody] AITrainingCompleteRequest request, CancellationToken ct)
    {
        var member = await _context.Members.FindAsync([request.MemberId], ct);
        if (member is null)
            return NotFound(new { error = "Member not found" });

        if (request.Success)
        {
            member.AITrainingStatus = AITrainingStatus.Trained;
            member.FaceEmbedding = request.EmbeddingVector;
            member.LastTrainedAt = DateTime.UtcNow;
        }
        else
        {
            member.AITrainingStatus = AITrainingStatus.Failed;
        }

        await _context.SaveChangesAsync(ct);

        return Ok(new { received = true, status = member.AITrainingStatus.ToString() });
    }
}
