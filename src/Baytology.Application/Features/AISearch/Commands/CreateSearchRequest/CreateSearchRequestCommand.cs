using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.AISearch;

using MediatR;

namespace Baytology.Application.Features.AISearch.Commands.CreateSearchRequest;

public record CreateSearchRequestCommand(
    string UserId,
    SearchInputType InputType,
    SearchEngine SearchEngine,
    string? RawQuery,
    string? AudioFileUrl,
    string? ImageFileUrl,
    string? City,
    string? District,
    string? PropertyType,
    string? ListingType,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinArea,
    decimal? MaxArea,
    int? MinBedrooms,
    int? MaxBedrooms) : IRequest<Result<Guid>>;
