using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class GateSessionConfiguration : IEntityTypeConfiguration<GateSession>
{
    public void Configure(EntityTypeBuilder<GateSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.SessionToken).IsUnique();
        builder.Property(x => x.DeviceId).HasMaxLength(100);
        builder.Property(x => x.FailureReason).HasMaxLength(500);

        builder.HasOne(x => x.Gate)
            .WithMany(x => x.GateSessions)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Member)
            .WithMany(x => x.GateSessions)
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.AIValidationLogs)
            .WithOne(x => x.GateSession)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.FingerprintValidationLogs)
            .WithOne(x => x.GateSession)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PasswordAttempts)
            .WithOne(x => x.GateSession)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.DeviceCommands)
            .WithOne(x => x.GateSession)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Notifications)
            .WithOne(x => x.GateSession)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);

  
        builder.HasMany(x => x.EmergencyEvents)
            .WithOne(x => x.GateSession)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AccessLog)
            .WithOne(x => x.GateSession)
            .HasForeignKey<AccessLog>(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
