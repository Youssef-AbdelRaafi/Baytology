using Baytology.Domain.Common.Enums;
using Baytology.Domain.Properties;
using Baytology.Domain.Properties.Events;

namespace Baytology.Domain.Tests.Properties;

public sealed class PropertyTests
{
    [Fact]
    public void Create_raises_property_created_event()
    {
        var result = Property.Create(
            "agent-1",
            "Luxury Apartment",
            "Nice property",
            PropertyType.Apartment,
            ListingType.Rent,
            10000m,
            150m,
            3,
            2,
            "Cairo",
            "Nasr City");

        Assert.True(result.IsSuccess);
        Assert.Equal(PropertyStatus.Available, result.Value.Status);
        Assert.Single(result.Value.DomainEvents);
        Assert.IsType<PropertyCreatedEvent>(result.Value.DomainEvents.Single());
    }

    [Theory]
    [InlineData(ListingType.Rent, PropertyStatus.Rented)]
    [InlineData(ListingType.Sale, PropertyStatus.Sold)]
    public void MarkUnavailableForConfirmedBooking_sets_status_based_on_listing_type(
        ListingType listingType,
        PropertyStatus expectedStatus)
    {
        var property = Property.Create(
            "agent-1",
            "Luxury Apartment",
            "Nice property",
            PropertyType.Apartment,
            listingType,
            10000m,
            150m,
            3,
            2).Value;

        property.MarkUnavailableForConfirmedBooking();

        Assert.Equal(expectedStatus, property.Status);
    }

    [Fact]
    public void Create_rejects_invalid_floor_range()
    {
        var result = Property.Create(
            "agent-1",
            "Luxury Apartment",
            "Nice property",
            PropertyType.Apartment,
            ListingType.Rent,
            10000m,
            150m,
            3,
            2,
            "Cairo",
            "Nasr City",
            floor: 8,
            totalFloors: 5);

        Assert.True(result.IsError);
        Assert.Equal("Property_Floor_Range_Invalid", result.TopError.Code);
    }
}
