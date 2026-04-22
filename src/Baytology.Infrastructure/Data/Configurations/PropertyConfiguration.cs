using Baytology.Domain.Properties;
using Baytology.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Baytology.Infrastructure.Data.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties", table =>
        {
            table.HasCheckConstraint(
                "CK_Properties_BusinessRules",
                "[Price] > 0 AND [Area] > 0 AND [Bedrooms] >= 0 AND [Bathrooms] >= 0 AND " +
                "([Floor] IS NULL OR [Floor] >= 0) AND ([TotalFloors] IS NULL OR [TotalFloors] > 0) AND " +
                "([Floor] IS NULL OR [TotalFloors] IS NULL OR [Floor] <= [TotalFloors]) AND " +
                "([Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)) AND " +
                "([Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180))");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(5000);
        builder.Property(x => x.PropertyType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ListingType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Area).HasPrecision(12, 2);
        builder.Property(x => x.AddressLine).HasMaxLength(500);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.ZipCode).HasMaxLength(20);
        builder.Property(x => x.SourceListingUrl).HasMaxLength(1000);
        builder.Property(x => x.Latitude).HasPrecision(10, 7);
        builder.Property(x => x.Longitude).HasPrecision(10, 7);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.AgentUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Images).WithOne().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Images).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasOne(x => x.Amenity).WithOne().HasForeignKey<PropertyAmenity>(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Price);
        builder.HasIndex(x => new { x.Status, x.CreatedOnUtc });
    }
}
