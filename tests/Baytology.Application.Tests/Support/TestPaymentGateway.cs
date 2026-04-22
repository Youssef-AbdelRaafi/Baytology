using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

namespace Baytology.Application.Tests.Support;

internal sealed class TestPaymentGateway(Result<PaymentIntentionResponse> result) : IPaymentGateway
{
    public int CallCount { get; private set; }

    public Task<Result<PaymentIntentionResponse>> CreatePaymentIntentionAsync(
        decimal amount,
        string currency,
        string payerEmail,
        string payerName,
        string payerPhone,
        Guid paymentId,
        CancellationToken ct = default)
    {
        CallCount++;
        return Task.FromResult(result);
    }
}
