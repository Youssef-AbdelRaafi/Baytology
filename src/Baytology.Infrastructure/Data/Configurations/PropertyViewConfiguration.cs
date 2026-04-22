using Baytology.Domain.Properties;
using Baytology.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class PropertyViewConfiguration : IEntityTypeConfiguration<PropertyView>
{
    public void Configure(EntityTypeBuilder<PropertyView> builder)
    {
        builder.ToTable("PropertyViews");
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.HasOne<Property>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.PropertyId);
    }
}
