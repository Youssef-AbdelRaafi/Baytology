using Baytology.Application.Features.Properties.Commands.CreateProperty;
using Baytology.Application.Tests.Support;
using Baytology.Domain.Common.Enums;

namespace Baytology.Application.Tests.Properties;

public sealed class CreatePropertyCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_property_with_amenity_and_images()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreatePropertyCommandHandler(context);

        var command = new CreatePropertyCommand(
            "agent-1",
            "Modern Apartment",
            "Great location",
            PropertyType.Apartment,
            ListingType.Sale,
            1_500_000m,
            180m,
            3,
            2,
            Floor: 5,
            TotalFloors: 10,
            AddressLine: "123 Main St",
            City: "Cairo",
            District: "New Cairo",
            ZipCode: "11835",
            Latitude: 30.0m,
            Longitude: 31.2m,
            HasParking: true,
            HasPool: false,
            HasGym: true,
            HasElevator: true,
            HasSecurity: true,
            HasBalcony: true,
            HasGarden: false,
            HasCentralAC: true,
            FurnishingStatus: FurnishingStatus.SemiFurnished,
            ViewType: ViewType.City,
            ImageUrls: ["https://images.test/1.jpg", "https://images.test/2.jpg"]);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var property = Assert.Single(context.Properties);
        Assert.Equal("Modern Apartment", property.Title);
        Assert.Equal(PropertyStatus.Available, property.Status);
        Assert.Equal("Cairo", property.City);
        Assert.Equal(5, property.Floor);

        var amenity = Assert.Single(context.PropertyAmenities);
        Assert.True(amenity.HasParking);
        Assert.True(amenity.HasElevator);
        Assert.False(amenity.HasPool);
        Assert.Equal(FurnishingStatus.SemiFurnished, amenity.FurnishingStatus);

        Assert.Equal(2, context.PropertyImages.Count());
    }

    [Fact]
    public async Task Handle_creates_property_without_optional_fields()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreatePropertyCommandHandler(context);

        var command = new CreatePropertyCommand(
            "agent-1",
            "Simple Apartment",
            null,
            PropertyType.Apartment,
            ListingType.Rent,
            5000m,
            45m,
            0,
            1,
            null, null, null, null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var property = Assert.Single(context.Properties);
        Assert.Equal("Simple Apartment", property.Title);
        Assert.Null(property.Floor);
        Assert.Empty(context.PropertyImages);
    }

    [Fact]
    public async Task Handle_returns_error_for_invalid_price()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreatePropertyCommandHandler(context);

        var command = new CreatePropertyCommand(
            "agent-1",
            "Bad Property",
            null,
            PropertyType.Apartment,
            ListingType.Sale,
            -100m,
            100m,
            2,
            1,
            null, null, null, null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Empty(context.Properties);
    }
}
