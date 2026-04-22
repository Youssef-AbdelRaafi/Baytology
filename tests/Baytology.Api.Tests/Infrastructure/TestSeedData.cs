namespace Baytology.Api.Tests.Infrastructure;

public sealed class TestSeedData
{
    public const string AiWorkerServiceToken = "test-ai-worker-token";
    public const string AiWorkerServiceTokenHeaderName = "X-AI-Service-Token";

    public const string AdminUserId = "admin-user";
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin@Test123";

    public const string AgentUserId = "agent-user";
    public const string AgentEmail = "agent@test.local";
    public const string AgentPassword = "Agent@Test123";

    public const string BuyerUserId = "buyer-user";
    public const string BuyerEmail = "buyer@test.local";
    public const string BuyerPassword = "Buyer@Test123";

    public const string FreshBuyerUserId = "fresh-buyer-user";
    public const string FreshBuyerEmail = "freshbuyer@test.local";
    public const string FreshBuyerPassword = "Buyer@Test123";

    public Guid CatalogPropertyId { get; set; }
    public Guid ConversationPropertyId { get; set; }
    public Guid SavablePropertyId { get; set; }
    public Guid BookingPropertyId { get; set; }
    public Guid PendingBookingPropertyId { get; set; }
    public Guid ReviewPropertyId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public Guid NotificationId { get; set; }
    public Guid PendingBookingId { get; set; }
    public Guid RefundablePaymentId { get; set; }
    public Guid AdminRefundRequestId { get; set; }
    public Guid WebhookPaymentId { get; set; }
    public Guid ReviewBookingId { get; set; }
    public Guid ReviewPaymentId { get; set; }
}
