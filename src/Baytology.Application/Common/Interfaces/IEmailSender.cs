namespace Baytology.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string email, string userId, string token);
    Task SendPasswordResetAsync(string email, string token);
}
