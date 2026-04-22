using Baytology.Application.Common.Interfaces;
using Baytology.Domain.AgentDetails;
using Baytology.Domain.Common.Results;
using Baytology.Domain.UserProfiles;

using MediatR;

namespace Baytology.Application.Features.Identity.Commands.RegisterUser;

public class RegisterUserCommandHandler(IIdentityService identityService, IAppDbContext context)
    : IRequestHandler<RegisterUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var role = request.Role is "Agent" or "Buyer" ? request.Role : "Buyer";

        var registerResult = await identityService.RegisterUserAsync(request.Email, request.Password, role);

        if (registerResult.IsError)
        {
            return registerResult.Errors;
        }

        var userId = registerResult.Value;

        var userProfileResult = UserProfile.Create(userId, request.DisplayName);
        if (userProfileResult.IsSuccess)
        {
            context.UserProfiles.Add(userProfileResult.Value);
        }

        if (role == "Agent")
        {
            var agentDetailResult = AgentDetail.Create(userId);
            if (agentDetailResult.IsSuccess)
            {
                context.AgentDetails.Add(agentDetailResult.Value);
            }
        }

        await context.SaveChangesAsync(ct);

        return userId;
    }
}
