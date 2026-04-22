using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    string UserId,
    string Token) : IRequest<Result<Success>>;
