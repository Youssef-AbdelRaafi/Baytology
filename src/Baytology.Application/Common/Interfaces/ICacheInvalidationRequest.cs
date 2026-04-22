namespace Baytology.Application.Common.Interfaces;

public interface ICacheInvalidationRequest
{
    IEnumerable<string> CacheTagsToInvalidate { get; }
}
