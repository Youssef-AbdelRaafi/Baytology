using Baytology.Infrastructure.Payments;
using Baytology.Infrastructure.Settings;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace Baytology.Application.Tests.Payments;

public sealed class PaymobGatewayTests
{
    [Fact]
    public async Task Development_local_simulation_takes_precedence_even_when_gateway_keys_are_present()
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:5201")
        };

        var gateway = new PaymobGateway(
            httpClient,
            Options.Create(new PaymobSettings
            {
                EnableLocalSimulation = true,
                SecretKey = "configured-secret",
                PublicKey = "configured-public",
                IntegrationId = 123456
            }),
            new TestHostEnvironment("Development"),
            NullLogger<PaymobGateway>.Instance);

        var paymentId = Guid.NewGuid();
        var result = await gateway.CreatePaymentIntentionAsync(
            2500m,
            "EGP",
            "buyer@test.local",
            "Buyer Test",
            "01000000000",
            paymentId,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"dev-intent-{paymentId:N}", result.Value.IntentionId);
        Assert.Equal($"/api/v1/Payments/dev/checkout?paymentId={paymentId:D}", result.Value.RedirectUrl);
    }

    [Fact]
    public async Task Production_gateway_uses_per_request_token_header_and_parses_redirect_response()
    {
        var handler = new RecordingHandler("""
            {"id":"intent-123","client_secret":"secret-123"}
            """);

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://accept.paymob.com/")
        };

        var gateway = new PaymobGateway(
            httpClient,
            Options.Create(new PaymobSettings
            {
                EnableLocalSimulation = false,
                SecretKey = "configured-secret",
                PublicKey = "configured-public",
                IntegrationId = 123456,
                BaseUrl = "v1/intention/"
            }),
            new TestHostEnvironment("Production"),
            NullLogger<PaymobGateway>.Instance);

        var result = await gateway.CreatePaymentIntentionAsync(
            2500m,
            "EGP",
            "buyer@test.local",
            "Buyer Test",
            "01000000000",
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Token", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("configured-secret", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        Assert.Equal("intent-123", result.Value.IntentionId);
        Assert.Contains("configured-public", result.Value.RedirectUrl, StringComparison.Ordinal);
        Assert.Contains("secret-123", result.Value.RedirectUrl, StringComparison.Ordinal);
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Baytology.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private sealed class RecordingHandler(string responseBody) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        }
    }
}
