using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetRefundRequests;

public record RefundRequestAdminDto(
    Guid Id,
    Guid PaymentId,
    string RequestedBy,
    string Reason,
    decimal Amount,
    string Status,
    string? ReviewedBy,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? ReviewedOnUtc);

public record GetRefundRequestsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedList<RefundRequestAdminDto>>>;
