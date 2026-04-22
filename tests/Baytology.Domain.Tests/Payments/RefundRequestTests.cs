using Baytology.Domain.Common.Enums;
using Baytology.Domain.Payments;

namespace Baytology.Domain.Tests.Payments;

public sealed class RefundRequestTests
{
    [Fact]
    public void Create_requires_reason_and_positive_amount()
    {
        var result = RefundRequest.Create(Guid.NewGuid(), "buyer-1", " ", 0m);

        Assert.True(result.IsError);
        Assert.Equal("Refund_Reason_Required", result.TopError.Code);
    }

    [Fact]
    public void Approved_refund_can_be_marked_processed()
    {
        var refund = RefundRequest.Create(Guid.NewGuid(), "buyer-1", "Need refund", 500m).Value;

        Assert.True(refund.Approve("admin-1").IsSuccess);
        Assert.True(refund.MarkProcessed().IsSuccess);
        Assert.Equal(RefundStatus.Processed, refund.Status);
    }
}
