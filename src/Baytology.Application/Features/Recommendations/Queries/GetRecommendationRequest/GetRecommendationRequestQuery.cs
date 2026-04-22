using Baytology.Application.Common.Caching;
using Baytology.Application.Features.Recommendations.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Recommendations.Queries.GetRecommendationRequest;

public record GetRecommendationRequestQuery(Guid Id, string UserId) : IRequest<Result<RecommendationRequestDto>>;
