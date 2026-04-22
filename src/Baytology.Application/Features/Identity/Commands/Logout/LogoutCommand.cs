using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.Logout;

public record LogoutCommand(
    string UserId) : IRequest<Result<Success>>;
