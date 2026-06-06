namespace Opti_Sec_Backend.Contracts.AccessLogs;

public record AccessLogRequest(
    int GateId,
    string? UserName,
    string? FingerprintTemplate,
    IFormFile? Image
);
