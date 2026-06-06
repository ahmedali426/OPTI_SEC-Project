using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class DeviceCommandConfiguration : IEntityTypeConfiguration<DeviceCommand>
{
    public void Configure(EntityTypeBuilder<DeviceCommand> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PayloadJson).HasMaxLength(4000).IsRequired();

        builder.HasOne(x => x.Gate)
            .WithMany(x => x.DeviceCommands)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IssuedBy)
            .WithMany()
            .HasForeignKey(x => x.IssuedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.GateSession)
            .WithMany(x => x.DeviceCommands)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
