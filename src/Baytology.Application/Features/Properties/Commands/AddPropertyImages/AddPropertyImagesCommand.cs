using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.AddPropertyImages;

public record AddPropertyImagesCommand(Guid PropertyId, string AgentUserId, List<string> ImageUrls) : IRequest<Result<Success>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.Properties,
        ApplicationCacheTags.Property(PropertyId),
        ApplicationCacheTags.SavedProperties
    ];
}
