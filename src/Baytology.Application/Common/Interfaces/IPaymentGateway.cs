using Baytology.Domain.Common.Results;

namespace Baytology.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<Result<PaymentIntentionResponse>> CreatePaymentIntentionAsync(
        decimal amount,
        string currency,
        string payerEmail,
        string payerName,
        string payerPhone,
        Guid paymentId,
        CancellationToken ct = default);
}

public record PaymentIntentionResponse(
    string IntentionId,
    string ClientSecret,
    string RedirectUrl);
