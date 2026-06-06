using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class EmergencyEventConfiguration : IEntityTypeConfiguration<EmergencyEvent>
{
    public void Configure(EntityTypeBuilder<EmergencyEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(500);

        builder.HasOne(x => x.Gate)
            .WithMany(x => x.EmergencyEvents)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResolvedBy)
            .WithMany()
            .HasForeignKey(x => x.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.GateSession)
            .WithMany(x => x.EmergencyEvents)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.GateId, x.IsResolved });
    }
}
