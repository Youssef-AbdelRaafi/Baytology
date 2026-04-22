using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.CreateAgentReview;

public class CreateAgentReviewCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateAgentReviewCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAgentReviewCommand request, CancellationToken ct)
    {
        if (request.PropertyId.HasValue)
        {
            var property = await context.Properties
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PropertyId.Value, ct);

            if (property is null)
                return ApplicationErrors.Property.NotFound;

            if (property.AgentUserId != request.AgentUserId)
                return ApplicationErrors.Review.AgentMismatch;
        }

        var hasConfirmedBooking = await context.Bookings.AnyAsync(
            b => b.UserId == request.ReviewerUserId &&
                 b.AgentUserId == request.AgentUserId &&
                 b.Status == BookingStatus.Confirmed &&
                 (!request.PropertyId.HasValue || b.PropertyId == request.PropertyId.Value),
            ct);

        if (!hasConfirmedBooking)
            return ApplicationErrors.Review.BookingRequired;

        var alreadyReviewed = await context.AgentReviews.AnyAsync(
            r => r.AgentUserId == request.AgentUserId &&
                 r.ReviewerUserId == request.ReviewerUserId &&
                 r.PropertyId == request.PropertyId,
            ct);

        if (alreadyReviewed)
            return ApplicationErrors.Review.AlreadySubmitted;

        var reviewResult = AgentReview.Create(
            request.AgentUserId,
            request.ReviewerUserId,
            request.PropertyId,
            request.Rating,
            request.Comment);

        if (reviewResult.IsError)
            return reviewResult.Errors;

        context.AgentReviews.Add(reviewResult.Value);
        await context.SaveChangesAsync(ct);

        // Update Agent Rating and Review Count
        var agentDetail = await context.AgentDetails
            .FirstOrDefaultAsync(a => a.UserId == request.AgentUserId, ct);

        if (agentDetail is not null)
        {
            var agentReviews = await context.AgentReviews
                .Where(r => r.AgentUserId == request.AgentUserId)
                .ToListAsync(ct);

            var newCount = agentReviews.Count;
            var newRating = newCount > 0 ? (decimal)agentReviews.Average(r => r.Rating) : 0;

            agentDetail.UpdateRating(newRating, newCount);
            await context.SaveChangesAsync(ct);
        }

        return reviewResult.Value.Id;
    }
}
