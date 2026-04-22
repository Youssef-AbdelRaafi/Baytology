using FluentValidation;

namespace Baytology.Application.Features.AgentDetails.Commands.UpdateAgentDetail;

public class UpdateAgentDetailCommandValidator : AbstractValidator<UpdateAgentDetailCommand>
{
    public UpdateAgentDetailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AgencyName).MaximumLength(300);
        RuleFor(x => x.LicenseNumber).MaximumLength(100);
        RuleFor(x => x.CommissionRate).GreaterThan(0).LessThan(1);
    }
}
