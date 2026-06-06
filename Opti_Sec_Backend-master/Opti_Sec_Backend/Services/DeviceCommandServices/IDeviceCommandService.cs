using Opti_Sec_Backend.Contracts.Device;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Services.DeviceCommandServices;

public interface IDeviceCommandService
{
    Task<DeviceCommand> SendOpenGateAsync(int gateId, string? userId = null, int? sessionId = null, CancellationToken ct = default);
    Task<DeviceCommand> SendActivateBuzzerAsync(int gateId, int durationSeconds = 30, int? sessionId = null, CancellationToken ct = default);
    Task<DeviceCommand> SendStopBuzzerAsync(int gateId, string userId, CancellationToken ct = default);
    Task<Result> AcknowledgeCommandAsync(int commandId, CancellationToken ct = default);
    Task<IEnumerable<PendingCommandResponse>> GetPendingCommandsAsync(int gateId, CancellationToken ct = default);
}
