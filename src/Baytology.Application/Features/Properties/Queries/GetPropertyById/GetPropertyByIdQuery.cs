using Baytology.Application.Common.Caching;
using Baytology.Application.Features.Properties.Dtos;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Queries.GetPropertyById;

public record GetPropertyByIdQuery(Guid Id) : IRequest<Result<PropertyDto>>;
