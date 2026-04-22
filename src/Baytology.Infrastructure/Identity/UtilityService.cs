namespace Baytology.Infrastructure.Identity;

public static class UtilityService
{
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return email;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return email;

        var localPart = parts[0];
        var maskedLocal = localPart.Length <= 2
            ? localPart
            : $"{localPart[0]}{new string('*', localPart.Length - 2)}{localPart[^1]}";

        return $"{maskedLocal}@{parts[1]}";
    }
}
