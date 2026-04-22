using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ResendConfirmation;

public record ResendConfirmationCommand(
    string Email) : IRequest<Result<Success>>;
