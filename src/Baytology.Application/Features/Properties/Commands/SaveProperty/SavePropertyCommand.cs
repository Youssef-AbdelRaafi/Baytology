using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.SaveProperty;

public record SavePropertyCommand(string UserId, Guid PropertyId) : IRequest<Result<Guid>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.SavedProperties,
        ApplicationCacheTags.SavedPropertiesByUser(UserId)
    ];
}
