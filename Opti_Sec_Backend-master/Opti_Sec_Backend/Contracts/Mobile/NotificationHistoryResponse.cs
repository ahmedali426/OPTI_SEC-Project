namespace Opti_Sec_Backend.Contracts.Mobile;

// This record represents the response for a notification history item in the mobile application.
public record NotificationHistoryResponse(
    int Id,
    // The type of the notification (e.g., "Alert", "Info", etc.).
    string Type,
    // The priority level of the notification (e.g., "High", "Medium", "Low").
    string Priority,
    string Title,
    string Body,
    // Indicates whether the notification has been sent or not. by the FCM service or not because we use the retry.
    bool IsSent,
    DateTime CreatedAt,
    DateTime? SentAt,
    int? GateId,
    string? GateName
);
