using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Abstractions.Consts;

public static class DefaultRoles
{
    public const string Admin = nameof(Admin);
    public const string AdminRoleId = "019d0165-8401-7242-8c9b-65e629be99b3";
    public const string AdminRoleConcurrencyStamp = "019d0166-15af-7300-b2be-87428be3dd2e";


    public const string Member = nameof(Client);
    public const string MemberRoleId = "019d0165-bdd2-79de-9c06-f98ae213063a";
    public const string MemberRoleConcurrencyStamp = "019d0166-3bd5-7de0-bcb1-39b8bed0cbc7";
}
