using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AgentDetails;

public sealed class AgentDetail : Entity
{
    public string UserId { get; private set; } = null!;
    public string? AgencyName { get; private set; }
    public string? LicenseNumber { get; private set; }
    public decimal Rating { get; private set; }
    public int ReviewCount { get; private set; }
    public bool IsVerified { get; private set; }
    public decimal CommissionRate { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public DateTimeOffset UpdatedOnUtc { get; private set; }

    private AgentDetail() { }

    private AgentDetail(
        Guid id,
        string userId,
        string? agencyName,
        string? licenseNumber,
        decimal commissionRate) : base(id)
    {
        UserId = userId;
        AgencyName = agencyName;
        LicenseNumber = licenseNumber;
        CommissionRate = commissionRate;
        Rating = 0;
        ReviewCount = 0;
        IsVerified = false;
        CreatedOnUtc = DateTimeOffset.UtcNow;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public static Result<AgentDetail> Create(
        string userId,
        string? agencyName = null,
        string? licenseNumber = null,
        decimal commissionRate = 0.025m)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return AgentDetailErrors.UserIdRequired;

        return new AgentDetail(Guid.NewGuid(), userId, agencyName, licenseNumber, commissionRate);
    }

    public void Update(string? agencyName, string? licenseNumber, decimal commissionRate)
    {
        AgencyName = agencyName;
        LicenseNumber = licenseNumber;
        CommissionRate = commissionRate;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void Verify() => IsVerified = true;

    public void UpdateRating(decimal newRating, int newCount)
    {
        Rating = newRating;
        ReviewCount = newCount;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }
}
