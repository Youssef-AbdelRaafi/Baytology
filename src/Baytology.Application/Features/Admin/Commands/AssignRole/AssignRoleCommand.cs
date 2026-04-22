using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Admin.Commands.AssignRole;

public record AssignRoleCommand(string TargetUserId, string Role) : IRequest<Result<Success>>;
