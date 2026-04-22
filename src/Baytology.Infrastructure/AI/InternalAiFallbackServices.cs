using System.IO;

using Baytology.Application.Common.Interfaces;
using Baytology.Domain.AISearch;
using Baytology.Domain.Common.Enums;
using Baytology.Infrastructure.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.AI;

internal sealed class InternalAiSearchFallbackService(
    IAppDbContext context,
    IOptions<AiProcessingSettings> settings)
    : IAiSearchFallbackService
{
    private readonly AiProcessingSettings _settings = settings.Value;

    public async Task<AiSearchFallbackResolution?> ResolveAsync(Guid searchRequestId, CancellationToken ct = default)
    {
        if (!_settings.EnableInProcessFallback)
            return null;

        var searchRequest = await context.SearchRequests
            .Include(request => request.TextSearch)
            .Include(request => request.VoiceSearch)
            .Include(request => request.ImageSearch)
            .Include(request => request.Filter)
            .FirstOrDefaultAsync(request => request.Id == searchRequestId, ct);

        if (searchRequest is null || searchRequest.Status != RequestStatus.Pending)
            return null;

        var metadataUpdated = false;
        var queryText = searchRequest.TextSearch?.RawQuery;

        if (searchRequest.VoiceSearch is not null && string.IsNullOrWhiteSpace(searchRequest.VoiceSearch.TranscribedText))
        {
            var derivedTranscription = BuildVoiceFallbackText(searchRequest.VoiceSearch.AudioFileUrl, searchRequest.Filter);
            searchRequest.VoiceSearch.SetTranscription(derivedTranscription, 0.35f, "InternalFallback", derivedTranscription);
            metadataUpdated = true;
            queryText = derivedTranscription;
        }
        else if (searchRequest.VoiceSearch is not null)
        {
            queryText = searchRequest.VoiceSearch.TranscribedText ?? searchRequest.VoiceSearch.ParsedQuery;
        }

        if (searchRequest.ImageSearch is not null && string.IsNullOrWhiteSpace(searchRequest.ImageSearch.EmbeddingVector))
        {
            searchRequest.ImageSearch.SetEmbedding(
                $"internal-fallback:{ExtractHintFromUrl(searchRequest.ImageSearch.ImageFileUrl)}",
                "InternalFallbackVision");
            metadataUpdated = true;
        }

        if (metadataUpdated)
            await context.SaveChangesAsync(ct);

        var filter = searchRequest.Filter;
        var query = context.Properties
            .AsNoTracking()
            .Where(property => property.Status == PropertyStatus.Available);

        if (!string.IsNullOrWhiteSpace(filter?.City))
            query = query.Where(property => property.City == filter.City);

        if (!string.IsNullOrWhiteSpace(filter?.District))
            query = query.Where(property => property.District == filter.District);

        if (!string.IsNullOrWhiteSpace(filter?.PropertyType) &&
            Enum.TryParse<PropertyType>(filter.PropertyType, true, out var propertyType))
        {
            query = query.Where(property => property.PropertyType == propertyType);
        }

        if (!string.IsNullOrWhiteSpace(filter?.ListingType) &&
            Enum.TryParse<ListingType>(filter.ListingType, true, out var listingType))
        {
            query = query.Where(property => property.ListingType == listingType);
        }

        if (filter?.MinPrice is not null)
            query = query.Where(property => property.Price >= filter.MinPrice.Value);

        if (filter?.MaxPrice is not null)
            query = query.Where(property => property.Price <= filter.MaxPrice.Value);

        if (filter?.MinArea is not null)
            query = query.Where(property => property.Area >= filter.MinArea.Value);

        if (filter?.MaxArea is not null)
            query = query.Where(property => property.Area <= filter.MaxArea.Value);

        if (filter?.MinBedrooms is not null)
            query = query.Where(property => property.Bedrooms >= filter.MinBedrooms.Value);

        if (filter?.MaxBedrooms is not null)
            query = query.Where(property => property.Bedrooms <= filter.MaxBedrooms.Value);

        var properties = await query
            .Select(property => new SearchCandidate(
                property.Id,
                property.Title,
                property.Description,
                property.City,
                property.District,
                property.PropertyType,
                property.ListingType,
                property.Price,
                property.Area,
                property.Bedrooms,
                property.IsFeatured,
                property.CreatedOnUtc,
                property.Status))
            .ToListAsync(ct);

        var tokens = Tokenize(queryText);
        var scoreSource = searchRequest.InputType switch
        {
            SearchInputType.Text => "InternalTextFallback",
            SearchInputType.Voice => "InternalVoiceFallback",
            SearchInputType.Image => "InternalImageFallback",
            _ => "InternalFallback"
        };

        var ranked = properties
            .Select(property => new
            {
                Property = property,
                Score = CalculateSearchScore(property, filter, tokens, searchRequest.InputType)
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Property.IsFeatured)
            .ThenByDescending(item => item.Property.CreatedOnUtc)
            .Take(Math.Clamp(_settings.DefaultSearchResultLimit, 1, 50))
            .ToList();

        var results = ranked
            .Select((item, index) => new AiSearchFallbackResult(
                item.Property.Id,
                index + 1,
                item.Score,
                scoreSource,
                item.Property.Title,
                item.Property.Price,
                item.Property.City,
                item.Property.Status.ToString()))
            .ToList();

        return new AiSearchFallbackResolution(true, results);
    }

    private static float CalculateSearchScore(
        SearchCandidate property,
        SearchFilter? filter,
        IReadOnlyCollection<string> tokens,
        SearchInputType inputType)
    {
        var score = 0.15f;

        if (property.IsFeatured)
            score += 0.05f;

        if (!string.IsNullOrWhiteSpace(filter?.City) &&
            string.Equals(property.City, filter.City, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.18f;
        }

        if (!string.IsNullOrWhiteSpace(filter?.District) &&
            string.Equals(property.District, filter.District, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.18f;
        }

        if (filter?.MinPrice is not null || filter?.MaxPrice is not null)
            score += CalculateRangeScore(property.Price, filter.MinPrice, filter.MaxPrice, 0.12f);

        if (filter?.MinArea is not null || filter?.MaxArea is not null)
            score += CalculateRangeScore(property.Area, filter.MinArea, filter.MaxArea, 0.08f);

        if (filter?.MinBedrooms is not null || filter?.MaxBedrooms is not null)
            score += CalculateRangeScore(property.Bedrooms, filter.MinBedrooms, filter.MaxBedrooms, 0.08f);

        foreach (var token in tokens)
        {
            if (Contains(property.Title, token))
                score += 0.16f;

            if (Contains(property.Description, token))
                score += 0.07f;

            if (Contains(property.City, token))
                score += 0.09f;

            if (Contains(property.District, token))
                score += 0.09f;

            if (string.Equals(property.PropertyType.ToString(), token, StringComparison.OrdinalIgnoreCase))
                score += 0.12f;

            if (string.Equals(property.ListingType.ToString(), token, StringComparison.OrdinalIgnoreCase))
                score += 0.12f;
        }

        if (inputType == SearchInputType.Image && tokens.Count == 0)
            score += 0.05f;

        return MathF.Round(score, 4);
    }

    private static float CalculateRangeScore(decimal value, decimal? min, decimal? max, float maxBoost)
    {
        if (min is null && max is null)
            return 0;

        var normalizedMin = min ?? max ?? value;
        var normalizedMax = max ?? min ?? value;

        if (value < normalizedMin || value > normalizedMax)
            return 0;

        if (normalizedMin == normalizedMax)
            return maxBoost;

        var midpoint = (normalizedMin + normalizedMax) / 2;
        var maxDistance = Math.Max(Math.Abs(normalizedMax - midpoint), 1m);
        var distance = Math.Abs(value - midpoint);
        var closeness = 1m - (distance / maxDistance);
        return (float)Math.Max(0m, closeness) * maxBoost;
    }

    private static float CalculateRangeScore(int value, int? min, int? max, float maxBoost)
    {
        if (min is null && max is null)
            return 0;

        var normalizedMin = min ?? max ?? value;
        var normalizedMax = max ?? min ?? value;

        if (value < normalizedMin || value > normalizedMax)
            return 0;

        if (normalizedMin == normalizedMax)
            return maxBoost;

        var midpoint = (normalizedMin + normalizedMax) / 2f;
        var maxDistance = Math.Max(Math.Abs(normalizedMax - midpoint), 1f);
        var distance = Math.Abs(value - midpoint);
        var closeness = 1f - (distance / maxDistance);
        return MathF.Max(0, closeness) * maxBoost;
    }

    private static IReadOnlyCollection<string> Tokenize(string? text)
    {
        return (text ?? string.Empty)
            .Split(
                [' ', '\t', '\r', '\n', ',', '.', ';', ':', '-', '_', '/', '\\', '|', '(', ')', '[', ']', '{', '}', '"', '\''],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 3)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool Contains(string? text, string token)
    {
        return !string.IsNullOrWhiteSpace(text) &&
               text.Contains(token, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildVoiceFallbackText(string? audioFileUrl, SearchFilter? filter)
    {
        var hint = ExtractHintFromUrl(audioFileUrl);
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(hint))
            segments.Add(hint);

        if (!string.IsNullOrWhiteSpace(filter?.District))
            segments.Add(filter.District);

        if (!string.IsNullOrWhiteSpace(filter?.City))
            segments.Add(filter.City);

        if (!string.IsNullOrWhiteSpace(filter?.PropertyType))
            segments.Add(filter.PropertyType);

        if (!string.IsNullOrWhiteSpace(filter?.ListingType))
            segments.Add(filter.ListingType);

        return segments.Count == 0
            ? "voice property search"
            : string.Join(' ', segments);
    }

    private static string ExtractHintFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        var path = url;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            path = uri.AbsolutePath;

        return Path.GetFileNameWithoutExtension(path)
            .Replace('-', ' ')
            .Replace('_', ' ')
            .Trim();
    }

    private sealed record SearchCandidate(
        Guid Id,
        string Title,
        string? Description,
        string? City,
        string? District,
        PropertyType PropertyType,
        ListingType ListingType,
        decimal Price,
        decimal Area,
        int Bedrooms,
        bool IsFeatured,
        DateTimeOffset CreatedOnUtc,
        PropertyStatus Status);
}

internal sealed class InternalRecommendationFallbackService(
    IAppDbContext context,
    IOptions<AiProcessingSettings> settings)
    : IRecommendationFallbackService
{
    private readonly AiProcessingSettings _settings = settings.Value;

    public async Task<RecommendationFallbackResolution?> ResolveAsync(Guid recommendationRequestId, CancellationToken ct = default)
    {
        if (!_settings.EnableInProcessFallback)
            return null;

        var request = await context.RecommendationRequests
            .FirstOrDefaultAsync(item => item.Id == recommendationRequestId, ct);

        if (request is null || request.Status != RequestStatus.Pending)
            return null;

        var availableProperties = await context.Properties
            .AsNoTracking()
            .Where(property => property.Status == PropertyStatus.Available)
            .Select(property => new RecommendationCandidate(
                property.Id,
                property.Title,
                property.City,
                property.District,
                property.PropertyType,
                property.ListingType,
                property.Price,
                property.Area,
                property.Bedrooms,
                property.IsFeatured,
                property.CreatedOnUtc))
            .ToListAsync(ct);

        var interactedPropertyIds = await GetInteractedPropertyIdsAsync(request.RequestedByUserId, ct);
        var preferredPropertyIds = new HashSet<Guid>(interactedPropertyIds);

        RecommendationCandidate? sourceProperty = null;
        SearchFilter? sourceFilter = null;

        if (request.SourceEntityType.Contains("history", StringComparison.OrdinalIgnoreCase) ||
            request.SourceEntityType.Contains("user", StringComparison.OrdinalIgnoreCase))
        {
            var latestInteractionPropertyId = await context.PropertyViews
                .AsNoTracking()
                .Where(view => view.UserId == request.RequestedByUserId)
                .OrderByDescending(view => view.ViewedAt)
                .Select(view => (Guid?)view.PropertyId)
                .FirstOrDefaultAsync(ct)
                ?? await context.SavedProperties
                    .AsNoTracking()
                    .Where(savedProperty => savedProperty.UserId == request.RequestedByUserId)
                    .OrderByDescending(savedProperty => savedProperty.SavedAt)
                    .Select(savedProperty => (Guid?)savedProperty.PropertyId)
                    .FirstOrDefaultAsync(ct)
                ?? await context.Bookings
                    .AsNoTracking()
                    .Where(booking => booking.UserId == request.RequestedByUserId)
                    .OrderByDescending(booking => booking.CreatedOnUtc)
                    .Select(booking => (Guid?)booking.PropertyId)
                    .FirstOrDefaultAsync(ct);

            if (latestInteractionPropertyId.HasValue)
            {
                sourceProperty = availableProperties.FirstOrDefault(property => property.Id == latestInteractionPropertyId.Value)
                    ?? await context.Properties
                        .AsNoTracking()
                        .Where(property => property.Id == latestInteractionPropertyId.Value)
                        .Select(property => new RecommendationCandidate(
                            property.Id,
                            property.Title,
                            property.City,
                            property.District,
                            property.PropertyType,
                            property.ListingType,
                            property.Price,
                            property.Area,
                            property.Bedrooms,
                            property.IsFeatured,
                            property.CreatedOnUtc))
                        .FirstOrDefaultAsync(ct);
            }
        }
        else if (request.SourceEntityType.Contains("property", StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(request.SourceEntityId, out var sourcePropertyId))
        {
            sourceProperty = availableProperties.FirstOrDefault(property => property.Id == sourcePropertyId)
                ?? await context.Properties
                    .AsNoTracking()
                    .Where(property => property.Id == sourcePropertyId)
                    .Select(property => new RecommendationCandidate(
                        property.Id,
                        property.Title,
                        property.City,
                        property.District,
                        property.PropertyType,
                        property.ListingType,
                        property.Price,
                        property.Area,
                        property.Bedrooms,
                        property.IsFeatured,
                        property.CreatedOnUtc))
                    .FirstOrDefaultAsync(ct);

            if (sourceProperty is not null)
                preferredPropertyIds.Add(sourceProperty.Id);
        }
        else if (request.SourceEntityType.Contains("search", StringComparison.OrdinalIgnoreCase) &&
                 Guid.TryParse(request.SourceEntityId, out var sourceSearchRequestId))
        {
            sourceFilter = await context.SearchFilters
                .AsNoTracking()
                .FirstOrDefaultAsync(filter => filter.SearchRequestId == sourceSearchRequestId, ct);

            var topSearchResultPropertyId = await context.SearchResults
                .AsNoTracking()
                .Where(result => result.SearchRequestId == sourceSearchRequestId)
                .OrderBy(result => result.Rank)
                .Select(result => (Guid?)result.PropertyId)
                .FirstOrDefaultAsync(ct);

            if (topSearchResultPropertyId.HasValue)
            {
                sourceProperty = availableProperties.FirstOrDefault(property => property.Id == topSearchResultPropertyId.Value)
                    ?? await context.Properties
                        .AsNoTracking()
                        .Where(property => property.Id == topSearchResultPropertyId.Value)
                        .Select(property => new RecommendationCandidate(
                            property.Id,
                            property.Title,
                            property.City,
                            property.District,
                            property.PropertyType,
                            property.ListingType,
                            property.Price,
                            property.Area,
                            property.Bedrooms,
                            property.IsFeatured,
                            property.CreatedOnUtc))
                        .FirstOrDefaultAsync(ct);
            }
        }

        if (sourceProperty is not null)
            preferredPropertyIds.Add(sourceProperty.Id);

        var preferredCities = await context.PropertyViews
            .AsNoTracking()
            .Where(view => view.UserId == request.RequestedByUserId)
            .Join(
                context.Properties.AsNoTracking(),
                view => view.PropertyId,
                property => property.Id,
                (_, property) => property.City)
            .Where(city => !string.IsNullOrWhiteSpace(city))
            .ToListAsync(ct);

        var ranked = availableProperties
            .Where(property => !preferredPropertyIds.Contains(property.Id))
            .Select(property => new
            {
                Property = property,
                Score = CalculateRecommendationScore(property, sourceProperty, sourceFilter, preferredCities)
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Property.IsFeatured)
            .ThenByDescending(item => item.Property.CreatedOnUtc)
            .Take(Math.Clamp(request.TopN > 0 ? request.TopN : _settings.DefaultRecommendationResultLimit, 1, 50))
            .ToList();

        var results = ranked
            .Select((item, index) => new RecommendationFallbackResult(
                item.Property.Id,
                null,
                item.Score,
                index + 1,
                item.Property.Title,
                item.Property.Price))
            .ToList();

        return new RecommendationFallbackResolution(true, results);
    }

    private async Task<HashSet<Guid>> GetInteractedPropertyIdsAsync(string userId, CancellationToken ct)
    {
        var viewed = context.PropertyViews
            .AsNoTracking()
            .Where(view => view.UserId == userId)
            .Select(view => view.PropertyId);

        var saved = context.SavedProperties
            .AsNoTracking()
            .Where(savedProperty => savedProperty.UserId == userId)
            .Select(savedProperty => savedProperty.PropertyId);

        var booked = context.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == userId)
            .Select(booking => booking.PropertyId);

        var propertyIds = await viewed
            .Concat(saved)
            .Concat(booked)
            .Distinct()
            .ToListAsync(ct);

        return propertyIds.ToHashSet();
    }

    private static float CalculateRecommendationScore(
        RecommendationCandidate property,
        RecommendationCandidate? sourceProperty,
        SearchFilter? sourceFilter,
        IReadOnlyCollection<string?> preferredCities)
    {
        var score = 0.12f;

        if (property.IsFeatured)
            score += 0.04f;

        if (sourceProperty is not null)
        {
            if (property.PropertyType == sourceProperty.PropertyType)
                score += 0.28f;

            if (property.ListingType == sourceProperty.ListingType)
                score += 0.22f;

            if (string.Equals(property.City, sourceProperty.City, StringComparison.OrdinalIgnoreCase))
                score += 0.16f;

            if (string.Equals(property.District, sourceProperty.District, StringComparison.OrdinalIgnoreCase))
                score += 0.16f;

            score += CalculateSimilarityBoost(property.Price, sourceProperty.Price, 0.12f);
            score += CalculateSimilarityBoost(property.Area, sourceProperty.Area, 0.06f);
            score += CalculateSimilarityBoost(property.Bedrooms, sourceProperty.Bedrooms, 0.05f);
        }

        if (!string.IsNullOrWhiteSpace(sourceFilter?.City) &&
            string.Equals(property.City, sourceFilter.City, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.12f;
        }

        if (!string.IsNullOrWhiteSpace(sourceFilter?.District) &&
            string.Equals(property.District, sourceFilter.District, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.12f;
        }

        if (!string.IsNullOrWhiteSpace(sourceFilter?.PropertyType) &&
            string.Equals(property.PropertyType.ToString(), sourceFilter.PropertyType, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.14f;
        }

        if (!string.IsNullOrWhiteSpace(sourceFilter?.ListingType) &&
            string.Equals(property.ListingType.ToString(), sourceFilter.ListingType, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.14f;
        }

        if (preferredCities.Any(city => string.Equals(city, property.City, StringComparison.OrdinalIgnoreCase)))
            score += 0.08f;

        return MathF.Round(score, 4);
    }

    private static float CalculateSimilarityBoost(decimal left, decimal right, float maxBoost)
    {
        if (left <= 0 || right <= 0)
            return 0;

        var max = Math.Max(left, right);
        var diff = Math.Abs(left - right);
        var closeness = 1m - (diff / max);
        return (float)Math.Max(0m, closeness) * maxBoost;
    }

    private static float CalculateSimilarityBoost(int left, int right, float maxBoost)
    {
        if (left <= 0 || right <= 0)
            return 0;

        var max = Math.Max(left, right);
        var diff = Math.Abs(left - right);
        var closeness = 1f - ((float)diff / max);
        return MathF.Max(0, closeness) * maxBoost;
    }

    private sealed record RecommendationCandidate(
        Guid Id,
        string Title,
        string? City,
        string? District,
        PropertyType PropertyType,
        ListingType ListingType,
        decimal Price,
        decimal Area,
        int Bedrooms,
        bool IsFeatured,
        DateTimeOffset CreatedOnUtc);
}
