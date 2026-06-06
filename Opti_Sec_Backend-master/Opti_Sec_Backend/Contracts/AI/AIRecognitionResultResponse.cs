using Opti_Sec_Backend.Contracts.Device;
using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Contracts.AI;

public record AIRecognitionResultResponse(
    bool Received,
    AIRecognitionStatus Status,
    string? NextStep,
    int? MemberId,
    string? Message,
    DeviceCommandsDto Commands
);
