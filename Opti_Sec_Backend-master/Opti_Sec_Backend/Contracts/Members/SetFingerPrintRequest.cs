namespace Opti_Sec_Backend.Contracts.Members;

public record SetFingerPrintRequest(
    int MemberId,
    string FingerprintTemplate
);