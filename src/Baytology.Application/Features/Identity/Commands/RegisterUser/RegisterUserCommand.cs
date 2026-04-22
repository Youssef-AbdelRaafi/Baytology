using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName,
    string Role = "Buyer") : IRequest<Result<string>>;
