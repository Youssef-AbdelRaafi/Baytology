using System.Text.Json;
using System.Text.Json.Serialization;

namespace Baytology.Contracts.Requests.Payments;

public sealed class PaymobWebhookRequest
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("obj")]
    public PaymobWebhookObject? Obj { get; set; }
}

public sealed class PaymobWebhookObject
{
    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("id")]
    public JsonElement? Id { get; set; }

    [JsonPropertyName("special_reference")]
    public JsonElement? SpecialReference { get; set; }

    [JsonPropertyName("order")]
    public PaymobWebhookOrder? Order { get; set; }
}

public sealed class PaymobWebhookOrder
{
    [JsonPropertyName("id")]
    public JsonElement? Id { get; set; }

    [JsonPropertyName("merchant_order_id")]
    public JsonElement? MerchantOrderId { get; set; }

    [JsonPropertyName("special_reference")]
    public JsonElement? SpecialReference { get; set; }
}
