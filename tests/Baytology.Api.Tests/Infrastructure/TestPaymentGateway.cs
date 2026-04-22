using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

namespace Baytology.Api.Tests.Infrastructure;

internal sealed class TestPaymentGateway : IPaymentGateway
{
    public Task<Result<PaymentIntentionResponse>> CreatePaymentIntentionAsync(
        decimal amount,
        string currency,
        string payerEmail,
        string payerName,
        string payerPhone,
        Guid paymentId,
        CancellationToken ct = default)
    {
        return Task.FromResult<Result<PaymentIntentionResponse>>(
            new PaymentIntentionResponse(
                $"intent-{paymentId:N}",
                $"secret-{paymentId:N}",
                $"https://checkout.test/payments/{paymentId:N}"));
    }
}
