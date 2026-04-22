namespace Baytology.Domain.Identity;

public static class Role
{
    public const string Admin = nameof(Admin);
    public const string Agent = nameof(Agent);
    public const string Buyer = nameof(Buyer);
    public const string Renter = nameof(Renter);

    public static readonly IReadOnlyList<string> All = [Admin, Agent, Buyer, Renter];
}
