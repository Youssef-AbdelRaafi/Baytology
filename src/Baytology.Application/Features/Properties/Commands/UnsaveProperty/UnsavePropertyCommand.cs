using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Properties.Commands.UnsaveProperty;

public record UnsavePropertyCommand(string UserId, Guid PropertyId) : IRequest<Result<Success>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.SavedProperties,
        ApplicationCacheTags.SavedPropertiesByUser(UserId)
    ];
}
