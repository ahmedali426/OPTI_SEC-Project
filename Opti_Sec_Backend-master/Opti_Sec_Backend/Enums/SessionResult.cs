namespace Opti_Sec_Backend.Enums;

public enum SessionResult
{
    Pending = 0,
    Granted = 1,
    DeniedPassword = 2,
    DeniedAI = 3,
    DeniedFingerprint = 4,
    EmergencyTriggered = 5,
    Expired = 6
}
