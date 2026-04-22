using Baytology.Application.Features.Payments.Commands.ProcessWebhook;
using Baytology.Application.Tests.Support;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;

using Microsoft.Extensions.Logging.Abstractions;

namespace Baytology.Application.Tests.Payments;

public sealed class ProcessPaymentWebhookCommandHandlerTests
{
    [Fact]
    public async Task Completed_payment_is_not_downgraded_by_late_failed_webhook()
    {
        await using var context = TestDbContextFactory.Create();
        var notifications = new TestNotificationService();
        var handler = new ProcessPaymentWebhookCommandHandler(
            context,
            notifications,
            NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);

        var payment = CreatePayment();
        payment.Complete();

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var command = new ProcessPaymentWebhookCommand(payment.Id, "gw-late-failure", "failed", "{}");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Empty(notifications.SentNotifications);
    }

    [Fact]
    public async Task Duplicate_webhook_does_not_create_duplicate_transactions()
    {
        await using var context = TestDbContextFactory.Create();
        var notifications = new TestNotificationService();
        var handler = new ProcessPaymentWebhookCommandHandler(
            context,
            notifications,
            NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);

        var payment = CreatePayment();
        payment.MarkAsEscrow();

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var command = new ProcessPaymentWebhookCommand(payment.Id, "gw-duplicate", "success", "{\"success\":true}");

        var firstResult = await handler.Handle(command, CancellationToken.None);
        var secondResult = await handler.Handle(command, CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal(1, context.PaymentTransactions.Count(t => t.PaymentId == payment.Id));
    }

    [Fact]
    public async Task Success_webhook_can_recover_payment_after_earlier_failed_callback()
    {
        await using var context = TestDbContextFactory.Create();
        var notifications = new TestNotificationService();
        var handler = new ProcessPaymentWebhookCommandHandler(
            context,
            notifications,
            NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);

        var payment = CreatePayment();
        payment.MarkAsEscrow();

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var failedCommand = new ProcessPaymentWebhookCommand(payment.Id, "gw-retry-failed", "failed", "{}");
        var successCommand = new ProcessPaymentWebhookCommand(payment.Id, "gw-retry-success", "success", "{}");

        await handler.Handle(failedCommand, CancellationToken.None);
        await handler.Handle(successCommand, CancellationToken.None);

        Assert.Equal(PaymentStatus.Completed, payment.Status);
    }

    [Fact]
    public async Task Missing_gateway_reference_uses_payment_scoped_fallback_for_idempotency()
    {
        await using var context = TestDbContextFactory.Create();
        var notifications = new TestNotificationService();
        var handler = new ProcessPaymentWebhookCommandHandler(
            context,
            notifications,
            NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);

        var payment = CreatePayment();
        payment.MarkAsEscrow();

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var command = new ProcessPaymentWebhookCommand(payment.Id, string.Empty, "success", "{\"success\":true}");

        var firstResult = await handler.Handle(command, CancellationToken.None);
        var secondResult = await handler.Handle(command, CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal(1, context.PaymentTransactions.Count(t => t.PaymentId == payment.Id));
        Assert.Equal(
            $"payment:{payment.Id:N}:success",
            context.PaymentTransactions.Single(t => t.PaymentId == payment.Id).GatewayReference);
    }

    [Fact]
    public async Task Failed_payment_webhook_remains_successful_when_notification_delivery_fails()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new ProcessPaymentWebhookCommandHandler(
            context,
            new ThrowingNotificationService(),
            NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);

        var payment = CreatePayment();
        payment.MarkAsEscrow();

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var command = new ProcessPaymentWebhookCommand(payment.Id, "gw-failure-notification", "failed", "{}");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(1, context.PaymentTransactions.Count(t => t.PaymentId == payment.Id));
    }

    private static Payment CreatePayment()
    {
        return Payment.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            1000m,
            0.1m,
            PaymentPurpose.Deposit,
            "EGP").Value;
    }

    private sealed class ThrowingNotificationService : INotificationService
    {
        public Task SendAsync(Notification notification, CancellationToken ct = default)
            => throw new InvalidOperationException("Notification persistence is unavailable.");
    }
}
