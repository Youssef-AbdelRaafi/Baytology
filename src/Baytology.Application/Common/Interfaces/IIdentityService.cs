using Baytology.Application.Features.Identity.Dtos;
using Baytology.Domain.Common.Results;

namespace Baytology.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<List<UserSummaryDto>>> GetUsersAsync();
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string? policyName);
    Task<Result<AppUserDto>> AuthenticateAsync(string email, string password);
    Task<Result<string>> RegisterUserAsync(string email, string password, string role);
    Task<Result<Success>> ToggleUserStatusAsync(string userId, bool isActive);
    Task<Result<Success>> AssignRoleAsync(string userId, string role);
    Task<Result<AppUserDto>> GetUserByIdAsync(string userId);
    Task<Result<ExternalLoginResultDto>> ExternalLoginAsync(string provider, string providerSubjectId, string email, string? firstName, string? lastName);
    Task<Result<Success>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<Result<Success>> ForgotPasswordAsync(string email);
    Task<Result<Success>> ResetPasswordAsync(string email, string token, string newPassword);
    Task<Result<Success>> ResendEmailConfirmationAsync(string email);
    Task<Result<Success>> ConfirmEmailAsync(string userId, string token);
    Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId);
    Task<Result<Success>> DeleteAccountAsync(string userId);
    Task<Result<Success>> RevokeRefreshTokensAsync(string userId);
}
