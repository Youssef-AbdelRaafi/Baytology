using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Properties.Queries.GetPropertySavedState;

public record GetPropertySavedStateQuery(string UserId, Guid PropertyId) : IRequest<Result<bool>>;
