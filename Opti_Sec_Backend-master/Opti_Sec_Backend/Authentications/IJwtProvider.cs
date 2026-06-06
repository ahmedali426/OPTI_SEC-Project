using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Authentications;

public interface IJwtProvider
{
    (string token, int expirIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles);

    string? ValidateToken(string token);
}
