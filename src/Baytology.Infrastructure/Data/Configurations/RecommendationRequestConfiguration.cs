using Baytology.Domain.Recommendations;
using Baytology.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class RecommendationRequestConfiguration : IEntityTypeConfiguration<RecommendationRequest>
{
    public void Configure(EntityTypeBuilder<RecommendationRequest> builder)
    {
        builder.ToTable("RecommendationRequests", table =>
        {
            table.HasCheckConstraint(
                "CK_RecommendationRequests_BusinessRules",
                "LEN(LTRIM(RTRIM([RequestedByUserId]))) > 0 AND LEN(LTRIM(RTRIM([SourceEntityType]))) > 0 AND [TopN] >= 1 AND [TopN] <= 50 AND (([Status] = 'Pending' AND [ResolvedAt] IS NULL) OR ([Status] <> 'Pending' AND [ResolvedAt] IS NOT NULL))");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.SourceEntityType).HasMaxLength(50);
        builder.Property(x => x.SourceEntityId).HasMaxLength(200);
        builder.Property(x => x.CorrelationId).HasMaxLength(200);
        builder.Property(x => x.ModelVersion).HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Results).WithOne().HasForeignKey(x => x.RequestId);
        builder.Navigation(x => x.Results).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => x.RequestedByUserId);
    }
}
