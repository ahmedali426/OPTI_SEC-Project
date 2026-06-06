using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence.EntitiesConfigurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Phone)
            .HasMaxLength(15);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.HasIndex(x => x.UserName).IsUnique();

        builder.Property(x => x.FaceImageUrl)
            .HasMaxLength(500);

        builder.HasMany(x => x.AccessLogs)
            .WithOne(x => x.Member)
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.GateSessions)
            .WithOne(x => x.Member)
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AIValidationLogs)
            .WithOne(x => x.MatchedMember)
            .HasForeignKey(x => x.MatchedMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.FingerprintValidationLogs)
            .WithOne(x => x.ExpectedMember)
            .HasForeignKey(x => x.ExpectedMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
