using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class AIValidationLogConfiguration : IEntityTypeConfiguration<AIValidationLog>
{
    public void Configure(EntityTypeBuilder<AIValidationLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.AIRawResponseJson).HasMaxLength(4000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        builder.Property(x => x.ConfidenceScore)
             .HasPrecision(5, 2)
             .IsRequired();

        builder.HasOne(x => x.GateSession)
            .WithMany(x => x.AIValidationLogs)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Gate)
            .WithMany(x => x.AIValidationLogs)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MatchedMember)
            .WithMany(x => x.AIValidationLogs)
            .HasForeignKey(x => x.MatchedMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
