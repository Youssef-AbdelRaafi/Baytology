using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;


namespace Baytology.Application.Features.Payments.Commands.ProcessWebhook;

public record ProcessPaymentWebhookCommand(
    Guid? PaymentId,
    string GatewayReference,
    string TransactionStatus,
    string? RawResponse) : IRequest<Result<bool>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.Properties,
        ApplicationCacheTags.SavedProperties
    ];
}
