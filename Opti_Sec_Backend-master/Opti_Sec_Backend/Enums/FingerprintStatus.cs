namespace Opti_Sec_Backend.Enums;

public enum FingerprintStatus
{
    Success,
    InvalidSession,
    MemberMismatch,
    WrongFingerprint,
    MaxAttemptsReached
}