using Baytology.Domain.Identity;

namespace Baytology.Domain.Tests.Identity;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Create_returns_validation_error_when_expiry_is_not_in_the_future()
    {
        var result = RefreshToken.Create(Guid.NewGuid(), "refresh-token", "user-1", DateTimeOffset.UtcNow);

        Assert.True(result.IsError);
        Assert.Equal(RefreshTokenErrors.ExpiryInvalid, result.TopError);
    }

    [Fact]
    public void Create_succeeds_for_valid_refresh_token()
    {
        var result = RefreshToken.Create(Guid.NewGuid(), "refresh-token", "user-1", DateTimeOffset.UtcNow.AddDays(7));

        Assert.True(result.IsSuccess);
        Assert.Equal("user-1", result.Value.UserId);
    }
}
