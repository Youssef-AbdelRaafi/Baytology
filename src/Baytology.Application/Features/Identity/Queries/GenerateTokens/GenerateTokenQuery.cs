using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Identity.Queries.GenerateTokens;

public record GenerateTokenQuery(
    string Email,
    string Password) : IRequest<Result<TokenResponse>>;