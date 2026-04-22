using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ForgotPassword;

public record ForgotPasswordCommand(
    string Email) : IRequest<Result<Success>>;
