using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;
using Baytology.Domain.Common.Enums;
using Baytology.Infrastructure.Data;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class ConversationPaymentNotificationEndpointTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Sending_message_creates_notification_and_only_recipient_can_mark_it_read()
    {
        await factory.ResetDatabaseAsync();

        using var buyerClient = factory.CreateAuthenticatedClient(TestSeedData.BuyerUserId, TestSeedData.BuyerEmail, "Buyer");
        using var agentClient = factory.CreateAuthenticatedClient(TestSeedData.AgentUserId, TestSeedData.AgentEmail, "Agent");

        var sendMessageResponse = await buyerClient.PostAsJsonAsync(
            $"/api/v1/Conversations/{factory.SeedData.ConversationId}/messages",
            new
            {
                Content = "Can we schedule a visit tomorrow?",
                AttachmentUrl = "https://files.test/visit-request.pdf"
            });

        Assert.Equal(HttpStatusCode.OK, sendMessageResponse.StatusCode);
        var messageId = (await sendMessageResponse.ReadJsonAsync()).GetProperty("messageId").GetGuid();

        var senderReadResponse = await buyerClient.PatchAsync($"/api/v1/Conversations/messages/{messageId}/read", null);
        Assert.Equal(HttpStatusCode.Conflict, senderReadResponse.StatusCode);

        var recipientReadResponse = await agentClient.PatchAsync($"/api/v1/Conversations/messages/{messageId}/read", null);
        Assert.Equal(HttpStatusCode.OK, recipientReadResponse.StatusCode);

        var unreadNotificationsResponse = await agentClient.GetAsync("/api/v1/Notifications?unreadOnly=true");
        Assert.Equal(HttpStatusCode.OK, unreadNotificationsResponse.StatusCode);
        var unreadNotifications = await unreadNotificationsResponse.ReadJsonAsync();
        Assert.True(unreadNotifications.GetArrayLength() > 0);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var persistedMessage = await context.Messages.FindAsync(messageId);
        var recipientNotification = await context.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.UserId == TestSeedData.AgentUserId &&
                notification.ReferenceId == messageId.ToString() &&
                notification.Type == NotificationType.NewMessage)
            .OrderByDescending(notification => notification.CreatedOnUtc)
            .FirstOrDefaultAsync();

        Assert.NotNull(persistedMessage);
        Assert.True(persistedMessage!.IsRead);
        Assert.NotNull(persistedMessage.ReadAt);
        Assert.NotNull(recipientNotification);
        Assert.False(recipientNotification!.IsRead);
    }

    [Fact]
    public async Task Duplicate_payment_webhook_is_idempotent_and_creates_single_confirmation_notification_set()
    {
        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var payload = new
        {
            obj = new
            {
                success = true,
                special_reference = factory.SeedData.WebhookPaymentId.ToString()
            }
        };

        var firstResponse = await client.PostAsJsonAsync("/api/v1/Payments/webhook?token=webhook-token", payload);
        var secondResponse = await client.PostAsJsonAsync("/api/v1/Payments/webhook?token=webhook-token", payload);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var payment = await context.Payments.FindAsync(factory.SeedData.WebhookPaymentId);
        var booking = await context.Bookings.FindAsync(factory.SeedData.PendingBookingId);
        var property = await context.Properties.FindAsync(factory.SeedData.PendingBookingPropertyId);
        var paymentNotifications = await context.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.ReferenceId == factory.SeedData.PendingBookingId.ToString() &&
                notification.Type == NotificationType.PaymentUpdate)
            .ToListAsync();

        Assert.NotNull(payment);
        Assert.NotNull(booking);
        Assert.NotNull(property);
        Assert.Equal(PaymentStatus.Completed, payment!.Status);
        Assert.Equal(BookingStatus.Confirmed, booking!.Status);
        Assert.Equal(PropertyStatus.Rented, property!.Status);
        Assert.Equal(2, await context.PaymentTransactions.CountAsync(transaction => transaction.PaymentId == factory.SeedData.WebhookPaymentId));
        Assert.Equal(2, paymentNotifications.Count);
        Assert.Equal(1, paymentNotifications.Count(notification => notification.UserId == TestSeedData.BuyerUserId));
        Assert.Equal(1, paymentNotifications.Count(notification => notification.UserId == TestSeedData.AgentUserId));
    }
}
