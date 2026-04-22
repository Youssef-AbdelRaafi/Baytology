using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Identity.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery() : IRequest<Result<List<UserSummaryDto>>>;
