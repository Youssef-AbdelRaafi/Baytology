using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.InternalAi.Dtos;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.InternalAi.Queries.LookupPropertyMappings;

public sealed class LookupPropertyMappingsQueryHandler(IAppDbContext context)
    : IRequestHandler<LookupPropertyMappingsQuery, Result<List<PropertyLookupResultDto>>>
{
    public async Task<Result<List<PropertyLookupResultDto>>> Handle(LookupPropertyMappingsQuery request, CancellationToken ct)
    {
        if (request.Items.Count == 0)
            return new List<PropertyLookupResultDto>();

        var properties = await context.Properties
            .AsNoTracking()
            .Select(property => new PropertyCandidate(
                property.Id,
                property.SourceListingUrl,
                property.Title,
                property.Price,
                property.City,
                property.District,
                property.PropertyType,
                property.Area,
                property.Bedrooms))
            .ToListAsync(ct);

        var results = request.Items
            .Select((item, index) => ResolveSingle(index, item, properties))
            .ToList();

        return results;
    }

    private static PropertyLookupResultDto ResolveSingle(
        int index,
        PropertyLookupItemDto item,
        IReadOnlyCollection<PropertyCandidate> properties)
    {
        var normalizedUrl = NormalizeUrl(item.SourceListingUrl);

        if (!string.IsNullOrWhiteSpace(normalizedUrl))
        {
            var exactMatch = properties.FirstOrDefault(property =>
                string.Equals(NormalizeUrl(property.SourceListingUrl), normalizedUrl, StringComparison.OrdinalIgnoreCase));

            if (exactMatch is not null)
            {
                return new PropertyLookupResultDto(index, exactMatch.Id, "SourceListingUrl", exactMatch.SourceListingUrl);
            }
        }

        var rankedCandidates = properties
            .Select(property => new
            {
                Property = property,
                Score = CalculateHeuristicScore(item, property)
            })
            .Where(candidate => candidate.Score >= 5)
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Property.Title)
            .ToList();

        var bestCandidate = rankedCandidates.FirstOrDefault();

        return new PropertyLookupResultDto(
            index,
            bestCandidate?.Property.Id,
            bestCandidate is null ? null : "Heuristic",
            bestCandidate?.Property.SourceListingUrl);
    }

    private static int CalculateHeuristicScore(PropertyLookupItemDto lookup, PropertyCandidate property)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(lookup.Title) &&
            ContainsEitherWay(lookup.Title, property.Title))
        {
            score += 2;
        }

        if (lookup.Price is not null && Math.Abs(property.Price - lookup.Price.Value) <= 0.01m)
            score += 4;

        if (!string.IsNullOrWhiteSpace(lookup.City) &&
            ContainsEitherWay(lookup.City, property.City))
        {
            score += 2;
        }

        if (!string.IsNullOrWhiteSpace(lookup.District) &&
            ContainsEitherWay(lookup.District, property.District))
        {
            score += 2;
        }

        if (!string.IsNullOrWhiteSpace(lookup.PropertyType) &&
            Enum.TryParse<PropertyType>(lookup.PropertyType, true, out var propertyType) &&
            property.PropertyType == propertyType)
        {
            score += 2;
        }

        if (lookup.Area is not null && Math.Abs(property.Area - lookup.Area.Value) <= 1m)
            score += 1;

        if (lookup.Bedrooms is not null && property.Bedrooms == lookup.Bedrooms.Value)
            score += 1;

        return score;
    }

    private static bool ContainsEitherWay(string? left, string? right)
    {
        var normalizedLeft = NormalizeText(left);
        var normalizedRight = NormalizeText(right);

        if (string.IsNullOrWhiteSpace(normalizedLeft) || string.IsNullOrWhiteSpace(normalizedRight))
            return false;

        return normalizedLeft.Contains(normalizedRight, StringComparison.OrdinalIgnoreCase) ||
               normalizedRight.Contains(normalizedLeft, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeText(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static string NormalizeUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return uri.GetLeftPart(UriPartial.Path).TrimEnd('/').ToLowerInvariant();
        }

        return value.Trim().TrimEnd('/').ToLowerInvariant();
    }

    private sealed record PropertyCandidate(
        Guid Id,
        string? SourceListingUrl,
        string Title,
        decimal Price,
        string? City,
        string? District,
        PropertyType PropertyType,
        decimal Area,
        int Bedrooms);
}
