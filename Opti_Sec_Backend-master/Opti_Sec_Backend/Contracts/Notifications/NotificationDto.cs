namespace Opti_Sec_Backend.Contracts.Notifications;

public record NotificationDto(
    int Id,
    string Type,
    string Priority,
    string Title,
    string Body,
    DateTime CreatedAt,
    int? GateId
);
