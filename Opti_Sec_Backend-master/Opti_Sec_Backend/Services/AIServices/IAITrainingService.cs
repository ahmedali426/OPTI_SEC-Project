namespace Opti_Sec_Backend.Services.AIServices;

public interface IAITrainingService
{
    /// <summary>
    /// Updates member training status to Pending and enqueues a background task for AI training.
    /// Non-blocking, fire-and-forget/background queued.
    /// </summary>
    Task TriggerTrainingAsync(int memberId, string imageUrl, CancellationToken ct = default);

    /// <summary>
    /// Processes the training request inside the background job.
    /// </summary>
    Task ProcessTrainingRequestAsync(int memberId, string imageUrl, CancellationToken ct = default);
}
