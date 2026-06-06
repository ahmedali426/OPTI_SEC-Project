using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Entities;

public class Member : AuditableEntity
{
    public int Id { get; set; }

    public string FName { get; set; } = string.Empty;

    public string LName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string? FingerprintTemplate { get; set; }

    public string? FaceImageUrl { get; set; }

    // Storing the face embedding as a JSON string or a base64-encoded string, depending on your implementation
    public string? FaceEmbedding { get; set; }

    public AITrainingStatus AITrainingStatus { get; set; } = AITrainingStatus.NotTrained;

    public DateTime? TrainingStartedAt { get; set; }
    public string? AITrainingError { get; set; }
    public DateTime? TrainingCompletedAt { get; set; }
    public DateTime? LastTrainedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int ClientId { get; set; }

    public Client Client { get; set; } = default!;

    public ICollection<AccessLog> AccessLogs { get; set; } = [];
    public ICollection<GateSession> GateSessions { get; set; } = [];

    public ICollection<AIValidationLog> AIValidationLogs { get; set; } = [];

    public ICollection<FingerprintValidationLog> FingerprintValidationLogs { get; set; } = [];
}
