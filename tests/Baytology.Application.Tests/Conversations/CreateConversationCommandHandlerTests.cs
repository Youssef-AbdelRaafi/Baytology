using Baytology.Application.Features.Conversations.Commands.CreateConversation;
using Baytology.Application.Tests.Support;
using Baytology.Domain.AgentDetails;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Conversations;
using Baytology.Domain.Properties;

namespace Baytology.Application.Tests.Conversations;

public sealed class CreateConversationCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_conversation_between_buyer_and_agent()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateConversationCommandHandler(context);

        var property = CreateProperty();
        context.Properties.Add(property);
        context.AgentDetails.Add(AgentDetail.Create(property.AgentUserId).Value);
        await context.SaveChangesAsync();

        var command = new CreateConversationCommand(property.Id, "buyer-1");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var conversation = Assert.Single(context.Conversations);
        Assert.Equal("buyer-1", conversation.BuyerUserId);
        Assert.Equal(property.AgentUserId, conversation.AgentUserId);
        Assert.Equal(property.Id, conversation.PropertyId);
    }

    [Fact]
    public async Task Handle_returns_existing_conversation_id_when_duplicate()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateConversationCommandHandler(context);

        var property = CreateProperty();
        var existingConversation = Conversation.Create(property.Id, "buyer-1", property.AgentUserId).Value;

        context.Properties.Add(property);
        context.AgentDetails.Add(AgentDetail.Create(property.AgentUserId).Value);
        context.Conversations.Add(existingConversation);
        await context.SaveChangesAsync();

        var command = new CreateConversationCommand(property.Id, "buyer-1");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingConversation.Id, result.Value);
        Assert.Single(context.Conversations);
    }

    [Fact]
    public async Task Handle_returns_not_found_for_nonexistent_property()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateConversationCommandHandler(context);

        var command = new CreateConversationCommand(Guid.NewGuid(), "buyer-1");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Property_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_rejects_self_conversation()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateConversationCommandHandler(context);

        var property = CreateProperty();
        context.Properties.Add(property);
        context.AgentDetails.Add(AgentDetail.Create(property.AgentUserId).Value);
        await context.SaveChangesAsync();

        var command = new CreateConversationCommand(property.Id, property.AgentUserId);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Conversation_SelfContact", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_rejects_when_agent_profile_missing()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateConversationCommandHandler(context);

        var property = CreateProperty();
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        var command = new CreateConversationCommand(property.Id, "buyer-1");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Conversation_AgentUnavailable", result.TopError.Code);
    }

    private static Property CreateProperty()
    {
        return Property.Create(
            "agent-1",
            "Test Property",
            "Description",
            PropertyType.Apartment,
            ListingType.Sale,
            800_000m,
            140m,
            3,
            2,
            "Cairo",
            "Heliopolis").Value;
    }
}
