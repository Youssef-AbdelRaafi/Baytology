using Baytology.Application.Common.Caching;
using Baytology.Application.Features.AgentDetails.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.AgentDetails.Queries.GetAgentDetail;

public record GetAgentDetailQuery(string UserId) : IRequest<Result<AgentDetailDto>>;
