using Baytology.Application.Features.InternalAi.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.InternalAi.Queries.LookupPropertyMappings;

public sealed record LookupPropertyMappingsQuery(
    IReadOnlyList<PropertyLookupItemDto> Items)
    : IRequest<Result<List<PropertyLookupResultDto>>>;
