namespace Baytology.Contracts.Requests.AgentDetails;

public sealed record UpdateAgentDetailRequest(
    string? AgencyName,
    string? LicenseNumber,
    decimal CommissionRate);
