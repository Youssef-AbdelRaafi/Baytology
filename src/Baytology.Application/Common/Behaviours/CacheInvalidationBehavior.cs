using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results.Abstractions;

using MediatR;

namespace Baytology.Application.Common.Behaviours;

public sealed class CacheInvalidationBehavior<TRequest, TResponse>(IQueryCache cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next(ct);

        if (request is ICacheInvalidationRequest invalidationRequest &&
            response is IResult result &&
            result.IsSuccess)
        {
            await cache.RemoveByTagAsync(invalidationRequest.CacheTagsToInvalidate, ct);
        }

        return response;
    }
}
