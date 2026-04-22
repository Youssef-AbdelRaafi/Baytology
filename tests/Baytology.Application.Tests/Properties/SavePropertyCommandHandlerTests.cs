using Baytology.Application.Features.Properties.Commands.SaveProperty;
using Baytology.Application.Tests.Support;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Properties;

namespace Baytology.Application.Tests.Properties;

public sealed class SavePropertyCommandHandlerTests
{
    [Fact]
    public async Task Handle_saves_property_for_authenticated_user()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new SavePropertyCommandHandler(context);

        var property = CreateProperty();
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        var command = new SavePropertyCommand("buyer-1", property.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedProperty = Assert.Single(context.SavedProperties);
        Assert.Equal("buyer-1", savedProperty.UserId);
        Assert.Equal(property.Id, savedProperty.PropertyId);
    }

    [Fact]
    public async Task Handle_returns_not_found_for_nonexistent_property()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new SavePropertyCommandHandler(context);

        var command = new SavePropertyCommand("buyer-1", Guid.NewGuid());
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Property_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_returns_conflict_when_property_already_saved()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new SavePropertyCommandHandler(context);

        var property = CreateProperty();
        context.Properties.Add(property);
        context.SavedProperties.Add(SavedProperty.Create("buyer-1", property.Id).Value);
        await context.SaveChangesAsync();

        var command = new SavePropertyCommand("buyer-1", property.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Property_Already_Saved", result.TopError.Code);
    }

    private static Property CreateProperty()
    {
        return Property.Create(
            "agent-1",
            "Test Property",
            "A test property",
            PropertyType.Apartment,
            ListingType.Sale,
            500000m,
            120m,
            2,
            1,
            "Cairo",
            "Maadi").Value;
    }
}
