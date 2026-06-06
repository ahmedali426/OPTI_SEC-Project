using System.Net.Http.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Opti_Sec_Backend.Contracts.AI;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;
using Opti_Sec_Backend.Settings;

namespace Opti_Sec_Backend.Services.AIServices;

public class AITrainingService(
    HttpClient httpClient,
    ApplicationDbContext context,
    IOptions<AIServiceSettings> settings,
    ILogger<AITrainingService> logger) : IAITrainingService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ApplicationDbContext _context = context;
    private readonly AIServiceSettings _settings = settings.Value;
    private readonly ILogger<AITrainingService> _logger = logger;

    public async Task TriggerTrainingAsync(int memberId, string imageUrl, CancellationToken ct = default)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.Id == memberId && !x.IsDeleted, ct);

        if (member is null)
        {
            _logger.LogWarning("Cannot trigger AI training: Member with ID {MemberId} not found.", memberId);
            return;
        }

        // Prevent duplicate training requests
        if (member.AITrainingStatus == AITrainingStatus.Training)
        {
            _logger.LogWarning(
                "AI training already in progress for MemberId: {MemberId}",
                memberId);

            return;
        }

        member.AITrainingStatus = AITrainingStatus.Pending;
        member.AITrainingError = null;
        member.TrainingStartedAt = null;
        member.TrainingCompletedAt = null;
       
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Enqueuing background AI training job for MemberId: {MemberId}", memberId);

        // 2. Enqueue the background task via Hangfire to make the process completely non-blocking
        BackgroundJob.Enqueue<IAITrainingService>(service => 
            service.ProcessTrainingRequestAsync(memberId, imageUrl, CancellationToken.None));
    }

    //[Queue("ai-training")]
    //[AutomaticRetry(Attempts = 3)]
    //public async Task ProcessTrainingRequestAsync(int memberId, string imageUrl, CancellationToken ct = default)
    //{
    //    var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == memberId && !m.IsDeleted, ct);
    //    if (member is null)
    //    {
    //        _logger.LogWarning("Background AI training aborted: Member with ID {MemberId} not found or is deleted.", memberId);
    //        return;
    //    }

        

    //    try
    //    {

    //        // 1. Update status to Training in the database
    //        member.AITrainingStatus = AITrainingStatus.Training;
    //        member.TrainingStartedAt = DateTime.UtcNow;
    //        member.AITrainingError = null;

    //        await _context.SaveChangesAsync(ct);

    //        var payload = new AITrainingRequest(
    //            MemberId: member.Id,
    //            Username: member.UserName,
    //            ImageUrl: imageUrl,
    //            Timestamp: DateTime.UtcNow
    //        );

    //        _logger.LogInformation("Starting AI face recognition model training request for MemberId: {MemberId}", memberId);
    //        // 2. Send POST request to external AI service
    //        // Note: HttpClient is injected as a typed/named client and is configured with Polly retry policy in DependencyInjection.cs
    //        var response = await _httpClient.PostAsJsonAsync(_settings.TrainingEndpoint, payload, ct);

    //        if (response.IsSuccessStatusCode)
    //        {
    //            member.AITrainingStatus = AITrainingStatus.Trained;

    //            member.LastTrainedAt = DateTime.UtcNow;

    //            member.TrainingCompletedAt = DateTime.UtcNow;

    //            member.AITrainingError = null;

    //            await _context.SaveChangesAsync(ct);
    //            _logger.LogInformation("AI training request successfully dispatched and received by AI service for MemberId: {MemberId}", memberId);
    //        }
    //        else
    //        {
    //            var responseContent = await response.Content.ReadAsStringAsync(ct);
    //            _logger.LogError("AI service returned error status code: {StatusCode} for MemberId: {MemberId}. Details: {Details}", 
    //                response.StatusCode, memberId, responseContent);

    //            // Update status to Failed in case of API failure
    //            member.AITrainingStatus = AITrainingStatus.Failed;
    //            member.AITrainingError = responseContent;
    //            member.TrainingCompletedAt = DateTime.UtcNow;
    //            await _context.SaveChangesAsync(ct);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unexpected error occurred during AI training request dispatch for MemberId: {MemberId}", memberId);

    //        // Update status to Failed on exception (e.g. timeout, DNS resolution failure, network loss)
    //        member.AITrainingStatus = AITrainingStatus.Failed;
    //        member.AITrainingError = ex.Message;
    //        member.TrainingCompletedAt = DateTime.UtcNow;
    //        await _context.SaveChangesAsync(ct);

    //        throw;
    //    }
    //}
    [Queue("ai-training")]
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessTrainingRequestAsync(int memberId, string imageUrl, CancellationToken ct = default)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == memberId && !m.IsDeleted, ct);
        if (member is null)
        {
            _logger.LogWarning("Background AI training aborted: Member with ID {MemberId} not found or is deleted.", memberId);
            return;
        }

        try
        {
            member.AITrainingStatus = AITrainingStatus.Training;
            member.TrainingStartedAt = DateTime.UtcNow;
            member.AITrainingError = null;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Starting AI face recognition model training request for MemberId: {MemberId}", memberId);

            // 1.  Õ„Ì· «·’Ê—… „‰ «·‹ URL ﬂ‹ Stream
            using var imageHttpClient = new HttpClient(); // »‰” Œœ„ Client ÃœÌœ ⁄‘«‰ ‰Õ„· «·’Ê—…
            var imageResponse = await imageHttpClient.GetAsync(imageUrl, ct);

            if (!imageResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to download image from {imageUrl}");
            }

            await using var imageStream = await imageResponse.Content.ReadAsStreamAsync(ct);

            // 2.  ÃÂÌ“ «·»Ì«‰«  ﬂ‹ MultipartFormData (⁄‘«‰  ÿ«»ﬁ «·‹ Swagger)
            using var formData = new MultipartFormDataContent();

            // ≈÷«›… «·‹ id Ê«·‹ username ﬂ‹ ‰’Ê’ ( √ﬂœ ≈‰ «·√”„«¡ „ÿ«»ﬁ…  „«„« ··‹ Swagger)
            formData.Add(new StringContent(member.Id.ToString()), "id");
            formData.Add(new StringContent(member.UserName), "username");

            // ≈÷«›… „·› «·’Ê—…
            var imageContent = new StreamContent(imageStream);
            // Õœœ ‰Ê⁄ «·„·› ·Ê √„ﬂ‰ («Œ Ì«—Ì »” Ì›÷·)
            // imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg"); 

            // „Â„ Ãœ«: «”„ «·»«—«„Ì — ·«“„ ÌﬂÊ‰ "file" “Ì „« „ﬂ Ê» ›Ì «·‹ Swagger
            string fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
            formData.Add(imageContent, "file", fileName);

            // 3. ≈—”«· «·‹ POST Request
            var response = await _httpClient.PostAsync(_settings.TrainingEndpoint, formData, ct);

            if (response.IsSuccessStatusCode)
            {
                member.AITrainingStatus = AITrainingStatus.Trained;
                member.LastTrainedAt = DateTime.UtcNow;
                member.TrainingCompletedAt = DateTime.UtcNow;
                member.AITrainingError = null;

                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("AI training request successfully dispatched and received by AI service for MemberId: {MemberId}", memberId);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("AI service returned error status code: {StatusCode} for MemberId: {MemberId}. Details: {Details}",
                    response.StatusCode, memberId, responseContent);

                member.AITrainingStatus = AITrainingStatus.Failed;
                member.AITrainingError = responseContent;
                member.TrainingCompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during AI training request dispatch for MemberId: {MemberId}", memberId);

            member.AITrainingStatus = AITrainingStatus.Failed;
            member.AITrainingError = ex.Message;
            member.TrainingCompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            throw;
        }
    }
}
