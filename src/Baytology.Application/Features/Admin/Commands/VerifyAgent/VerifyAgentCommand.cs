using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Commands.VerifyAgent;

public record VerifyAgentCommand(string AgentUserId) : IRequest<Result<Success>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.AgentDetails,
        ApplicationCacheTags.AgentDetail(AgentUserId),
        ApplicationCacheTags.Properties
    ];
}
