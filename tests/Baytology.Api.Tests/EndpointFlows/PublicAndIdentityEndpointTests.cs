using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;
using Baytology.Domain.Common.Enums;
using Baytology.Infrastructure.Data;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class PublicAndIdentityEndpointTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Public_and_identity_endpoints_work_end_to_end()
    {
        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/Properties")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/v1/Properties/{factory.SeedData.CatalogPropertyId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/v1/Agents/{TestSeedData.AgentUserId}")).StatusCode);

        var registerResponse = await client.PostAsJsonAsync("/api/identity/register", new
        {
            Email = "newbuyer@test.local",
            Password = "Buyer@Test123",
            DisplayName = "New Buyer",
            Role = "Buyer"
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var generateTokenResponse = await client.PostAsJsonAsync("/api/identity/token/generate", new
        {
            Email = TestSeedData.BuyerEmail,
            Password = TestSeedData.BuyerPassword
        });
        Assert.Equal(HttpStatusCode.OK, generateTokenResponse.StatusCode);

        var tokenPayload = await generateTokenResponse.ReadJsonAsync();
        var accessToken = tokenPayload.GetProperty("accessToken").GetString();
        var refreshToken = tokenPayload.GetProperty("refreshToken").GetString();

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));

        var refreshResponse = await client.PostAsJsonAsync("/api/identity/token/refresh", new
        {
            RefreshToken = refreshToken,
            ExpiredAccessToken = accessToken
        });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshPayload = await refreshResponse.ReadJsonAsync();
        Assert.False(string.IsNullOrWhiteSpace(refreshPayload.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(refreshPayload.GetProperty("refreshToken").GetString()));

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/v1/Properties/{factory.SeedData.CatalogPropertyId}/view", null)).StatusCode);

        var unauthorizedWebhookResponse = await client.PostAsJsonAsync("/api/v1/Payments/webhook", new
        {
            obj = new
            {
                success = true,
                id = "gw-no-token",
                special_reference = factory.SeedData.WebhookPaymentId.ToString()
            }
        });
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedWebhookResponse.StatusCode);

        var authorizedWebhookResponse = await client.PostAsJsonAsync(
            "/api/v1/Payments/webhook?token=webhook-token",
            new
            {
                obj = new
                {
                    success = true,
                    id = "gw-success",
                    special_reference = factory.SeedData.WebhookPaymentId.ToString(),
                    order = new
                    {
                        merchant_order_id = factory.SeedData.WebhookPaymentId.ToString()
                    }
                }
            });
        Assert.Equal(HttpStatusCode.OK, authorizedWebhookResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var webhookPayment = await context.Payments.FindAsync(factory.SeedData.WebhookPaymentId);
        var webhookBooking = await context.Bookings.FindAsync(factory.SeedData.PendingBookingId);
        var pendingProperty = await context.Properties.FindAsync(factory.SeedData.PendingBookingPropertyId);

        Assert.NotNull(webhookPayment);
        Assert.NotNull(webhookBooking);
        Assert.NotNull(pendingProperty);
        Assert.Equal(PaymentStatus.Completed, webhookPayment!.Status);
        Assert.Equal(BookingStatus.Confirmed, webhookBooking!.Status);
        Assert.Equal(PropertyStatus.Rented, pendingProperty!.Status);
        Assert.Equal(2, context.PaymentTransactions.Count(t => t.PaymentId == factory.SeedData.WebhookPaymentId));
        Assert.Equal(1, context.PropertyViews.Count(v => v.PropertyId == factory.SeedData.CatalogPropertyId));
    }

    [Fact]
    public async Task Missing_property_returns_not_found_instead_of_server_error()
    {
        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync($"/api/v1/Properties/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Inactive_user_cannot_generate_token_after_admin_deactivation()
    {
        await factory.ResetDatabaseAsync();

        using var publicClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var adminClient = factory.CreateAuthenticatedClient(TestSeedData.AdminUserId, TestSeedData.AdminEmail, "Admin");

        var email = "inactive-user@test.local";
        var password = "Buyer@Test123";

        var registerResponse = await publicClient.PostAsJsonAsync("/api/identity/register", new
        {
            Email = email,
            Password = password,
            DisplayName = "Inactive User",
            Role = "Buyer"
        });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerPayload = await registerResponse.ReadJsonAsync();
        var userId = registerPayload.GetProperty("userId").GetString();

        var deactivateResponse = await adminClient.PostAsJsonAsync($"/api/v1/Admin/users/{userId}/toggle-status", new
        {
            IsActive = false
        });

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var tokenResponse = await publicClient.PostAsJsonAsync("/api/identity/token/generate", new
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.Forbidden, tokenResponse.StatusCode);
    }
}
