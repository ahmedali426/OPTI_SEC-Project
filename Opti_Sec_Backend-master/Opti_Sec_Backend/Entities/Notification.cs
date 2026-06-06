using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Entities;

public class Notification
{
    public int Id { get; set; }

    public string RecipientUserId { get; set; } = string.Empty;
    public ApplicationUser Recipient { get; set; } = default!;

    public int? GateId { get; set; }
    public Gate? Gate { get; set; }

    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? DataJson { get; set; }

    public bool IsSent { get; set; }

    // For retry logic if the message fails to send, we can track how many times we've attempted to send it
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }

    public int? GateSessionId { get; set; }
    public GateSession? GateSession { get; set; }
}
