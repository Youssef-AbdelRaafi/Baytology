using FluentValidation;

namespace Baytology.Application.Features.Admin.Commands.ReviewRefund;

public class ReviewRefundCommandValidator : AbstractValidator<ReviewRefundCommand>
{
    public ReviewRefundCommandValidator()
    {
        RuleFor(x => x.RefundId).NotEmpty();
        RuleFor(x => x.AdminUserId).NotEmpty();
    }
}
