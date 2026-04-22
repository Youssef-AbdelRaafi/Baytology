using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;
using Baytology.Domain.Common.Enums;
using Baytology.Infrastructure.Data;
using Baytology.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class AdminEndpointFlowTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Admin_endpoints_work_end_to_end()
    {
        await factory.ResetDatabaseAsync();

        using var adminClient = factory.CreateAuthenticatedClient(TestSeedData.AdminUserId, TestSeedData.AdminEmail, "Admin");

        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/agents")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await adminClient.PostAsJsonAsync($"/api/v1/Admin/users/{TestSeedData.FreshBuyerUserId}/toggle-status", new
        {
            IsActive = false
        })).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await adminClient.PostAsJsonAsync($"/api/v1/Admin/users/{TestSeedData.FreshBuyerUserId}/assign-role", new
        {
            Role = "Agent"
        })).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await adminClient.PostAsync($"/api/v1/Admin/agents/{TestSeedData.AgentUserId}/verify", null)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await adminClient.PostAsJsonAsync($"/api/v1/Admin/refunds/{factory.SeedData.AdminRefundRequestId}/review", new
        {
            Approve = true
        })).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/audit-logs")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/payments")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/refunds")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/search-requests")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/recommendation-requests")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/domain-events")).StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var freshBuyer = await userManager.FindByIdAsync(TestSeedData.FreshBuyerUserId);
        var freshBuyerRoles = freshBuyer is null
            ? []
            : await userManager.GetRolesAsync(freshBuyer);
        var agentDetail = await context.AgentDetails.FirstOrDefaultAsync(a => a.UserId == TestSeedData.AgentUserId);
        var refundRequest = await context.RefundRequests.FindAsync(factory.SeedData.AdminRefundRequestId);
        var refundPayment = refundRequest is null
            ? null
            : await context.Payments.FindAsync(refundRequest.PaymentId);

        Assert.NotNull(freshBuyer);
        Assert.True(freshBuyer!.LockoutEnd.HasValue && freshBuyer.LockoutEnd > DateTimeOffset.UtcNow);
        Assert.Contains("Agent", freshBuyerRoles);
        Assert.NotNull(agentDetail);
        Assert.True(agentDetail!.IsVerified);
        Assert.NotNull(refundRequest);
        Assert.Equal(RefundStatus.Processed, refundRequest!.Status);
        Assert.NotNull(refundPayment);
        Assert.Equal(PaymentStatus.Refunded, refundPayment!.Status);
    }
}
