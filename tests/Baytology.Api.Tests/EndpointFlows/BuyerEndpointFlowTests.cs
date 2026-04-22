using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;
using Baytology.Domain.Common.Enums;
using Baytology.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class BuyerEndpointFlowTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Buyer_endpoints_work_end_to_end()
    {
        await factory.ResetDatabaseAsync();

        using var buyerClient = factory.CreateAuthenticatedClient(TestSeedData.BuyerUserId, TestSeedData.BuyerEmail, "Buyer");
        using var freshBuyerClient = factory.CreateAuthenticatedClient(TestSeedData.FreshBuyerUserId, TestSeedData.FreshBuyerEmail, "Buyer");

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.GetAsync("/api/v1/identity/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await buyerClient.GetAsync($"/api/v1/UserProfiles/{TestSeedData.BuyerUserId}")).StatusCode);

        var createProfileResponse = await freshBuyerClient.PostAsJsonAsync("/api/v1/UserProfiles", new
        {
            UserId = "",
            DisplayName = "Fresh Buyer",
            AvatarUrl = "https://images.test/fresh-buyer.png",
            Bio = "Fresh buyer bio",
            PhoneNumber = "01000000003",
            PreferredContactMethod = 0
        });
        Assert.Equal(HttpStatusCode.Created, createProfileResponse.StatusCode);
        Assert.NotNull(createProfileResponse.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.PutAsJsonAsync("/api/v1/UserProfiles/me", new
        {
            UserId = "",
            DisplayName = "Buyer Updated",
            AvatarUrl = "https://images.test/buyer-updated.png",
            Bio = "Updated buyer bio",
            PhoneNumber = "01000000009",
            PreferredContactMethod = 2
        })).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.GetAsync("/api/v1/Notifications")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await buyerClient.PatchAsync($"/api/v1/Notifications/{factory.SeedData.NotificationId}/read", null)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.GetAsync("/api/v1/Conversations")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await buyerClient.GetAsync($"/api/v1/Conversations/{factory.SeedData.ConversationId}/messages")).StatusCode);

        var createConversationResponse = await buyerClient.PostAsJsonAsync("/api/v1/Conversations", new
        {
            PropertyId = factory.SeedData.ConversationPropertyId
        });
        Assert.Equal(HttpStatusCode.OK, createConversationResponse.StatusCode);
        var newConversationId = (await createConversationResponse.ReadJsonAsync()).GetProperty("conversationId").GetGuid();

        var sendMessageResponse = await buyerClient.PostAsJsonAsync($"/api/v1/Conversations/{factory.SeedData.ConversationId}/messages", new
        {
            Content = "I want to book a viewing.",
            AttachmentUrl = "https://files.test/attachment.pdf"
        });
        Assert.Equal(HttpStatusCode.OK, sendMessageResponse.StatusCode);
        var sentMessageId = (await sendMessageResponse.ReadJsonAsync()).GetProperty("messageId").GetGuid();

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.PatchAsync($"/api/v1/Conversations/messages/{factory.SeedData.MessageId}/read", null)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.GetAsync("/api/v1/Bookings")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await buyerClient.GetAsync($"/api/v1/Bookings/{factory.SeedData.PendingBookingId}")).StatusCode);

        var createBookingResponse = await buyerClient.PostAsJsonAsync("/api/v1/Bookings", new
        {
            PropertyId = factory.SeedData.BookingPropertyId,
            StartDate = DateTimeOffset.UtcNow.AddDays(15),
            EndDate = DateTimeOffset.UtcNow.AddDays(18),
            Amount = 4200,
            CommissionRate = 0.03,
            Currency = "EGP",
            PayerEmail = TestSeedData.BuyerEmail,
            PayerName = "Buyer One",
            PayerPhone = "01000000001"
        });
        Assert.Equal(HttpStatusCode.Created, createBookingResponse.StatusCode);
        Assert.NotNull(createBookingResponse.Headers.Location);
        var bookingPayload = await createBookingResponse.ReadJsonAsync();
        var newBookingId = bookingPayload.GetProperty("bookingId").GetGuid();
        var newPaymentId = bookingPayload.GetProperty("paymentId").GetGuid();
        Assert.Contains(newPaymentId.ToString("N"), bookingPayload.GetProperty("redirectUrl").GetString(), StringComparison.OrdinalIgnoreCase);

        var refundResponse = await buyerClient.PostAsJsonAsync("/api/v1/Payments/refunds", new
        {
            PaymentId = factory.SeedData.RefundablePaymentId,
            RequestedBy = "",
            Reason = "Need a refund",
            Amount = 5000
        });
        Assert.Equal(HttpStatusCode.OK, refundResponse.StatusCode);
        var refundId = (await refundResponse.ReadJsonAsync()).GetProperty("refundId").GetGuid();

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.GetAsync("/api/v1/Properties/saved")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await buyerClient.PostAsync($"/api/v1/Properties/{factory.SeedData.SavablePropertyId}/save", null)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.PostAsJsonAsync("/api/v1/Properties/reviews", new
        {
            AgentUserId = TestSeedData.AgentUserId,
            ReviewerUserId = "",
            PropertyId = factory.SeedData.ReviewPropertyId,
            Rating = 5,
            Comment = "Professional and responsive."
        })).StatusCode);

        var createSearchResponse = await buyerClient.PostAsJsonAsync("/api/v1/Search", new
        {
            UserId = "",
            InputType = 0,
            SearchEngine = 2,
            RawQuery = "luxury apartment in cairo",
            AudioFileUrl = "",
            ImageFileUrl = "",
            City = "Cairo",
            District = "Nasr City",
            PropertyType = "Apartment",
            ListingType = "Rent",
            MinPrice = 1000,
            MaxPrice = 20000,
            MinArea = 100,
            MaxArea = 300,
            MinBedrooms = 2,
            MaxBedrooms = 4
        });
        Assert.Equal(HttpStatusCode.Accepted, createSearchResponse.StatusCode);
        Assert.NotNull(createSearchResponse.Headers.Location);
        var searchId = (await createSearchResponse.ReadJsonAsync()).GetProperty("searchRequestId").GetGuid();
        var searchDetailsResponse = await buyerClient.GetAsync($"/api/v1/Search/{searchId}");
        Assert.Equal(HttpStatusCode.OK, searchDetailsResponse.StatusCode);
        var searchDetails = await searchDetailsResponse.ReadJsonAsync();
        Assert.Equal("Completed", searchDetails.GetProperty("status").GetString());
        Assert.True(searchDetails.GetProperty("results").GetArrayLength() > 0);

        var createRecommendationResponse = await buyerClient.PostAsJsonAsync("/api/v1/Recommendations", new
        {
            UserId = "",
            SourceEntityType = "Property",
            SourceEntityId = factory.SeedData.CatalogPropertyId.ToString(),
            TopN = 5
        });
        Assert.Equal(HttpStatusCode.Accepted, createRecommendationResponse.StatusCode);
        Assert.NotNull(createRecommendationResponse.Headers.Location);
        var recommendationId = (await createRecommendationResponse.ReadJsonAsync()).GetProperty("requestId").GetGuid();
        var recommendationDetailsResponse = await buyerClient.GetAsync($"/api/v1/Recommendations/{recommendationId}");
        Assert.Equal(HttpStatusCode.OK, recommendationDetailsResponse.StatusCode);
        var recommendationDetails = await recommendationDetailsResponse.ReadJsonAsync();
        Assert.Equal("Completed", recommendationDetails.GetProperty("status").GetString());
        Assert.True(recommendationDetails.GetProperty("results").GetArrayLength() > 0);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var readNotification = await context.Notifications.FindAsync(factory.SeedData.NotificationId);
        var existingMessage = await context.Messages.FindAsync(factory.SeedData.MessageId);
        var newMessage = await context.Messages.FindAsync(sentMessageId);
        var createdConversation = await context.Conversations.FindAsync(newConversationId);
        var createdBooking = await context.Bookings.FindAsync(newBookingId);
        var createdPayment = await context.Payments.FindAsync(newPaymentId);
        var createdRefund = await context.RefundRequests.FindAsync(refundId);
        var createdSearch = await context.SearchRequests.FindAsync(searchId);
        var createdRecommendation = await context.RecommendationRequests.FindAsync(recommendationId);

        Assert.NotNull(readNotification);
        Assert.True(readNotification!.IsRead);
        Assert.NotNull(existingMessage);
        Assert.True(existingMessage!.IsRead);
        Assert.NotNull(newMessage);
        Assert.Equal("https://files.test/attachment.pdf", newMessage!.AttachmentUrl);
        Assert.NotNull(createdConversation);
        Assert.Equal(factory.SeedData.ConversationPropertyId, createdConversation!.PropertyId);
        Assert.NotNull(createdBooking);
        Assert.Equal(BookingStatus.Pending, createdBooking!.Status);
        Assert.NotNull(createdPayment);
        Assert.Equal(PaymentStatus.Escrow, createdPayment!.Status);
        Assert.Equal(1, await context.PaymentTransactions.CountAsync(t => t.PaymentId == newPaymentId));
        Assert.NotNull(createdRefund);
        Assert.Equal(RefundStatus.Pending, createdRefund!.Status);
        Assert.Equal(2, await context.SavedProperties.CountAsync(sp => sp.UserId == TestSeedData.BuyerUserId));
        Assert.Equal(1, await context.AgentReviews.CountAsync(r => r.PropertyId == factory.SeedData.ReviewPropertyId && r.ReviewerUserId == TestSeedData.BuyerUserId));
        Assert.NotNull(createdSearch);
        Assert.Equal(TestSeedData.BuyerUserId, createdSearch!.UserId);
        Assert.Equal(RequestStatus.Completed, createdSearch.Status);
        Assert.True(await context.SearchResults.AnyAsync(result => result.SearchRequestId == searchId));
        Assert.NotNull(createdRecommendation);
        Assert.Equal(TestSeedData.BuyerUserId, createdRecommendation!.RequestedByUserId);
        Assert.Equal(RequestStatus.Completed, createdRecommendation.Status);
        Assert.True(await context.RecommendationResults.AnyAsync(result => result.RequestId == recommendationId));
    }
}
