using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Entities;

public class DeviceCommand
{
    public int Id { get; set; }

    public int GateId { get; set; }
    public Gate Gate { get; set; } = default!;

    // type of the command 
    public CommandType Type { get; set; }

    // more information 
    public string PayloadJson { get; set; } = "{}";

    // backend or mobile 
    public CommandSource Source { get; set; }

    public CommandStatus Status { get; set; } = CommandStatus.Pending;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    
    // the time of the acknowledge 
    public DateTime? AcknowledgedAt { get; set; }

    public string? IssuedByUserId { get; set; }
    public ApplicationUser? IssuedBy { get; set; }

    public int? GateSessionId { get; set; }
    public GateSession? GateSession { get; set; }
}
