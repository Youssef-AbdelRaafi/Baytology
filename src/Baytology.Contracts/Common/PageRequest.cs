namespace Baytology.Contracts.Common;

public sealed record PageRequest(
    int PageNumber = 1,
    int PageSize = 10);
