using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Services.FingerprintServices;

public class FingerprintService(ApplicationDbContext context) : IFingerprintService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<bool> VerifyAsync(int expectedMemberId, string fingerprintTemplate, CancellationToken ct = default)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == expectedMemberId && !m.IsDeleted, ct);

        if (member?.FingerprintTemplate is null)
            return false;

        // Direct template comparison — matches existing project pattern
        return member.FingerprintTemplate == fingerprintTemplate;
    }
}
