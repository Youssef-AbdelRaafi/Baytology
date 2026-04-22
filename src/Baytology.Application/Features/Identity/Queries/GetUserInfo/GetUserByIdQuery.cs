using Baytology.Application.Features.Identity.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Identity.Queries.GetUserInfo;

public sealed record GetUserByIdQuery(string? UserId) : IRequest<Result<AppUserDto>>;