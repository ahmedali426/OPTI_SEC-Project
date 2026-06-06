namespace Opti_Sec_Backend.Contracts.Mobile;

// This record represents the response for a session history request in the mobile application.
// It contains details about a specific session, including its status, result,
// and various attributes related to the session's execution and outcomes.
public record SessionHistoryResponse(
    int Id,
    Guid SessionToken,
    // The status of the session, which could indicate whether it is pending, in progress, completed, etc.
    string Status,
    // The result of the session, which could indicate success, failure, or other outcomes.
    string Result,
    string? MemberName,
    bool IsSilentAlarm,
    DateTime StartedAt,
    DateTime? CompletedAt,
    bool PasswordPassed,
    bool AIPassed,
    bool FingerprintPassed
);
