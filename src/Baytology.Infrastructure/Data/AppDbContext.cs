using System.Reflection;

using Baytology.Application.Common.Interfaces;
using Baytology.Domain.AgentDetails;
using Baytology.Domain.AISearch;
using Baytology.Domain.AuditLogs;
using Baytology.Domain.Conversations;
using Baytology.Domain.Bookings;
using Baytology.Domain.DomainEvents;
using Baytology.Domain.Identity;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;
using Baytology.Domain.Properties;
using Baytology.Domain.Recommendations;
using Baytology.Domain.UserProfiles;
using Baytology.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Identity
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Profiles
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<AgentDetail> AgentDetails => Set<AgentDetail>();

    // Properties
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<PropertyAmenity> PropertyAmenities => Set<PropertyAmenity>();
    public DbSet<SavedProperty> SavedProperties => Set<SavedProperty>();
    public DbSet<PropertyView> PropertyViews => Set<PropertyView>();
    public DbSet<AgentReview> AgentReviews => Set<AgentReview>();

    // Conversations
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    // Bookings
    public DbSet<Booking> Bookings => Set<Booking>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();

    // Payments
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();

    // AI Search
    public DbSet<SearchRequest> SearchRequests => Set<SearchRequest>();
    public DbSet<TextSearch> TextSearches => Set<TextSearch>();
    public DbSet<VoiceSearch> VoiceSearches => Set<VoiceSearch>();
    public DbSet<ImageSearch> ImageSearches => Set<ImageSearch>();
    public DbSet<SearchFilter> SearchFilters => Set<SearchFilter>();
    public DbSet<SearchResult> SearchResults => Set<SearchResult>();

    // Recommendations
    public DbSet<RecommendationRequest> RecommendationRequests => Set<RecommendationRequest>();
    public DbSet<RecommendationResult> RecommendationResults => Set<RecommendationResult>();

    // Domain Events & Audit
    public DbSet<DomainEventLog> DomainEventLogs => Set<DomainEventLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
