using Baytology.Domain.AISearch;
using Baytology.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class SearchRequestConfiguration : IEntityTypeConfiguration<SearchRequest>
{
    public void Configure(EntityTypeBuilder<SearchRequest> builder)
    {
        builder.ToTable("SearchRequests", table =>
        {
            table.HasCheckConstraint(
                "CK_SearchRequests_State",
                "LEN(LTRIM(RTRIM([UserId]))) > 0 AND [ResultCount] >= 0 AND (([Status] = 'Pending' AND [ResolvedAt] IS NULL) OR ([Status] <> 'Pending' AND [ResolvedAt] IS NOT NULL))");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.InputType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.SearchEngine).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CorrelationId).HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TextSearch).WithOne().HasForeignKey<TextSearch>(x => x.SearchRequestId);
        builder.HasOne(x => x.VoiceSearch).WithOne().HasForeignKey<VoiceSearch>(x => x.SearchRequestId);
        builder.HasOne(x => x.ImageSearch).WithOne().HasForeignKey<ImageSearch>(x => x.SearchRequestId);
        builder.HasOne(x => x.Filter).WithOne().HasForeignKey<SearchFilter>(x => x.SearchRequestId);
        builder.HasMany(x => x.Results).WithOne().HasForeignKey(x => x.SearchRequestId);
        builder.Navigation(x => x.Results).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => x.UserId);
    }
}

public class TextSearchConfiguration : IEntityTypeConfiguration<TextSearch>
{
    public void Configure(EntityTypeBuilder<TextSearch> builder)
    {
        builder.ToTable("TextSearches");
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.RawQuery).HasMaxLength(2000);
        builder.Property(x => x.ParsedQuery).HasMaxLength(2000);
    }
}

public class VoiceSearchConfiguration : IEntityTypeConfiguration<VoiceSearch>
{
    public void Configure(EntityTypeBuilder<VoiceSearch> builder)
    {
        builder.ToTable("VoiceSearches");
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.AudioFileUrl).HasMaxLength(1000);
        builder.Property(x => x.TranscribedText).HasMaxLength(5000);
        builder.Property(x => x.Language).HasMaxLength(10);
        builder.Property(x => x.STTProvider).HasMaxLength(50);
        builder.Property(x => x.ParsedQuery).HasMaxLength(2000);
    }
}

public class ImageSearchConfiguration : IEntityTypeConfiguration<ImageSearch>
{
    public void Configure(EntityTypeBuilder<ImageSearch> builder)
    {
        builder.ToTable("ImageSearches");
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.ImageFileUrl).HasMaxLength(1000);
        builder.Property(x => x.EmbeddingVector).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ModelUsed).HasMaxLength(50);
    }
}

public class SearchFilterConfiguration : IEntityTypeConfiguration<SearchFilter>
{
    public void Configure(EntityTypeBuilder<SearchFilter> builder)
    {
        builder.ToTable("SearchFilters", table =>
        {
            table.HasCheckConstraint(
                "CK_SearchFilters_Ranges",
                "([MinPrice] IS NULL OR [MinPrice] >= 0) AND ([MaxPrice] IS NULL OR [MaxPrice] >= 0) AND ([MinPrice] IS NULL OR [MaxPrice] IS NULL OR [MinPrice] <= [MaxPrice]) AND " +
                "([MinArea] IS NULL OR [MinArea] >= 0) AND ([MaxArea] IS NULL OR [MaxArea] >= 0) AND ([MinArea] IS NULL OR [MaxArea] IS NULL OR [MinArea] <= [MaxArea]) AND " +
                "([MinBedrooms] IS NULL OR [MinBedrooms] >= 0) AND ([MaxBedrooms] IS NULL OR [MaxBedrooms] >= 0) AND ([MinBedrooms] IS NULL OR [MaxBedrooms] IS NULL OR [MinBedrooms] <= [MaxBedrooms])");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.PropertyType).HasMaxLength(30);
        builder.Property(x => x.ListingType).HasMaxLength(20);
        builder.Property(x => x.MinPrice).HasPrecision(18, 2);
        builder.Property(x => x.MaxPrice).HasPrecision(18, 2);
        builder.Property(x => x.MinArea).HasPrecision(12, 2);
        builder.Property(x => x.MaxArea).HasPrecision(12, 2);
    }
}

public class SearchResultConfiguration : IEntityTypeConfiguration<SearchResult>
{
    public void Configure(EntityTypeBuilder<SearchResult> builder)
    {
        builder.ToTable("SearchResults", table =>
        {
            table.HasCheckConstraint(
                "CK_SearchResults_BusinessRules",
                "[Rank] > 0 AND [RelevanceScore] >= 0 AND ([SnapshotPrice] IS NULL OR [SnapshotPrice] >= 0)");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.ScoreSource).HasMaxLength(50);
        builder.Property(x => x.SnapshotTitle).HasMaxLength(500);
        builder.Property(x => x.SnapshotPrice).HasPrecision(18, 2);
        builder.Property(x => x.SnapshotCity).HasMaxLength(100);
        builder.Property(x => x.SnapshotStatus).HasMaxLength(30);
        builder.HasIndex(x => new { x.SearchRequestId, x.Rank }).IsUnique();
    }
}
