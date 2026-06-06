namespace Opti_Sec_Backend.Enums;

public enum SessionStatus
{
    PasswordPending = 0,
    PasswordPassed = 10,
    AIPending = 20,
    AIPassed = 30,
    FingerprintPending = 40,
    FingerprintPassed = 50,
    Completed = 100,
    Failed = -1,
    Emergency = -2,
    Expired = -3
}
