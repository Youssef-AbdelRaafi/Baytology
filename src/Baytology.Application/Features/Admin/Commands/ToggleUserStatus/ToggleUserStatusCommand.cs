using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Admin.Commands.ToggleUserStatus;

public record ToggleUserStatusCommand(string TargetUserId, bool IsActive) : IRequest<Result<Success>>;
