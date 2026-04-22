using Baytology.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.ToTable("PropertyImages", table =>
        {
            table.HasCheckConstraint(
                "CK_PropertyImages_BusinessRules",
                "LEN(LTRIM(RTRIM([Url]))) > 0 AND [SortOrder] >= 0");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => new { x.PropertyId, x.SortOrder }).IsUnique();
    }
}
