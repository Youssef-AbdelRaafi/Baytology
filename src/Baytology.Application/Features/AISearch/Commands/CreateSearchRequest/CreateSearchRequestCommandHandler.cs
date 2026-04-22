using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.AISearch;

using MediatR;

namespace Baytology.Application.Features.AISearch.Commands.CreateSearchRequest;

public class CreateSearchRequestCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateSearchRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSearchRequestCommand request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();

        var searchRequestResult = SearchRequest.Create(
            request.UserId,
            request.InputType,
            request.SearchEngine,
            correlationId);

        if (searchRequestResult.IsError)
            return searchRequestResult.Errors;

        var searchRequest = searchRequestResult.Value;
        context.SearchRequests.Add(searchRequest);

        // Create type-specific search record
        switch (request.InputType)
        {
            case SearchInputType.Text:
                var textSearch = TextSearch.Create(searchRequest.Id, request.RawQuery);
                if (textSearch.IsError)
                    return textSearch.Errors;

                context.TextSearches.Add(textSearch.Value);
                break;

            case SearchInputType.Voice:
                var voiceSearch = VoiceSearch.Create(searchRequest.Id, request.AudioFileUrl);
                if (voiceSearch.IsError)
                    return voiceSearch.Errors;

                context.VoiceSearches.Add(voiceSearch.Value);
                break;

            case SearchInputType.Image:
                var imageSearch = ImageSearch.Create(searchRequest.Id, request.ImageFileUrl);
                if (imageSearch.IsError)
                    return imageSearch.Errors;

                context.ImageSearches.Add(imageSearch.Value);
                break;
        }

        if (HasAnyFilter(request))
        {
            var filterResult = SearchFilter.Create(
                searchRequest.Id,
                request.City,
                request.District,
                request.PropertyType,
                request.ListingType,
                request.MinPrice,
                request.MaxPrice,
                request.MinArea,
                request.MaxArea,
                request.MinBedrooms,
                request.MaxBedrooms);

            if (filterResult.IsError)
                return filterResult.Errors;

            context.SearchFilters.Add(filterResult.Value);
        }

        await context.SaveChangesAsync(ct);

        return searchRequest.Id;
    }

    private static bool HasAnyFilter(CreateSearchRequestCommand request)
    {
        return !string.IsNullOrWhiteSpace(request.City)
            || !string.IsNullOrWhiteSpace(request.District)
            || !string.IsNullOrWhiteSpace(request.PropertyType)
            || !string.IsNullOrWhiteSpace(request.ListingType)
            || request.MinPrice.HasValue
            || request.MaxPrice.HasValue
            || request.MinArea.HasValue
            || request.MaxArea.HasValue
            || request.MinBedrooms.HasValue
            || request.MaxBedrooms.HasValue;
    }
}
