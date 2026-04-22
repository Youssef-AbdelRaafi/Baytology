using Baytology.Domain.AISearch;
using Baytology.Domain.AgentDetails;
using Baytology.Domain.AuditLogs;
using Baytology.Domain.Conversations;
using Baytology.Domain.DomainEvents;
using Baytology.Domain.Bookings;
using Baytology.Domain.Identity;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;
using Baytology.Domain.Properties;
using Baytology.Domain.Recommendations;
using Baytology.Domain.UserProfiles;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Baytology.Application.Common.Interfaces;

public interface IAppDbContext
{
    // Identity
    DbSet<RefreshToken> RefreshTokens { get; }

    // Profiles
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<AgentDetail> AgentDetails { get; }

    // Properties
    DbSet<Property> Properties { get; }
    DbSet<PropertyImage> PropertyImages { get; }
    DbSet<PropertyAmenity> PropertyAmenities { get; }
    DbSet<SavedProperty> SavedProperties { get; }
    DbSet<PropertyView> PropertyViews { get; }
    DbSet<AgentReview> AgentReviews { get; }

    // Conversations
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }

    // Bookings
    DbSet<Booking> Bookings { get; }

    // Notifications
    DbSet<Notification> Notifications { get; }

    // Payments
    DbSet<Payment> Payments { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<RefundRequest> RefundRequests { get; }

    // AI Search
    DbSet<SearchRequest> SearchRequests { get; }
    DbSet<TextSearch> TextSearches { get; }
    DbSet<VoiceSearch> VoiceSearches { get; }
    DbSet<ImageSearch> ImageSearches { get; }
    DbSet<SearchFilter> SearchFilters { get; }
    DbSet<SearchResult> SearchResults { get; }

    // Recommendations
    DbSet<RecommendationRequest> RecommendationRequests { get; }
    DbSet<RecommendationResult> RecommendationResults { get; }

    // Domain Events & Audit
    DbSet<DomainEventLog> DomainEventLogs { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
