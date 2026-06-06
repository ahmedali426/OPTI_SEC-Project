using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class PasswordAttemptConfiguration : IEntityTypeConfiguration<PasswordAttempt>
{
    public void Configure(EntityTypeBuilder<PasswordAttempt> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceId).HasMaxLength(100);
        builder.Property(x => x.PasswordHashAttempt).HasMaxLength(256).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(45);

        builder.HasOne(x => x.Gate)
            .WithMany(x => x.PasswordAttempts)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GateSession)
            .WithMany(x => x.PasswordAttempts)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasIndex(x => new { x.GateId, x.AttemptedAt });
    }
}
