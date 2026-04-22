using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.DeleteProperty;

public record DeletePropertyCommand(Guid PropertyId, string AgentUserId) : IRequest<Result<Success>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.Properties,
        ApplicationCacheTags.Property(PropertyId),
        ApplicationCacheTags.SavedProperties
    ];
}
