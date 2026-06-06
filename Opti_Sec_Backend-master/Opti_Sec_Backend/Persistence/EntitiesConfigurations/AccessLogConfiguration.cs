using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.DateTime)
            .IsRequired();

        builder.Property(x => x.IsAuthorized)
            .IsRequired();

        builder.HasOne(x => x.Member)
            .WithMany(x => x.AccessLogs)
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Gate)
            .WithMany(x => x.AccessLogs)
            .HasForeignKey(x => x.GateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
