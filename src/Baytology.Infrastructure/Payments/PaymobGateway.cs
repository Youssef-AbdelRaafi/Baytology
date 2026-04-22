using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Infrastructure.Settings;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.Payments;

public class PaymobGateway(
    HttpClient httpClient,
    IOptions<PaymobSettings> settings,
    IHostEnvironment hostEnvironment,
    ILogger<PaymobGateway> logger) : IPaymentGateway
{
    private readonly PaymobSettings _settings = settings.Value;

    public async Task<Result<PaymentIntentionResponse>> CreatePaymentIntentionAsync(
        decimal amount,
        string currency,
        string payerEmail,
        string payerName,
        string payerPhone,
        Guid paymentId,
        CancellationToken ct = default)
    {
        if (_settings.EnableLocalSimulation && hostEnvironment.IsDevelopment())
        {
            var simulatedPaymentId = paymentId.ToString("D");
            logger.LogInformation("Using local payment simulation for payment {PaymentId}.", simulatedPaymentId);

            return new PaymentIntentionResponse(
                $"dev-intent-{paymentId:N}",
                $"dev-secret-{paymentId:N}",
                $"/api/v1/Payments/dev/checkout?paymentId={simulatedPaymentId}");
        }

        if (string.IsNullOrWhiteSpace(_settings.SecretKey) ||
            string.IsNullOrWhiteSpace(_settings.PublicKey) ||
            _settings.IntegrationId <= 0)
        {
            return ApplicationErrors.Paymob.NotConfigured;
        }

        try
        {
            var requestBody = new
            {
                amount = (int)(amount * 100), // Paymob uses piasters
                currency,
                merchant_order_id = paymentId.ToString(),
                payment_methods = new[] { _settings.IntegrationId },
                items = new[]
                {
                    new
                    {
                        name = "Property Payment",
                        amount = (int)(amount * 100),
                        description = $"Payment {paymentId}",
                        quantity = 1
                    }
                },
                billing_data = new
                {
                    first_name = payerName,
                    last_name = ".",
                    email = payerEmail,
                    phone_number = payerPhone,
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    city = "Cairo",
                    country = "EG",
                    state = "NA"
                },
                special_reference = paymentId.ToString()
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", _settings.SecretKey);

            using var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                logger.LogError("Paymob API error: {StatusCode} - {Body}", response.StatusCode, errorBody);
                return ApplicationErrors.Paymob.ApiError(response.StatusCode.ToString());
            }

            var content = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

            if (!content.TryGetProperty("client_secret", out var clientSecretElement) ||
                string.IsNullOrWhiteSpace(clientSecretElement.GetString()) ||
                !content.TryGetProperty("id", out var intentionIdElement) ||
                string.IsNullOrWhiteSpace(intentionIdElement.GetString()))
            {
                logger.LogError("Paymob response is missing required fields. Response body: {Body}", content.GetRawText());
                return ApplicationErrors.Paymob.ApiError("invalid response");
            }

            var clientSecret = clientSecretElement.GetString()!;
            var intentionId = intentionIdElement.GetString()!;

            return new PaymentIntentionResponse(
                intentionId,
                clientSecret,
                $"https://accept.paymob.com/unifiedcheckout/?publicKey={_settings.PublicKey}&clientSecret={clientSecret}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Paymob payment intention");
            return ApplicationErrors.Paymob.GatewayError(ex.Message);
        }
    }
}
