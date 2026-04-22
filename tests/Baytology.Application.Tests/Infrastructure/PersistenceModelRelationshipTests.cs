using Baytology.Application.Tests.Support;
using Baytology.Domain.AISearch;
using Baytology.Domain.Bookings;
using Baytology.Domain.Conversations;
using Baytology.Domain.Identity;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;
using Baytology.Domain.Properties;
using Baytology.Domain.Recommendations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Baytology.Application.Tests.Infrastructure;

public sealed class PersistenceModelRelationshipTests
{
    [Fact]
    public void Core_business_entities_have_expected_foreign_keys()
    {
        using var context = TestDbContextFactory.Create();

        AssertForeignKey<Booking>(context, nameof(Booking.PropertyId));
        AssertForeignKey<Booking>(context, nameof(Booking.PaymentId));
        AssertForeignKey<Payment>(context, nameof(Payment.PropertyId));
        AssertForeignKey<Conversation>(context, nameof(Conversation.PropertyId));
        AssertForeignKey<Message>(context, nameof(Message.ConversationId));
        AssertForeignKey<Message>(context, nameof(Message.SenderId));
        AssertForeignKey<RefundRequest>(context, nameof(RefundRequest.PaymentId));
    }

    [Fact]
    public void Property_and_identity_related_entities_have_expected_foreign_keys()
    {
        using var context = TestDbContextFactory.Create();

        AssertForeignKey<SavedProperty>(context, nameof(SavedProperty.PropertyId));
        AssertForeignKey<SavedProperty>(context, nameof(SavedProperty.UserId));
        AssertForeignKey<PropertyView>(context, nameof(PropertyView.PropertyId));
        AssertForeignKey<PropertyView>(context, nameof(PropertyView.UserId));
        AssertForeignKey<AgentReview>(context, nameof(AgentReview.AgentUserId));
        AssertForeignKey<AgentReview>(context, nameof(AgentReview.ReviewerUserId));
        AssertForeignKey<AgentReview>(context, nameof(AgentReview.PropertyId));
        AssertForeignKey<Notification>(context, nameof(Notification.UserId));
        AssertForeignKey<RefreshToken>(context, nameof(RefreshToken.UserId));
    }

    [Fact]
    public void Ai_and_recommendation_entities_have_expected_foreign_keys()
    {
        using var context = TestDbContextFactory.Create();

        AssertForeignKey<SearchRequest>(context, nameof(SearchRequest.UserId));
        AssertForeignKey<SearchResult>(context, nameof(SearchResult.SearchRequestId));
        AssertForeignKey<RecommendationRequest>(context, nameof(RecommendationRequest.RequestedByUserId));
        AssertForeignKey<RecommendationResult>(context, nameof(RecommendationResult.RequestId));
        AssertForeignKey<RecommendationResult>(context, nameof(RecommendationResult.RecommendedPropertyId));
    }

    [Fact]
    public void Core_business_entities_have_expected_check_constraints_and_unique_business_indexes()
    {
        using var context = TestDbContextFactory.Create();

        AssertCheckConstraint<Booking>(context, "CK_Bookings_DateRange");
        AssertCheckConstraint<Conversation>(context, "CK_Conversations_DistinctParticipants");
        AssertCheckConstraint<Message>(context, "CK_Messages_ReadState");
        AssertCheckConstraint<Message>(context, "CK_Messages_ContentOrAttachment");
        AssertCheckConstraint<Notification>(context, "CK_Notifications_ReadState");
        AssertCheckConstraint<Payment>(context, "CK_Payments_Amounts_Valid");
        AssertCheckConstraint<PaymentTransaction>(context, "CK_PaymentTransactions_RequiredState");
        AssertCheckConstraint<Property>(context, "CK_Properties_BusinessRules");
        AssertCheckConstraint<PropertyImage>(context, "CK_PropertyImages_BusinessRules");
        AssertCheckConstraint<RefundRequest>(context, "CK_RefundRequests_Amount_Positive");
        AssertCheckConstraint<RefundRequest>(context, "CK_RefundRequests_Status_Valid");
        AssertCheckConstraint<RefundRequest>(context, "CK_RefundRequests_ReviewState");
        AssertCheckConstraint<AgentReview>(context, "CK_AgentReviews_Rating_Range");
        AssertCheckConstraint<AgentReview>(context, "CK_AgentReviews_DistinctUsers");
        AssertCheckConstraint<SearchRequest>(context, "CK_SearchRequests_State");
        AssertCheckConstraint<SearchFilter>(context, "CK_SearchFilters_Ranges");
        AssertCheckConstraint<SearchResult>(context, "CK_SearchResults_BusinessRules");
        AssertCheckConstraint<RecommendationRequest>(context, "CK_RecommendationRequests_BusinessRules");
        AssertCheckConstraint<RecommendationResult>(context, "CK_RecommendationResults_BusinessRules");

        AssertUniqueIndex<AgentReview>(context, nameof(AgentReview.AgentUserId), nameof(AgentReview.ReviewerUserId), nameof(AgentReview.PropertyId));
        AssertUniqueIndex<AgentReview>(context, nameof(AgentReview.AgentUserId), nameof(AgentReview.ReviewerUserId));
        AssertUniqueIndex<PropertyImage>(context, nameof(PropertyImage.PropertyId), nameof(PropertyImage.SortOrder));
        AssertUniqueIndex<SearchResult>(context, nameof(SearchResult.SearchRequestId), nameof(SearchResult.Rank));
        AssertUniqueIndex<RecommendationResult>(context, nameof(RecommendationResult.RequestId), nameof(RecommendationResult.Rank));
    }

    private static void AssertForeignKey<TEntity>(global::Baytology.Infrastructure.Data.AppDbContext context, string propertyName)
    {
        var entityType = GetDesignTimeEntityType<TEntity>(context);

        Assert.NotNull(entityType);
        Assert.Contains(
            entityType!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == propertyName));
    }

    private static void AssertCheckConstraint<TEntity>(global::Baytology.Infrastructure.Data.AppDbContext context, string constraintName)
    {
        var entityType = GetDesignTimeEntityType<TEntity>(context);

        Assert.NotNull(entityType);
        Assert.Contains(
            entityType!.GetCheckConstraints(),
            constraint => string.Equals(constraint.Name, constraintName, StringComparison.Ordinal));
    }

    private static void AssertUniqueIndex<TEntity>(global::Baytology.Infrastructure.Data.AppDbContext context, params string[] propertyNames)
    {
        var entityType = GetDesignTimeEntityType<TEntity>(context);

        Assert.NotNull(entityType);
        Assert.Contains(
            entityType!.GetIndexes(),
            index => index.IsUnique && propertyNames.SequenceEqual(index.Properties.Select(property => property.Name)));
    }

    private static IEntityType? GetDesignTimeEntityType<TEntity>(global::Baytology.Infrastructure.Data.AppDbContext context)
    {
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;
        return designTimeModel.FindEntityType(typeof(TEntity));
    }
}
