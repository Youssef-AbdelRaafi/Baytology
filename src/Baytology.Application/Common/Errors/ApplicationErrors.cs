using Baytology.Domain.AgentDetails;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Payments;
using Baytology.Domain.Properties;

namespace Baytology.Application.Common.Errors;

public static class ApplicationErrors
{
    public static class Validation
    {
        public static Error Pipeline(string propertyName, string message) =>
            Error.Validation(propertyName, message);

        public static readonly Error InvalidTokenFormat =
            Error.Validation("Invalid_Token_Format", "The provided token is invalid.");
    }

    public static class Tokens
    {
        public static readonly Error ExpiredAccessTokenInvalid =
            Error.Unauthorized("Token_Expired_Access_Invalid", "Expired access token is not valid.");

        public static readonly Error UserIdClaimInvalid =
            Error.Unauthorized("Token_UserId_Claim_Invalid", "User ID claim is invalid.");

        public static readonly Error RefreshTokenExpired =
            Error.Unauthorized("Token_Refresh_Expired", "Refresh token has expired.");
    }

    public static class Admin
    {
        public static readonly Error RefundNotFound =
            Error.NotFound("Refund_Not_Found", "Refund request not found.");

        public static readonly Error RefundAlreadyReviewed =
            Error.Conflict("Refund.AlreadyReviewed", "Refund request has already been reviewed.");

        public static readonly Error PartialRefundNotSupported =
            Error.Conflict(
                "Refund.PartialRefundNotSupported",
                "This payment model supports full refunds only. Reject or recreate the request with the full payment amount.");

        public static readonly Error PaymentNotRefundable =
            Error.Conflict("Refund.PaymentNotRefundable", "Only completed payments can be marked as refunded.");

        public static readonly Error AgentNotFound =
            Error.NotFound("Agent_Not_Found", "Agent profile not found.");
    }

    public static class AgentDetails
    {
        public static readonly Error NotFound = AgentDetailErrors.NotFound;

        public static readonly Error InvalidCommissionRate =
            Error.Validation("AgentDetail.CommissionRate", "Commission rate must be greater than 0 and less than 1.");
    }

    public static class Search
    {
        public static readonly Error CompletionRequestNotFound =
            Error.NotFound("SearchRequest.NotFound", "Search request not found.");

        public static readonly Error RequestNotFound =
            Error.NotFound("Search_Request_Not_Found", "Search request not found.");

        public static readonly Error AccessDenied =
            Error.Forbidden("Search_Request_Access_Denied", "You don't have permission to view this search request.");
    }

    public static class Booking
    {
        public static readonly Error PropertyNotAvailable =
            Error.Conflict("Booking.PropertyNotAvailable", "Property is not available for booking.");

        public static readonly Error SelfBooking =
            Error.Conflict("Booking.SelfBooking", "You cannot book your own property.");

        public static readonly Error AgentUnavailable =
            Error.Conflict("Booking.AgentUnavailable", "This listing is not currently bookable.");

        public static readonly Error Overlapping =
            Error.Conflict("Booking.Overlapping", "Property is already booked for the selected dates.");

        public static readonly Error ConcurrentRequest =
            Error.Conflict(
                "Booking.ConcurrentRequest",
                "Another booking request is being processed for the selected dates. Please retry.");

        public static readonly Error NotFound =
            Error.NotFound("Booking_Not_Found", "Booking not found.");

        public static readonly Error StatusUpdateNotFound =
            Error.NotFound("Booking.NotFound", "Booking not found.");

        public static readonly Error AccessDenied =
            Error.Forbidden("Booking_Access_Denied", "You don't have permission to view this booking.");

        public static readonly Error NotAgent =
            Error.Forbidden("Booking.NotAgent", "Only the agent can update the booking status.");

        public static readonly Error NoPayment =
            Error.Conflict("Booking.NoPayment", "Cannot confirm booking without payment.");

        public static readonly Error PaymentNotCompleted =
            Error.Conflict("Booking.PaymentNotCompleted", "Payment is not completed yet.");

        public static readonly Error AlreadyConfirmed =
            Error.Conflict("Booking.AlreadyConfirmed", "Confirmed bookings cannot be cancelled from this endpoint.");
    }

    public static class Conversation
    {
        public static readonly Error SelfContact =
            Error.Conflict("Conversation_SelfContact", "You cannot start a conversation about your own property.");

        public static readonly Error AgentUnavailable =
            Error.Conflict("Conversation_AgentUnavailable", "This listing is not currently available for direct contact.");

        public static readonly Error MessageNotFound =
            Error.NotFound("Message_Not_Found", "Message not found.");

        public static readonly Error MessageReadNotAllowed =
            Error.Conflict("Message.ReadNotAllowed", "Only the message recipient can mark the message as read.");
    }

    public static class Notification
    {
        public static readonly Error NotFound =
            Error.NotFound("Notification_Not_Found", "Notification not found.");
    }

    public static class Refund
    {
        public static readonly Error PaymentNotCompleted =
            Error.Conflict("Refund.PaymentNotCompleted", "Refunds can only be requested for completed payments.");

        public static readonly Error AmountInvalid =
            Error.Validation("Refund.AmountInvalid", "Refund amount cannot exceed the original payment amount.");

        public static readonly Error AmountMustMatchPayment =
            Error.Validation(
                "Refund.AmountMustMatchPayment",
                "The current payment model supports full refunds only. Request the full original payment amount.");

        public static readonly Error PendingExists =
            Error.Conflict("Refund.PendingExists", "There is already a pending refund request for this payment.");
    }

    public static class Property
    {
        public static readonly Error NotFound = PropertyErrors.NotFound;

        public static readonly Error AccessDenied =
            Error.Forbidden("Property_Access_Denied", "You don't own this property.");

        public static readonly Error DeleteNotAllowed =
            Error.Conflict(
                "Property_Delete_Not_Allowed",
                "Properties with existing bookings, payments, conversations, or reviews cannot be deleted.");
    }

    public static class Review
    {
        public static readonly Error AgentMismatch =
            Error.Conflict("Review.AgentMismatch", "The selected property does not belong to the reviewed agent.");

        public static readonly Error BookingRequired =
            Error.Forbidden("Review.BookingRequired", "You can only review agents after a confirmed booking.");

        public static readonly Error AlreadySubmitted =
            Error.Conflict("Review.AlreadySubmitted", "You have already reviewed this agent for the selected property.");
    }

    public static class Recommendation
    {
        public static readonly Error CompletionRequestNotFound =
            Error.NotFound("RecommendationRequest.NotFound", "Recommendation request not found.");

        public static readonly Error RequestNotFound =
            Error.NotFound("Recommendation_Request_Not_Found", "Recommendation request not found.");

        public static readonly Error AccessDenied =
            Error.Forbidden("Recommendation_Request_Access_Denied", "You do not have permission to view this request.");
    }

    public static class ExternalAi
    {
        public static Error Unavailable(string serviceName) =>
            Error.Failure($"{serviceName}.Unavailable", $"{serviceName} service is unavailable.");

        public static Error Disabled(string serviceName) =>
            Error.Failure($"{serviceName}.Disabled", $"{serviceName} integration is disabled.");

        public static Error NotConfigured(string serviceName) =>
            Error.Failure($"{serviceName}.NotConfigured", $"{serviceName} base URL is not configured.");

        public static Error Failed(string serviceName, int statusCode, string body) =>
            Error.Failure($"{serviceName}.Failed", $"{serviceName} returned {statusCode}: {Truncate(body)}");
    }

    public static class ExternalLogin
    {
        public static readonly Error ProviderInvalid =
            Error.Validation("ExternalLogin_Provider_Invalid", "Unsupported provider.");

        public static readonly Error GoogleTokenInvalid =
            Error.Unauthorized("ExternalLogin_Token_Invalid", "Invalid Google token.");

        public static readonly Error GoogleTokenParseFailed =
            Error.Unauthorized("ExternalLogin_Token_Invalid", "Failed to parse Google token.");

        public static readonly Error FacebookTokenInvalid =
            Error.Unauthorized("ExternalLogin_Token_Invalid", "Invalid Facebook token.");

        public static readonly Error FacebookTokenNotValid =
            Error.Unauthorized("ExternalLogin_Token_Invalid", "Facebook token is not valid.");

        public static readonly Error AudienceMismatch =
            Error.Unauthorized("ExternalLogin_Audience_Mismatch", "Token was not issued for this application.");

        public static readonly Error GoogleIssuerMismatch =
            Error.Unauthorized("ExternalLogin_Issuer_Mismatch", "Token was not issued by Google.");

        public static readonly Error GoogleTokenExpired =
            Error.Unauthorized("ExternalLogin_Token_Expired", "Google token has expired.");

        public static readonly Error FacebookTokenExpired =
            Error.Unauthorized("ExternalLogin_Token_Expired", "Facebook token has expired.");

        public static readonly Error EmailMissing =
            Error.Validation("ExternalLogin_Email_Missing", "Facebook account must have an email address associated.");

        public static readonly Error UserInfoFailed =
            Error.Unexpected("ExternalLogin_UserInfo_Failed", "Failed to retrieve Facebook user info.");

        public static Error TokenValidationFailed(string providerName) =>
            Error.Unexpected("ExternalLogin_Token_Validation_Failed", $"Failed to validate {providerName} token.");
    }

    public static class Identity
    {
        public static readonly Error UserUnauthorized =
            Error.Unauthorized("User_Unauthorized", "The current user is not authenticated.");

        public static readonly Error UserNotFound =
            Error.NotFound("User_Not_Found", "User not found.");

        public static Error UserNotFoundByEmail(string maskedEmail) =>
            Error.NotFound("User_Not_Found", $"User with email {maskedEmail} not found");

        public static Error EmailNotConfirmed(string maskedEmail) =>
            Error.Conflict("Email_Not_Confirmed", $"email '{maskedEmail}' not confirmed");

        public static readonly Error InvalidLoginAttempt =
            Error.Conflict("Invalid_Login_Attempt", "Email / Password are incorrect");

        public static Error ValidationFailure(string code, string description) =>
            Error.Validation(code, description);

        public static readonly Error RoleInvalid =
            Error.Validation("Role_Invalid", "Role must be Buyer, Agent, or Admin.");

        public static readonly Error RoleAssignmentAgentResponsibilities =
            Error.Conflict(
                "Role_Assignment_AgentResponsibilities",
                "This user still owns active agent records. Reassign or close them before changing the role.");

        public static readonly Error EmailAlreadyConfirmed =
            Error.Conflict("Email_Already_Confirmed", "Email is already confirmed.");

        public static readonly Error UserInactiveDeleted =
            Error.Forbidden("User_Inactive", "This account is no longer available.");

        public static readonly Error UserInactiveLocked =
            Error.Forbidden("User_Inactive", "This account has been deactivated by an administrator.");
    }

    public static class Payment
    {
        public static readonly Error NotFound = PaymentErrors.NotFound;
    }

    public static class Paymob
    {
        public static readonly Error NotConfigured =
            Error.Failure(
                "Paymob_Not_Configured",
                "Paymob settings are incomplete. Configure SecretKey, PublicKey, IntegrationId, and WebhookToken before starting payment flows.");

        public static Error ApiError(string statusCode) =>
            Error.Failure("Paymob_Api_Error", $"Paymob returned {statusCode}");

        public static Error GatewayError(string message) =>
            Error.Failure("Paymob_Error", message);
    }

    private static string Truncate(string value)
    {
        return value.Length <= 300 ? value : value[..300];
    }
}
