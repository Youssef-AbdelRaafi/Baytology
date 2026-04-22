using Baytology.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class PropertyAmenityConfiguration : IEntityTypeConfiguration<PropertyAmenity>
{
    public void Configure(EntityTypeBuilder<PropertyAmenity> builder)
    {
        builder.ToTable("PropertyAmenities");
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.HasIndex(x => x.PropertyId).IsUnique();
        builder.Property(x => x.FurnishingStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ViewType).HasConversion<string>().HasMaxLength(20);
    }
}
