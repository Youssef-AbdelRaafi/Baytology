using System.Data;

using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Identity.Dtos;
using Baytology.Domain.AgentDetails;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Identity;
using Baytology.Infrastructure.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Baytology.Infrastructure.Identity;

public class IdentityService(
    UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    IEmailSender emailSender,
    AppDbContext dbContext) : IIdentityService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUserClaimsPrincipalFactory<AppUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<Result<List<UserSummaryDto>>> GetUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var items = new List<UserSummaryDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isActive = user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow;

            items.Add(new UserSummaryDto(
                user.Id,
                user.Email ?? string.Empty,
                roles,
                isActive,
                user.EmailConfirmed));
        }

        return items;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string? policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName!);

        return result.Succeeded;
    }

    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return ApplicationErrors.Identity.UserNotFoundByEmail(UtilityService.MaskEmail(email));
        }

        var signInRestriction = GetSignInRestriction(user);
        if (signInRestriction is not null)
        {
            return signInRestriction;
        }

        if (!user.EmailConfirmed)
        {
            return ApplicationErrors.Identity.EmailNotConfirmed(UtilityService.MaskEmail(email));
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return ApplicationErrors.Identity.InvalidLoginAttempt;
        }

        return new AppUserDto(
            user.Id,
            user.Email!,
            await _userManager.GetRolesAsync(user),
            await _userManager.GetClaimsAsync(user),
            await GetDisplayNameAsync(user.Id));
    }

    public async Task<Result<string>> RegisterUserAsync(string email, string password, string role)
    {
        var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
        var normalizedRole = role is "Agent" or "Buyer" ? role : "Buyer";
        
        return await ExecuteIdentityTransactionAsync<string>(async () =>
        {
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApplicationErrors.Identity.ValidationFailure("Registration_Failed", errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
            if (!roleResult.Succeeded)
            {
                return CreateIdentityFailure("Registration_Failed", roleResult);
            }

            return user.Id;
        });
    }


    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return ApplicationErrors.Identity.UserUnauthorized;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApplicationErrors.Identity.UserNotFound;

        var roles = await _userManager.GetRolesAsync(user);

        var claims = await _userManager.GetClaimsAsync(user);

        return new AppUserDto(
            user.Id,
            user.Email!,
            roles,
            claims,
            await GetDisplayNameAsync(user.Id));
    }

    public async Task<Result<Success>> ToggleUserStatusAsync(string userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApplicationErrors.Identity.UserNotFound;

        var result = isActive
            ? await _userManager.SetLockoutEndDateAsync(user, null)
            : await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        if (!result.Succeeded)
            return CreateIdentityFailure("User_Status_Update_Failed", result);

        return Result.Success;
    }

    public async Task<Result<Success>> AssignRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApplicationErrors.Identity.UserNotFound;

        if (role is not Role.Buyer and not Role.Agent and not Role.Admin)
            return ApplicationErrors.Identity.RoleInvalid;

        return await ExecuteIdentityTransactionAsync<Success>(async () =>
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var isDemotingAgent = currentRoles.Any(currentRole => string.Equals(currentRole, Role.Agent, StringComparison.OrdinalIgnoreCase)) &&
                                  !string.Equals(role, Role.Agent, StringComparison.OrdinalIgnoreCase);

            if (isDemotingAgent && await HasActiveAgentResponsibilitiesAsync(userId))
            {
                return ApplicationErrors.Identity.RoleAssignmentAgentResponsibilities;
            }

            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return CreateIdentityFailure("Role_Assignment_Failed", removeResult);
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                return CreateIdentityFailure("Role_Assignment_Failed", addResult);
            }

            if (role == Role.Agent)
            {
                var ensureAgentDetailResult = await EnsureAgentDetailExistsAsync(userId);
                if (ensureAgentDetailResult.IsError)
                    return ensureAgentDetailResult.Errors;
            }
            else if (isDemotingAgent)
            {
                var agentDetail = await _dbContext.AgentDetails
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (agentDetail is not null)
                {
                    _dbContext.AgentDetails.Remove(agentDetail);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return Result.Success;
        });
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    private async Task<Result<T>> ExecuteIdentityTransactionAsync<T>(Func<Task<Result<T>>> operation)
    {
        if (!_dbContext.Database.IsRelational())
        {
            return await operation();
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var result = await operation();

            if (result.IsError)
            {
                await transaction.RollbackAsync();
                return result;
            }

            await transaction.CommitAsync();
            return result;
        });
    }

    private static Error CreateIdentityFailure(string code, IdentityResult result)
    {
        var description = string.Join(", ", result.Errors.Select(error => error.Description));
        return ApplicationErrors.Identity.ValidationFailure(code, description);
    }

    private async Task<Result<Success>> EnsureAgentDetailExistsAsync(string userId)
    {
        var hasAgentDetail = await _dbContext.AgentDetails
            .AnyAsync(a => a.UserId == userId);

        if (hasAgentDetail)
            return Result.Success;

        var agentDetailResult = AgentDetail.Create(userId);
        if (agentDetailResult.IsError)
        {
            return agentDetailResult.Errors;
        }

        _dbContext.AgentDetails.Add(agentDetailResult.Value);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            var duplicateExists = await _dbContext.AgentDetails.AnyAsync(a => a.UserId == userId);
            if (!duplicateExists)
            {
                throw;
            }
        }

        return Result.Success;
    }

    private async Task<bool> HasActiveAgentResponsibilitiesAsync(string userId)
    {
        if (await _dbContext.Properties.AnyAsync(property => property.AgentUserId == userId))
            return true;

        if (await _dbContext.Bookings.AnyAsync(
                booking => booking.AgentUserId == userId && booking.Status != BookingStatus.Cancelled))
            return true;

        return await _dbContext.Conversations.AnyAsync(conversation => conversation.AgentUserId == userId);
    }

    public async Task<Result<ExternalLoginResultDto>> ExternalLoginAsync(string provider, string providerSubjectId, string email, string? firstName, string? lastName)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

        if (user is not null)
        {
            var signInRestriction = GetSignInRestriction(user);
            if (signInRestriction is not null)
            {
                return signInRestriction;
            }
        }

        var isNewUser = false;
        if (user == null)
        {
            user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return CreateIdentityFailure("ExternalLogin_CreateUser_Failed", result);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Role.Buyer);
            if (!roleResult.Succeeded)
            {
                return CreateIdentityFailure("ExternalLogin_AddToRole_Failed", roleResult);
            }

            isNewUser = true;
        }

        var loginInfo = await _userManager.FindByLoginAsync(provider, providerSubjectId);
        if (loginInfo == null)
        {
            var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerSubjectId, provider));
            if (!addLoginResult.Succeeded)
            {
                return CreateIdentityFailure("ExternalLogin_AddLogin_Failed", addLoginResult);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var dto = new AppUserDto(
            user.Id,
            user.Email!,
            roles,
            claims,
            await GetDisplayNameAsync(user.Id));

        return new ExternalLoginResultDto(dto, isNewUser);
    }

    private async Task<string?> GetDisplayNameAsync(string userId)
    {
        return await _dbContext.UserProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => profile.DisplayName)
            .FirstOrDefaultAsync();
    }

    public async Task<Result<Success>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApplicationErrors.Identity.UserNotFound;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded) return CreateIdentityFailure("ChangePassword_Failed", result);

        return Result.Success;
    }

    public async Task<Result<Success>> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            // Don't reveal that the user does not exist or is not confirmed
            return Result.Success;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        await _emailSender.SendPasswordResetAsync(email, encodedToken);
        return Result.Success;
    }

    public async Task<Result<Success>> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return ApplicationErrors.Identity.UserNotFound;

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return ApplicationErrors.Validation.InvalidTokenFormat;
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        if (!result.Succeeded) return CreateIdentityFailure("ResetPassword_Failed", result);

        return Result.Success;
    }

    public async Task<Result<Success>> ResendEmailConfirmationAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result.Success;
        }

        if (user.EmailConfirmed)
        {
            return ApplicationErrors.Identity.EmailAlreadyConfirmed;
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        await _emailSender.SendEmailConfirmationAsync(email, user.Id, encodedToken);

        return Result.Success;
    }

    public async Task<Result<Success>> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApplicationErrors.Identity.UserNotFound;

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
             return ApplicationErrors.Validation.InvalidTokenFormat;
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded) return CreateIdentityFailure("ConfirmEmail_Failed", result);

        return Result.Success;
    }

    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApplicationErrors.Identity.UserNotFound;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public async Task<Result<Success>> DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApplicationErrors.Identity.UserNotFound;

        // Revoke tokens
        await RevokeRefreshTokensAsync(userId);

        // Soft delete
        user.IsDeleted = true;
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        await _userManager.UpdateAsync(user);

        return Result.Success;
    }

    public async Task<Result<Success>> RevokeRefreshTokensAsync(string userId)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(r => r.UserId == userId)
            .ToListAsync();

        if (tokens.Count > 0)
        {
            _dbContext.RefreshTokens.RemoveRange(tokens);
            await _dbContext.SaveChangesAsync();
        }

        return Result.Success;
    }

    private static Error? GetSignInRestriction(AppUser user)
    {
        if (user.IsDeleted)
        {
            return ApplicationErrors.Identity.UserInactiveDeleted;
        }

        if (user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            return ApplicationErrors.Identity.UserInactiveLocked;
        }

        return null;
    }
}
