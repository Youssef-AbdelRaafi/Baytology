using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AISearch;

public sealed class ImageSearch : Entity
{
    public Guid SearchRequestId { get; private set; }
    public string ImageFileUrl { get; private set; } = null!;
    public string? EmbeddingVector { get; private set; }
    public string? ModelUsed { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private ImageSearch() { }

    private ImageSearch(Guid searchRequestId, string imageFileUrl)
        : base(Guid.NewGuid())
    {
        SearchRequestId = searchRequestId;
        ImageFileUrl = imageFileUrl;
    }

    public static Result<ImageSearch> Create(Guid searchRequestId, string? imageFileUrl)
    {
        if (searchRequestId == Guid.Empty)
            return SearchErrors.SearchRequestIdRequired;

        var normalizedImageFileUrl = imageFileUrl?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedImageFileUrl))
            return SearchErrors.ImageFileUrlRequired;

        if (normalizedImageFileUrl.Length > 1000)
            return SearchErrors.ImageFileUrlTooLong;

        return new ImageSearch(searchRequestId, normalizedImageFileUrl);
    }

    public void SetEmbedding(string embeddingVector, string modelUsed)
    {
        EmbeddingVector = embeddingVector;
        ModelUsed = modelUsed;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
