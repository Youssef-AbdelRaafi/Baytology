using Baytology.Domain.Properties;
using Baytology.Domain.Recommendations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class RecommendationResultConfiguration : IEntityTypeConfiguration<RecommendationResult>
{
    public void Configure(EntityTypeBuilder<RecommendationResult> builder)
    {
        builder.ToTable("RecommendationResults", table =>
        {
            table.HasCheckConstraint(
                "CK_RecommendationResults_BusinessRules",
                "[Rank] > 0 AND [SimilarityScore] >= 0 AND ([SnapshotPrice] IS NULL OR [SnapshotPrice] >= 0) AND ([RecommendedPropertyId] IS NOT NULL OR [ExternalReference] IS NOT NULL)");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.ExternalReference).HasMaxLength(500);
        builder.Property(x => x.SnapshotTitle).HasMaxLength(500);
        builder.Property(x => x.SnapshotPrice).HasPrecision(18, 2);
        builder.HasOne<Property>().WithMany().HasForeignKey(x => x.RecommendedPropertyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => new { x.RequestId, x.Rank }).IsUnique();
    }
}
