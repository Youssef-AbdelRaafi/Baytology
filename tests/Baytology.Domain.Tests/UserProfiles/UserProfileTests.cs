using Baytology.Domain.Common.Enums;
using Baytology.Domain.UserProfiles;

namespace Baytology.Domain.Tests.UserProfiles;

public sealed class UserProfileTests
{
    [Fact]
    public void Create_returns_validation_error_when_display_name_is_missing()
    {
        var result = UserProfile.Create("user-1", "");

        Assert.True(result.IsError);
        Assert.Equal(UserProfileErrors.DisplayNameRequired, result.TopError);
    }

    [Fact]
    public void Update_changes_profile_fields()
    {
        var profile = UserProfile.Create("user-1", "Buyer One").Value;

        profile.Update(
            "Buyer Updated",
            "https://images.test/avatar.png",
            "Updated bio",
            "01000000001",
            ContactMethod.Phone);

        Assert.Equal("Buyer Updated", profile.DisplayName);
        Assert.Equal(ContactMethod.Phone, profile.PreferredContactMethod);
        Assert.Equal("01000000001", profile.PhoneNumber);
    }
}
