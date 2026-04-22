using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.DeleteAccount;

public record DeleteAccountCommand(
    string UserId) : IRequest<Result<Success>>;
