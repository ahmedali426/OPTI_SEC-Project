using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.DataJson).HasMaxLength(4000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);

        builder.HasOne(x => x.Recipient)
            .WithMany()
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Gate)
            .WithMany()
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GateSession)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.GateSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.RecipientUserId, x.CreatedAt });
        builder.HasIndex(x => new { x.IsSent, x.RetryCount });
    }
}
