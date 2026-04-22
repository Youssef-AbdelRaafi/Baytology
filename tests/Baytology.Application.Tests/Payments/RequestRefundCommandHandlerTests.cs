using Baytology.Application.Features.Payments.Commands.RequestRefund;
using Baytology.Application.Tests.Support;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Payments;

namespace Baytology.Application.Tests.Payments;

public sealed class RequestRefundCommandHandlerTests
{
    [Fact]
    public async Task Partial_refund_request_is_rejected_when_payment_model_supports_full_refunds_only()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new RequestRefundCommandHandler(context);

        var payment = CreateCompletedPayment(2000m);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new RequestRefundCommand(payment.Id, payment.PayerId, "Need a partial refund", 500m),
            CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Refund.AmountMustMatchPayment", result.TopError.Code);
        Assert.Empty(context.RefundRequests);
    }

    [Fact]
    public async Task Full_refund_request_is_created_for_completed_payment()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new RequestRefundCommandHandler(context);

        var payment = CreateCompletedPayment(2000m);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new RequestRefundCommand(payment.Id, payment.PayerId, "Need a full refund", 2000m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(context.RefundRequests);
        Assert.Equal(RefundStatus.Pending, context.RefundRequests.Single().Status);
    }

    private static Payment CreateCompletedPayment(decimal amount)
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            amount,
            0.03m,
            PaymentPurpose.Deposit,
            "EGP").Value;

        payment.MarkAsEscrow();
        payment.Complete();
        return payment;
    }
}
