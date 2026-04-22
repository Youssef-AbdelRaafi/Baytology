using Baytology.Domain.Common.Enums;
using Baytology.Domain.Payments;
using Baytology.Domain.Payments.Events;

namespace Baytology.Domain.Tests.Payments;

public sealed class PaymentTests
{
    [Fact]
    public void Create_sets_pending_status_and_calculates_commission_and_net_amount()
    {
        var result = Payment.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            1000m,
            0.1m,
            PaymentPurpose.Deposit);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Pending, result.Value.Status);
        Assert.Equal(100m, result.Value.Commission);
        Assert.Equal(900m, result.Value.NetAmount);
    }

    [Fact]
    public void Complete_marks_payment_completed_and_raises_domain_event_once()
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            1000m,
            0.1m,
            PaymentPurpose.Deposit).Value;

        var firstResult = payment.Complete();
        var secondResult = payment.Complete();

        Assert.True(firstResult);
        Assert.False(secondResult);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Single(payment.DomainEvents);
        Assert.IsType<PaymentCompletedEvent>(payment.DomainEvents.Single());
    }

    [Fact]
    public void ReleaseEscrow_marks_payment_completed_sets_release_time_and_raises_domain_event()
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            1000m,
            0.1m,
            PaymentPurpose.Deposit).Value;

        payment.MarkAsEscrow();

        var result = payment.ReleaseEscrow();

        Assert.True(result);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.NotNull(payment.EscrowReleasedAt);
        Assert.Single(payment.DomainEvents);
        Assert.IsType<PaymentCompletedEvent>(payment.DomainEvents.Single());
    }

    [Fact]
    public void MarkRefunded_requires_completed_payment()
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            1000m,
            0.1m,
            PaymentPurpose.Deposit).Value;

        var beforeCompletion = payment.MarkRefunded();
        payment.Complete();
        var afterCompletion = payment.MarkRefunded();

        Assert.False(beforeCompletion);
        Assert.True(afterCompletion);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
    }

    [Fact]
    public void Create_rejects_invalid_commission_rate()
    {
        var result = Payment.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            1000m,
            1m,
            PaymentPurpose.Deposit);

        Assert.True(result.IsError);
        Assert.Equal("Payment_CommissionRate_Invalid", result.TopError.Code);
    }
}
