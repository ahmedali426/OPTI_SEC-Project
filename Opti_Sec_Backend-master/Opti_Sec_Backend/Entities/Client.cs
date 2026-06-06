namespace Opti_Sec_Backend.Entities;

public class Client : AuditableEntity
{
    public int Id { get; set; }
    public string FName { get; set; } = string.Empty;
    public string LName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public bool IsDeleted { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = default!;

    public ICollection<Member> Members { get; set; } = [];
    public ICollection<Gate> Gates { get; set; } = [];
}
