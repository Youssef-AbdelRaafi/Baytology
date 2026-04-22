using System.Net;
using System.Net.Http.Json;
using Baytology.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Baytology.Api.Tests.EndpointFlows;

public class UserConvenienceEndpointTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task ExternalLogin_creates_user_and_generates_token()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        // Invalid token test
        var invalidResponse = await client.PostAsJsonAsync("/api/v1/identity/external-login", new { Provider = "Google", IdToken = "invalid-token" });
        Assert.Equal(HttpStatusCode.Unauthorized, invalidResponse.StatusCode);

        // Expired token test
        var expiredResponse = await client.PostAsJsonAsync("/api/v1/identity/external-login", new { Provider = "Google", IdToken = "expired-token" });
        Assert.Equal(HttpStatusCode.Unauthorized, expiredResponse.StatusCode);

        // Valid token test (new user)
        var validResponse = await client.PostAsJsonAsync("/api/v1/identity/external-login", new { Provider = "Google", IdToken = "valid-google-token" });
        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);

        var json = await validResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.NotNull(json.GetProperty("tokens").GetProperty("accessToken").GetString());
        Assert.True(json.GetProperty("isNewUser").GetBoolean());

        // Call again to test existing user
        var validResponseExisting = await client.PostAsJsonAsync("/api/v1/identity/external-login", new { Provider = "Google", IdToken = "valid-google-token" });
        var jsonExisting = await validResponseExisting.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.False(jsonExisting.GetProperty("isNewUser").GetBoolean());
    }

    [Fact]
    public async Task ExternalLogin_rejects_deactivated_existing_users()
    {
        await factory.ResetDatabaseAsync();

        using var adminClient = factory.CreateAuthenticatedClient(TestSeedData.AdminUserId, TestSeedData.AdminEmail, "Admin");
        using var anonymousClient = factory.CreateClient();

        var deactivateResponse = await adminClient.PostAsJsonAsync($"/api/v1/Admin/users/{TestSeedData.BuyerUserId}/toggle-status", new
        {
            IsActive = false
        });

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var externalLoginResponse = await anonymousClient.PostAsJsonAsync("/api/v1/identity/external-login", new
        {
            Provider = "Google",
            IdToken = "buyer-google-token"
        });

        Assert.Equal(HttpStatusCode.Forbidden, externalLoginResponse.StatusCode);
    }

    [Fact]
    public async Task ExternalLogin_rejects_soft_deleted_existing_users()
    {
        await factory.ResetDatabaseAsync();

        using var buyerClient = factory.CreateAuthenticatedClient(TestSeedData.BuyerUserId, TestSeedData.BuyerEmail, "Buyer");
        using var anonymousClient = factory.CreateClient();

        var deleteResponse = await buyerClient.DeleteAsync("/api/v1/identity/account");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var externalLoginResponse = await anonymousClient.PostAsJsonAsync("/api/v1/identity/external-login", new
        {
            Provider = "Google",
            IdToken = "buyer-google-token"
        });

        Assert.Equal(HttpStatusCode.Forbidden, externalLoginResponse.StatusCode);
    }

    [Fact]
    public async Task User_convenience_endpoints_work_end_to_end()
    {
        await factory.ResetDatabaseAsync();

        // Use the authenticated buyer client
        using var client = factory.CreateAuthenticatedClient(TestSeedData.BuyerUserId, TestSeedData.BuyerEmail, "Buyer");
        using var anonymousClient = factory.CreateClient();

        // 1. Change password (we can't really verify the db changed easily here since it's identity core, but we can verify 200 OK)
        var changePasswordResponse = await client.PostAsJsonAsync("/api/v1/identity/change-password", new
        {
            CurrentPassword = TestSeedData.BuyerPassword,
            NewPassword = "NewPowerfulPassword123!"
        });
        Assert.Equal(HttpStatusCode.OK, changePasswordResponse.StatusCode);

        // 2. Forgot Password
        var forgotPasswordResponse = await anonymousClient.PostAsJsonAsync("/api/v1/identity/forgot-password", new
        {
            Email = TestSeedData.BuyerEmail
        });
        Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);

        // Verify email was sent via the mock
        using var scope = factory.Services.CreateScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<Baytology.Application.Common.Interfaces.IEmailSender>() as TestEmailSender;
        Assert.NotNull(emailSender);
        Assert.Contains(TestSeedData.BuyerEmail, emailSender.SentPasswordResets);

        // 3. Reset Password (we don't have a real token so we'll just test the validation failure)
        var resetPasswordResponse = await anonymousClient.PostAsJsonAsync("/api/v1/identity/reset-password", new
        {
            Email = TestSeedData.BuyerEmail,
            Token = "invalid-token",
            NewPassword = "AnotherNewPassword123!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, resetPasswordResponse.StatusCode);

        // 4. Resend Confirmation
        var resendConfirmationResponse = await anonymousClient.PostAsJsonAsync("/api/v1/identity/resend-confirmation", new
        {
            Email = TestSeedData.BuyerEmail
        });
        // Returns conflict because the seed user is already confirmed
        Assert.Equal(HttpStatusCode.Conflict, resendConfirmationResponse.StatusCode);

        // 5. Logout
        var logoutResponse = await client.PostAsJsonAsync("/api/v1/identity/logout", new { });
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        // Verify refresh tokens were deleted
        var dbContext = scope.ServiceProvider.GetRequiredService<Baytology.Infrastructure.Data.AppDbContext>();
        var tokens = await dbContext.RefreshTokens.Where(r => r.UserId == TestSeedData.BuyerUserId).ToListAsync();
        Assert.Empty(tokens);

        // 6. Delete Account
        var deleteAccountResponse = await client.DeleteAsync("/api/v1/identity/account");
        Assert.Equal(HttpStatusCode.OK, deleteAccountResponse.StatusCode);

        // Verify IsDeleted is true
        var user = await dbContext.Users.FindAsync(TestSeedData.BuyerUserId);
        Assert.NotNull(user);
        Assert.True(user.IsDeleted);
    }
}
