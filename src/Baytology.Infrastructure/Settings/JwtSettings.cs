namespace Baytology.Infrastructure.Settings;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Baytology.Api";
    public string Audience { get; set; } = "Baytology.Clients";
    public int AccesTokenExpirationInMinutes { get; set; } = 30;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
