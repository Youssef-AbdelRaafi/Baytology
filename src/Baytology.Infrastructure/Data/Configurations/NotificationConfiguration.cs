using Baytology.Domain.Notifications;
using Baytology.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", table =>
        {
            table.HasCheckConstraint(
                "CK_Notifications_ReadState",
                "([IsRead] = 0 AND [ReadAt] IS NULL) OR ([IsRead] = 1 AND [ReadAt] IS NOT NULL)");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ReferenceId).HasMaxLength(100);
        builder.Property(x => x.ReferenceType).HasConversion<string>().HasMaxLength(30);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.UserId, x.IsRead });
    }
}
