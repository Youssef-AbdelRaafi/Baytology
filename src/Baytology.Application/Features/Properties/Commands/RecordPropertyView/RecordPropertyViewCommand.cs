using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.RecordPropertyView;

public record RecordPropertyViewCommand(Guid PropertyId, string? UserId, string? IpAddress) : IRequest<Result<Guid>>;
