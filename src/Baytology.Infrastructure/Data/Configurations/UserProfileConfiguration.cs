using Baytology.Domain.UserProfiles;
using Baytology.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.PreferredContactMethod).HasConversion<string>().HasMaxLength(20);
        builder.HasOne<AppUser>().WithOne().HasForeignKey<UserProfile>(x => x.UserId);
    }
}
