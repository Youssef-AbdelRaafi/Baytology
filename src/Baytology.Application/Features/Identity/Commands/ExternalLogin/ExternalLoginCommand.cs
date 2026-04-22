using Baytology.Application.Features.Identity;
using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ExternalLogin;

public record ExternalLoginCommand(
    string Provider,
    string IdToken) : IRequest<Result<ExternalLoginCommandResponse>>;
