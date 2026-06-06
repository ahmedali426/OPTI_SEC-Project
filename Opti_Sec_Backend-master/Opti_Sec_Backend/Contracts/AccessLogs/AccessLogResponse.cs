namespace Opti_Sec_Backend.Contracts.AccessLogs;

public record AccessLogResponse(
    int Id,
    string Name,
    string GateName,
    string ImageUrl,
    DateOnly DateOnly,
    TimeOnly TimeOnly
);