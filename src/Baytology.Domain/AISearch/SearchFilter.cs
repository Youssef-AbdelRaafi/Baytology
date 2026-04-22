using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AISearch;

public sealed class SearchFilter : Entity
{
    public Guid SearchRequestId { get; private set; }
    public string? City { get; private set; }
    public string? District { get; private set; }
    public string? PropertyType { get; private set; }
    public string? ListingType { get; private set; }
    public decimal? MinPrice { get; private set; }
    public decimal? MaxPrice { get; private set; }
    public decimal? MinArea { get; private set; }
    public decimal? MaxArea { get; private set; }
    public int? MinBedrooms { get; private set; }
    public int? MaxBedrooms { get; private set; }

    private SearchFilter() { }

    private SearchFilter(Guid searchRequestId) : base(Guid.NewGuid())
    {
        SearchRequestId = searchRequestId;
    }

    public static Result<SearchFilter> Create(
        Guid searchRequestId,
        string? city,
        string? district,
        string? propertyType,
        string? listingType,
        decimal? minPrice,
        decimal? maxPrice,
        decimal? minArea,
        decimal? maxArea,
        int? minBedrooms,
        int? maxBedrooms)
    {
        if (searchRequestId == Guid.Empty)
            return SearchErrors.SearchRequestIdRequired;

        var filter = new SearchFilter(searchRequestId);
        var setResult = filter.SetFilters(
            city,
            district,
            propertyType,
            listingType,
            minPrice,
            maxPrice,
            minArea,
            maxArea,
            minBedrooms,
            maxBedrooms);

        return setResult.IsError ? setResult.Errors : filter;
    }

    public Result<Success> SetFilters(
        string? city, string? district, string? propertyType, string? listingType,
        decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea,
        int? minBedrooms, int? maxBedrooms)
    {
        if (minPrice.HasValue && minPrice.Value < 0)
            return SearchErrors.MinPriceInvalid;

        if (maxPrice.HasValue && maxPrice.Value < 0)
            return SearchErrors.MaxPriceInvalid;

        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            return SearchErrors.PriceRangeInvalid;

        if (minArea.HasValue && minArea.Value < 0)
            return SearchErrors.MinAreaInvalid;

        if (maxArea.HasValue && maxArea.Value < 0)
            return SearchErrors.MaxAreaInvalid;

        if (minArea.HasValue && maxArea.HasValue && minArea.Value > maxArea.Value)
            return SearchErrors.AreaRangeInvalid;

        if (minBedrooms.HasValue && minBedrooms.Value < 0)
            return SearchErrors.MinBedroomsInvalid;

        if (maxBedrooms.HasValue && maxBedrooms.Value < 0)
            return SearchErrors.MaxBedroomsInvalid;

        if (minBedrooms.HasValue && maxBedrooms.HasValue && minBedrooms.Value > maxBedrooms.Value)
            return SearchErrors.BedroomRangeInvalid;

        City = city;
        District = district;
        PropertyType = propertyType;
        ListingType = listingType;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        MinArea = minArea;
        MaxArea = maxArea;
        MinBedrooms = minBedrooms;
        MaxBedrooms = maxBedrooms;

        return Result.Success;
    }
}
