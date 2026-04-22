using Baytology.Application.Common.Interfaces;

namespace Baytology.Api.Tests.Infrastructure;

public class TestEmailSender : IEmailSender
{
    public List<string> SentConfirmations { get; } = new();
    public List<string> SentPasswordResets { get; } = new();

    public Task SendEmailConfirmationAsync(string email, string userId, string token)
    {
        SentConfirmations.Add(email);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string token)
    {
        SentPasswordResets.Add(email);
        return Task.CompletedTask;
    }
}
