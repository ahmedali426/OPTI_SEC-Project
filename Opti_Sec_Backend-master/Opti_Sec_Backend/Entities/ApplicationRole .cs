using Microsoft.AspNetCore.Identity;

namespace Opti_Sec_Backend.Entities;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole()
    {
        Id = Guid.CreateVersion7().ToString();
    }
    public bool IsDefault { get; set; }
    public bool IsDeleted { get; set; }
}
