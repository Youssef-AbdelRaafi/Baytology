using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Common.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetPayments;

public class GetPaymentsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPaymentsQuery, Result<PaginatedList<PaymentAdminDto>>>
{
    public async Task<Result<PaginatedList<PaymentAdminDto>>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var query = context.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedOnUtc)
            .Select(p => new PaymentAdminDto(
                p.Id,
                p.PropertyId,
                context.Properties
                    .Where(property => property.Id == p.PropertyId)
                    .Select(property => property.Title)
                    .FirstOrDefault() ?? "Unknown",
                p.PayerId,
                p.PayeeId,
                p.Amount,
                p.Commission,
                p.NetAmount,
                p.Currency,
                p.Purpose.ToString(),
                p.Status.ToString(),
                context.PaymentTransactions
                    .Where(transaction => transaction.PaymentId == p.Id)
                    .OrderByDescending(transaction => transaction.ProcessedAt)
                    .Select(transaction => transaction.GatewayReference)
                    .FirstOrDefault(),
                context.PaymentTransactions
                    .Where(transaction => transaction.PaymentId == p.Id)
                    .OrderByDescending(transaction => transaction.ProcessedAt)
                    .Select(transaction => transaction.TransactionStatus)
                    .FirstOrDefault(),
                p.CreatedOnUtc));

        return await PaginatedList<PaymentAdminDto>.CreateAsync(query, request.PageNumber, request.PageSize, ct);
    }
}
