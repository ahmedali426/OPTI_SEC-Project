using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class GateConfiguration : IEntityTypeConfiguration<Gate>
{
    public void Configure(EntityTypeBuilder<Gate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.SilentAlarmHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.DeviceId)
            .HasMaxLength(100);

        builder.Property(x => x.DeviceApiKey)
            .HasMaxLength(256);

        builder.HasMany(x => x.AccessLogs)
            .WithOne(x => x.Gate)
            .HasForeignKey(x => x.GateId);


        builder.HasOne(x => x.Client)
            .WithMany(x => x.Gates)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasMany(x => x.AccessLogs)
            .WithOne(x => x.Gate)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.GateSessions)
            .WithOne(x => x.Gate)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PasswordAttempts)
            .WithOne(x => x.Gate)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.EmergencyEvents)
            .WithOne(x => x.Gate)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.DeviceCommands)
            .WithOne(x => x.Gate)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasMany(x => x.AIValidationLogs)
            .WithOne(x => x.Gate)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

  
        builder.HasMany(x => x.FingerprintValidationLogs)
            .WithOne(x => x.Gate)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
