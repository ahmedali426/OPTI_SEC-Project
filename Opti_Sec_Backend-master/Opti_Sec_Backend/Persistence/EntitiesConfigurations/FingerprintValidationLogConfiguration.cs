using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class FingerprintValidationLogConfiguration : IEntityTypeConfiguration<FingerprintValidationLog>
{
    public void Configure(EntityTypeBuilder<FingerprintValidationLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FingerprintTemplateHash).HasMaxLength(256);
        builder.Property(x => x.FailureReason).HasMaxLength(500);

        builder.HasOne(x => x.GateSession)
            .WithMany(x => x.FingerprintValidationLogs)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Gate)
            .WithMany(x => x.FingerprintValidationLogs)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ExpectedMember)
            .WithMany(x => x.FingerprintValidationLogs)
            .HasForeignKey(x => x.ExpectedMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
