using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetRefundRequests;

public class GetRefundRequestsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetRefundRequestsQuery, Result<PaginatedList<RefundRequestAdminDto>>>
{
    public async Task<Result<PaginatedList<RefundRequestAdminDto>>> Handle(GetRefundRequestsQuery request, CancellationToken ct)
    {
        var query = context.RefundRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedOnUtc)
            .Select(r => new RefundRequestAdminDto(
                r.Id,
                r.PaymentId,
                r.RequestedBy,
                r.Reason,
                r.Amount,
                r.Status.ToString(),
                r.ReviewedBy,
                r.CreatedOnUtc,
                r.ReviewedOnUtc));

        return await PaginatedList<RefundRequestAdminDto>.CreateAsync(query, request.PageNumber, request.PageSize, ct);
    }
}
