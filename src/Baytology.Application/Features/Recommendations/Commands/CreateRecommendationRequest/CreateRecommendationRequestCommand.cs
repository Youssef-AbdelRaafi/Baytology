using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Recommendations;

using MediatR;

namespace Baytology.Application.Features.Recommendations.Commands.CreateRecommendationRequest;

public record CreateRecommendationRequestCommand(
    string UserId,
    string SourceEntityType,
    string? SourceEntityId,
    int TopN = 10) : IRequest<Result<Guid>>;
