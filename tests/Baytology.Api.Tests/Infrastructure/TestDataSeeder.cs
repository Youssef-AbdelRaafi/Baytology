using Baytology.Domain.AgentDetails;
using Baytology.Domain.AISearch;
using Baytology.Domain.AuditLogs;
using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Conversations;
using Baytology.Domain.DomainEvents;
using Baytology.Domain.Identity;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;
using Baytology.Domain.Properties;
using Baytology.Domain.Recommendations;
using Baytology.Domain.UserProfiles;
using Baytology.Infrastructure.Data;
using Baytology.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Baytology.Api.Tests.Infrastructure;

internal static class TestDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, TestSeedData data)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Role.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await CreateUserAsync(userManager, TestSeedData.AdminUserId, TestSeedData.AdminEmail, TestSeedData.AdminPassword, Role.Admin);
        await CreateUserAsync(userManager, TestSeedData.AgentUserId, TestSeedData.AgentEmail, TestSeedData.AgentPassword, Role.Agent);
        await CreateUserAsync(userManager, TestSeedData.BuyerUserId, TestSeedData.BuyerEmail, TestSeedData.BuyerPassword, Role.Buyer);
        await CreateUserAsync(userManager, TestSeedData.FreshBuyerUserId, TestSeedData.FreshBuyerEmail, TestSeedData.FreshBuyerPassword, Role.Buyer);

        context.UserProfiles.AddRange(
            UserProfile.Create(TestSeedData.BuyerUserId, "Buyer One", "https://images.test/buyer.png", "Buyer bio", "01000000001", ContactMethod.Email).Value,
            UserProfile.Create(TestSeedData.AgentUserId, "Agent One", "https://images.test/agent.png", "Trusted property agent", "01000000002", ContactMethod.Phone).Value);

        context.AgentDetails.Add(AgentDetail.Create(TestSeedData.AgentUserId, "Baytology Estates", "LIC-2026", 0.03m).Value);

        var catalogProperty = CreateProperty("Cairo Downtown Apartment", ListingType.Sale, "Downtown", "Cairo");
        var conversationProperty = CreateProperty("Nasr City Rental", ListingType.Rent, "Nasr City", "Cairo");
        var savableProperty = CreateProperty("Zayed Family Home", ListingType.Sale, "Sheikh Zayed", "Giza");
        var bookingProperty = CreateProperty("Maadi Booking Candidate", ListingType.Rent, "Maadi", "Cairo");
        var pendingBookingProperty = CreateProperty("Heliopolis Pending Booking", ListingType.Rent, "Heliopolis", "Cairo");
        var reviewProperty = CreateProperty("Alex Sold Chalet", ListingType.Sale, "Sidi Abdel Rahman", "Alexandria");

        data.CatalogPropertyId = catalogProperty.Id;
        data.ConversationPropertyId = conversationProperty.Id;
        data.SavablePropertyId = savableProperty.Id;
        data.BookingPropertyId = bookingProperty.Id;
        data.PendingBookingPropertyId = pendingBookingProperty.Id;
        data.ReviewPropertyId = reviewProperty.Id;

        context.Properties.AddRange(catalogProperty, conversationProperty, savableProperty, bookingProperty, pendingBookingProperty, reviewProperty);
        context.PropertyAmenities.AddRange(
            CreateAmenity(catalogProperty.Id, true, true),
            CreateAmenity(conversationProperty.Id, false, true),
            CreateAmenity(savableProperty.Id, true, false),
            CreateAmenity(bookingProperty.Id, false, false),
            CreateAmenity(pendingBookingProperty.Id, true, true),
            CreateAmenity(reviewProperty.Id, true, true));
        context.PropertyImages.AddRange(
            PropertyImage.Create(catalogProperty.Id, "https://images.test/catalog-1.jpg", true, 1).Value,
            PropertyImage.Create(conversationProperty.Id, "https://images.test/conversation-1.jpg", true, 1).Value,
            PropertyImage.Create(reviewProperty.Id, "https://images.test/review-1.jpg", true, 1).Value);

        context.SavedProperties.Add(SavedProperty.Create(TestSeedData.BuyerUserId, catalogProperty.Id).Value);

        var conversation = Conversation.Create(catalogProperty.Id, TestSeedData.BuyerUserId, TestSeedData.AgentUserId).Value;
        var message = conversation.SendMessage(TestSeedData.AgentUserId, "Welcome to Baytology!").Value;
        data.ConversationId = conversation.Id;
        data.MessageId = message.Id;
        context.Conversations.Add(conversation);
        context.Messages.Add(message);

        var notification = Notification.Create(
            TestSeedData.BuyerUserId,
            NotificationType.NewMessage,
            "New message",
            "Agent replied to your inquiry.",
            conversation.Id.ToString(),
            ReferenceType.Message).Value;
        data.NotificationId = notification.Id;
        context.Notifications.Add(notification);

        var pendingBooking = Booking.Create(
            pendingBookingProperty.Id,
            TestSeedData.BuyerUserId,
            TestSeedData.AgentUserId,
            DateTimeOffset.UtcNow.AddDays(2),
            DateTimeOffset.UtcNow.AddDays(5)).Value;
        var pendingPayment = Payment.Create(
            pendingBookingProperty.Id,
            TestSeedData.BuyerUserId,
            TestSeedData.AgentUserId,
            2500m,
            0.03m,
            PaymentPurpose.Deposit,
            "EGP").Value;
        pendingPayment.MarkAsEscrow();
        pendingBooking.AttachPayment(pendingPayment.Id);
        data.PendingBookingId = pendingBooking.Id;
        data.WebhookPaymentId = pendingPayment.Id;
        context.Bookings.Add(pendingBooking);
        context.Payments.Add(pendingPayment);
        context.PaymentTransactions.Add(pendingPayment.RecordTransaction("gw-pending", "Paymob", "created", null).Value);

        var reviewBooking = Booking.Create(
            reviewProperty.Id,
            TestSeedData.BuyerUserId,
            TestSeedData.AgentUserId,
            DateTimeOffset.UtcNow.AddDays(7),
            DateTimeOffset.UtcNow.AddDays(10)).Value;
        var reviewPayment = Payment.Create(
            reviewProperty.Id,
            TestSeedData.BuyerUserId,
            TestSeedData.AgentUserId,
            5000m,
            0.03m,
            PaymentPurpose.FullPayment,
            "EGP").Value;
        reviewPayment.MarkAsEscrow();
        reviewPayment.Complete();
        reviewBooking.AttachPayment(reviewPayment.Id);
        reviewBooking.Confirm();
        reviewProperty.ChangeStatus(PropertyStatus.Sold);
        data.ReviewBookingId = reviewBooking.Id;
        data.ReviewPaymentId = reviewPayment.Id;
        data.RefundablePaymentId = reviewPayment.Id;
        context.Bookings.Add(reviewBooking);
        context.Payments.Add(reviewPayment);
        context.PaymentTransactions.Add(reviewPayment.RecordTransaction("gw-completed", "Paymob", "success", null).Value);

        var adminRefundPayment = Payment.Create(
            catalogProperty.Id,
            TestSeedData.BuyerUserId,
            TestSeedData.AgentUserId,
            1800m,
            0.03m,
            PaymentPurpose.Deposit,
            "EGP").Value;
        adminRefundPayment.MarkAsEscrow();
        adminRefundPayment.Complete();
        context.Payments.Add(adminRefundPayment);
        context.PaymentTransactions.Add(adminRefundPayment.RecordTransaction("gw-admin-refund", "Paymob", "success", null).Value);

        var adminRefundRequest = RefundRequest.Create(adminRefundPayment.Id, TestSeedData.BuyerUserId, "Need admin review", 1800m).Value;
        data.AdminRefundRequestId = adminRefundRequest.Id;
        context.RefundRequests.Add(adminRefundRequest);

        var searchRequest = SearchRequest.Create(TestSeedData.BuyerUserId, SearchInputType.Text, SearchEngine.Hybrid, "corr-search").Value;
        searchRequest.Complete(1);
        context.SearchRequests.Add(searchRequest);
        context.TextSearches.Add(TextSearch.Create(searchRequest.Id, "apartment in cairo").Value);
        context.SearchResults.Add(SearchResult.Create(searchRequest.Id, catalogProperty.Id, 1, 0.98f, "Hybrid", catalogProperty.Title, catalogProperty.Price, catalogProperty.City, catalogProperty.Status.ToString()).Value);

        var recommendationRequest = RecommendationRequest.Create(TestSeedData.BuyerUserId, "Property", catalogProperty.Id.ToString(), 5, "corr-rec").Value;
        recommendationRequest.Complete();
        context.RecommendationRequests.Add(recommendationRequest);
        context.RecommendationResults.Add(RecommendationResult.Create(recommendationRequest.Id, savableProperty.Id, null, 0.91f, 1, savableProperty.Title, savableProperty.Price).Value);

        context.AuditLogs.Add(AuditLog.Create(TestSeedData.AdminUserId, "Updated", "Property", catalogProperty.Id.ToString(), "{\"Status\":\"Available\"}", "{\"Status\":\"Featured\"}", "127.0.0.1").Value);
        context.DomainEventLogs.Add(DomainEventLog.Create("SearchRequestedEvent", searchRequest.Id.ToString(), nameof(SearchRequest), "{\"CorrelationId\":\"corr-search\"}").Value);

        await context.SaveChangesAsync();
    }

    private static async Task CreateUserAsync(UserManager<AppUser> userManager, string userId, string email, string password, string role)
    {
        var user = new AppUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);
    }

    private static Property CreateProperty(string title, ListingType listingType, string district, string city)
    {
        var property = Property.Create(
            TestSeedData.AgentUserId,
            title,
            $"{title} description",
            PropertyType.Apartment,
            listingType,
            listingType == ListingType.Sale ? 1500000m : 12000m,
            150m,
            3,
            2,
            city,
            district).Value;

        property.SetLocation($"{district} Street", city, district, "11511", 30.1m, 31.3m);
        return property;
    }

    private static PropertyAmenity CreateAmenity(Guid propertyId, bool hasParking, bool hasPool)
    {
        var amenity = PropertyAmenity.Create(propertyId).Value;
        amenity.Update(hasParking, hasPool, true, true, true, true, false, true, FurnishingStatus.FullyFurnished, ViewType.City);
        return amenity;
    }
}
