using Asp.Versioning;

using Baytology.Application.Features.Payments.Commands.ProcessWebhook;
using Baytology.Application.Features.Payments.Commands.RequestRefund;
using Baytology.Contracts.Requests.Payments;
using Baytology.Contracts.Responses.Payments;
using Baytology.Infrastructure.Settings;

using System.Security.Cryptography;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
public class PaymentsController(
    ISender sender,
    IOptions<PaymobSettings> paymobOptions,
    IHostEnvironment hostEnvironment) : ApiController
{
    private readonly PaymobSettings _paymobSettings = paymobOptions.Value;

    [HttpGet("dev/checkout")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DevCheckout([FromQuery] Guid paymentId, [FromQuery] string? status, CancellationToken ct)
    {
        if (!IsLocalPaymentSimulationEnabled())
            return NotFound();

        if (paymentId == Guid.Empty)
        {
            return Problem(
                title: "Payment is required",
                detail: "Provide a valid payment id.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            var successUrl = $"/api/v1/Payments/dev/checkout?paymentId={paymentId:D}&status=success";
            var failureUrl = $"/api/v1/Payments/dev/checkout?paymentId={paymentId:D}&status=failed";

            return Content(
                $$"""
                <!doctype html>
                <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <title>Baytology Dev Checkout</title>
                    <style>
                        body { font-family: Arial, sans-serif; max-width: 720px; margin: 48px auto; padding: 0 20px; color: #1f2937; }
                        .card { border: 1px solid #d1d5db; border-radius: 16px; padding: 24px; box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08); }
                        h1 { margin-top: 0; }
                        .actions { display: flex; gap: 12px; margin-top: 20px; flex-wrap: wrap; }
                        a { text-decoration: none; padding: 12px 18px; border-radius: 10px; font-weight: 600; }
                        .success { background: #0f766e; color: white; }
                        .failure { background: #b91c1c; color: white; }
                        code { background: #f3f4f6; padding: 2px 6px; border-radius: 6px; }
                    </style>
                </head>
                <body>
                    <div class="card">
                        <h1>Baytology Dev Checkout</h1>
                        <p>This local checkout simulates the external payment gateway in development.</p>
                        <p>Payment Id: <code>{{paymentId}}</code></p>
                        <div class="actions">
                            <a class="success" href="{{successUrl}}">Complete payment</a>
                            <a class="failure" href="{{failureUrl}}">Fail payment</a>
                        </div>
                    </div>
                </body>
                </html>
                """,
                "text/html");
        }

        var normalizedStatus = status.Trim().ToLowerInvariant();
        if (normalizedStatus is not "success" and not "failed")
        {
            return Problem(
                title: "Invalid status",
                detail: "Use status=success or status=failed.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var rawResponse = JsonSerializer.Serialize(new
        {
            source = "dev-checkout",
            paymentId,
            status = normalizedStatus
        });

        var result = await sender.Send(
            new ProcessPaymentWebhookCommand(
                paymentId,
                $"dev-checkout-{paymentId:N}",
                normalizedStatus,
                rawResponse),
            ct);

        return result.Match(
            _ => Content(
                $$"""
                <!doctype html>
                <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <title>Baytology Dev Checkout</title>
                    <style>
                        body { font-family: Arial, sans-serif; max-width: 720px; margin: 48px auto; padding: 0 20px; color: #1f2937; }
                        .card { border: 1px solid #d1d5db; border-radius: 16px; padding: 24px; box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08); }
                        h1 { margin-top: 0; }
                        .status { font-weight: 700; color: {{(normalizedStatus == "success" ? "#0f766e" : "#b91c1c")}}; }
                    </style>
                </head>
                <body>
                    <div class="card">
                        <h1>Payment simulation finished</h1>
                        <p class="status">Status: {{normalizedStatus}}</p>
                        <p>You can return to the client now. The booking and notifications were updated using the same payment flow used by the webhook handler.</p>
                    </div>
                </body>
                </html>
                """,
                "text/html"),
            Problem);
    }

    [HttpPost("webhook")]
    [EndpointSummary("Paymob payment webhook callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Paymob webhook endpoint. Paymob calls this to update payment/transaction status.")]
    [EndpointName("PaymobWebhook")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_paymobSettings.WebhookToken))
        {
            return Problem(
                title: "Webhook security is not configured",
                detail: "Configure Paymob:WebhookToken before accepting payment callbacks.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (!IsAuthorizedWebhook(Request))
        {
            return Problem(
                title: "Invalid webhook token",
                detail: "The payment callback token is missing or invalid.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var rawBody = await ReadRequestBodyAsync(ct);

        PaymobWebhookRequest? request;

        try
        {
            request = JsonSerializer.Deserialize<PaymobWebhookRequest>(
                rawBody,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException)
        {
            return Problem(
                title: "Invalid webhook payload",
                detail: "The payment callback payload could not be parsed.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (request is null)
        {
            return Problem(
                title: "Invalid webhook payload",
                detail: "The payment callback payload is empty.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var paymentId = TryExtractPaymentId(request);
        var gatewayReference = TryExtractGatewayReference(request) ?? string.Empty;
        var command = new ProcessPaymentWebhookCommand(
            paymentId,
            gatewayReference,
            request.Obj?.Success == true ? "success" : "failed",
            JsonSerializer.Serialize(request));

        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("refunds")]
    [Authorize]
    [EndpointSummary("Request a refund")]
    [ProducesResponseType(typeof(RequestRefundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Requests a refund for a payment owned by the authenticated user. RequestedBy is derived from the JWT.")]
    [EndpointName("RequestRefund")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> RequestRefund([FromBody] RequestRefundRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new RequestRefundCommand(request.PaymentId, userId, request.Reason, request.Amount);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(id => Ok(new RequestRefundResponse(id)), Problem);
    }

    private static Guid? TryExtractPaymentId(PaymobWebhookRequest request)
    {
        var candidates = new[]
        {
            ReadJsonValue(request.Obj?.SpecialReference),
            ReadJsonValue(request.Obj?.Order?.SpecialReference),
            ReadJsonValue(request.Obj?.Order?.MerchantOrderId)
        };

        foreach (var candidate in candidates)
        {
            if (Guid.TryParse(candidate, out var paymentId))
                return paymentId;
        }

        return null;
    }

    private static string? TryExtractGatewayReference(PaymobWebhookRequest request)
    {
        var candidates = new[]
        {
            ReadJsonValue(request.Obj?.Id),
            ReadJsonValue(request.Obj?.Order?.Id),
            ReadJsonValue(request.Obj?.Order?.MerchantOrderId)
        };

        return candidates.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string? ReadJsonValue(JsonElement? value)
    {
        if (!value.HasValue)
            return null;

        return value.Value.ValueKind switch
        {
            JsonValueKind.String => value.Value.GetString(),
            JsonValueKind.Number => value.Value.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => value.Value.GetRawText()
        };
    }

    private bool IsAuthorizedWebhook(HttpRequest request)
    {
        var headerName = string.IsNullOrWhiteSpace(_paymobSettings.WebhookTokenHeaderName)
            ? "X-Webhook-Token"
            : _paymobSettings.WebhookTokenHeaderName;

        var queryParameterName = string.IsNullOrWhiteSpace(_paymobSettings.WebhookTokenQueryParameterName)
            ? "token"
            : _paymobSettings.WebhookTokenQueryParameterName;

        var providedToken = request.Headers[headerName].FirstOrDefault()
            ?? request.Query[queryParameterName].FirstOrDefault();

        return FixedTimeEquals(providedToken, _paymobSettings.WebhookToken);
    }

    private async Task<string> ReadRequestBodyAsync(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: false);
        return await reader.ReadToEndAsync(ct);
    }

    private static bool FixedTimeEquals(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        if (leftBytes.Length != rightBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private bool IsLocalPaymentSimulationEnabled()
    {
        return hostEnvironment.IsDevelopment() && _paymobSettings.EnableLocalSimulation;
    }
}
