using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Entities;

public class AccessLog : AuditableEntity
{
    public int Id { get; set; }

    public string? ImageUrl { get; set; } = string.Empty;

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public bool IsAuthorized { get; set; }

    // For unauthorized access, indicates if a silent alarm should be triggered
    public bool IsSilentAlarm { get; set; }

    // For authorized access, indicates if the member used a valid access method (e.g., fingerprint, card)

    public AccessMethod AccessMethod { get; set; }

    public int? GateSessionId { get; set; }
    public GateSession? GateSession { get; set; }

    public int? MemberId { get; set; }

    public Member? Member { get; set; }

    public bool IsDeleted { get; set; }

    public int? GateId { get; set; }

    public Gate? Gate { get; set; }
}
