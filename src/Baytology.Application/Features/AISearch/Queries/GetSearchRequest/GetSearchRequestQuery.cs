using Baytology.Application.Common.Caching;
using Baytology.Application.Features.AISearch.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.AISearch.Queries.GetSearchRequest;

public record GetSearchRequestQuery(Guid Id, string UserId) : IRequest<Result<SearchRequestDto>>;
