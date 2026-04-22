using Baytology.Domain.Properties;
using Baytology.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class AgentReviewConfiguration : IEntityTypeConfiguration<AgentReview>
{
    public void Configure(EntityTypeBuilder<AgentReview> builder)
    {
        builder.ToTable("AgentReviews", table =>
        {
            table.HasCheckConstraint("CK_AgentReviews_Rating_Range", "[Rating] >= 1 AND [Rating] <= 5");
            table.HasCheckConstraint("CK_AgentReviews_DistinctUsers", "[AgentUserId] <> [ReviewerUserId]");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.AgentUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.ReviewerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Property>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.AgentUserId);
        builder.HasIndex(x => new { x.AgentUserId, x.ReviewerUserId, x.PropertyId }).IsUnique();
        builder.HasIndex(x => new { x.AgentUserId, x.ReviewerUserId })
            .IsUnique()
            .HasFilter("[PropertyId] IS NULL");
    }
}
