using FluentValidation;

namespace Baytology.Application.Features.Payments.Commands.ProcessWebhook;

public class ProcessPaymentWebhookCommandValidator : AbstractValidator<ProcessPaymentWebhookCommand>
{
    public ProcessPaymentWebhookCommandValidator()
    {
        RuleFor(x => x.TransactionStatus).NotEmpty().MaximumLength(50);
        RuleFor(x => x)
            .Must(x => x.PaymentId.HasValue || !string.IsNullOrWhiteSpace(x.GatewayReference))
            .WithMessage("Webhook must include either a payment id or a gateway reference.");
    }
}
