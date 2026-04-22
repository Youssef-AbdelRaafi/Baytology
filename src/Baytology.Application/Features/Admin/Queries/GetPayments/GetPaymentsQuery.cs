using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Common.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetPayments;

public record PaymentAdminDto(
    Guid Id,
    Guid PropertyId,
    string PropertyTitle,
    string PayerId,
    string PayeeId,
    decimal Amount,
    decimal Commission,
    decimal NetAmount,
    string Currency,
    string Purpose,
    string Status,
    string? LatestGatewayReference,
    string? LatestTransactionStatus,
    DateTimeOffset CreatedOnUtc);

public record GetPaymentsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedList<PaymentAdminDto>>>;
