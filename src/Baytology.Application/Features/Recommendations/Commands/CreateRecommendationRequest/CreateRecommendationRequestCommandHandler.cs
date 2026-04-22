using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Recommendations;

using MediatR;

namespace Baytology.Application.Features.Recommendations.Commands.CreateRecommendationRequest;

public class CreateRecommendationRequestCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateRecommendationRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRecommendationRequestCommand request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();

        var recRequestResult = RecommendationRequest.Create(
            request.UserId,
            request.SourceEntityType,
            request.SourceEntityId,
            request.TopN,
            correlationId);

        if (recRequestResult.IsError)
            return recRequestResult.Errors;

        var recRequest = recRequestResult.Value;
        context.RecommendationRequests.Add(recRequest);
        await context.SaveChangesAsync(ct);

        return recRequest.Id;
    }
}
