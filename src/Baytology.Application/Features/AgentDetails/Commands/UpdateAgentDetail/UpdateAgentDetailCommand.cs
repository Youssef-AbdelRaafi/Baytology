using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.AgentDetails.Commands.UpdateAgentDetail;

public record UpdateAgentDetailCommand(
    string UserId,
    string? AgencyName,
    string? LicenseNumber,
    decimal CommissionRate) : IRequest<Result<Success>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.AgentDetails,
        ApplicationCacheTags.AgentDetail(UserId),
        ApplicationCacheTags.Properties
    ];
}
