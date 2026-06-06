using System.Reflection.Metadata.Ecma335;

namespace Opti_Sec_Backend.Abstractions;

public record Error(string Code, string Description, int? StatusCode)
{
    public static readonly Error None = new(string.Empty, string.Empty,null);
}
