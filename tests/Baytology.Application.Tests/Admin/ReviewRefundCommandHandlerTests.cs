using Baytology.Application.Features.Admin.Commands.ReviewRefund;
using Baytology.Application.Tests.Support;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Payments;

namespace Baytology.Application.Tests.Admin;

public sealed class ReviewRefundCommandHandlerTests
{
    [Fact]
    public async Task Approving_refund_marks_payment_refunded_and_request_processed()
    {
        await using var context = TestDbContextFactory.Create();
        var notifications = new TestNotificationService();
        var handler = new ReviewRefundCommandHandler(context, notifications);

        var payment = Payment.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            1500m,
            0.05m,
            PaymentPurpose.Deposit,
            "EGP").Value;
        payment.Complete();

        var refund = RefundRequest.Create(payment.Id, payment.PayerId, "Changed my mind", 1500m).Value;

        context.Payments.Add(payment);
        context.RefundRequests.Add(refund);
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new ReviewRefundCommand(refund.Id, true, "admin-1"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(RefundStatus.Processed, refund.Status);
        Assert.Single(notifications.SentNotifications);
    }
}
