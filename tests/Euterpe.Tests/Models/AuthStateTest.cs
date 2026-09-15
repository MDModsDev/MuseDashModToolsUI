using Euterpe.Contracts.Account;

namespace Euterpe.Tests.Models;

[Category("AuthStateTests")]
[TestSubject(typeof(AuthState))]
public sealed class AuthStateTest
{
    private static readonly UserInfo TestUser = new(1, 0, "test@test.com", "TestUser", "avatar.png", false, false, false);

    [Test]
    public async Task Constructor_DefaultState_HasNoTokensAndNoUser()
    {
        var state = new AuthState();

        using var _ = Assert.Multiple();
        await Assert.That(state.AccessToken).IsNull();
        await Assert.That(state.RefreshToken).IsNull();
        await Assert.That(state.CurrentUser).IsNull();
        await Assert.That(state.AccessTokenExpiry).IsEqualTo(default);
    }

    [Test]
    public async Task Clear_PopulatedAuthState_ResetsAllFieldsToDefault()
    {
        var state = new AuthState
        {
            AccessToken = "access",
            RefreshToken = "refresh",
            AccessTokenExpiry = DateTimeOffset.Now,
            CurrentUser = TestUser
        };

        state.Clear();

        using var _ = Assert.Multiple();
        await Assert.That(state.AccessToken).IsNull();
        await Assert.That(state.RefreshToken).IsNull();
        await Assert.That(state.CurrentUser).IsNull();
        await Assert.That(state.AccessTokenExpiry).IsEqualTo(default);
    }

    [Test]
    [Arguments(null, "https://euterpe-org.com/")]
    [Arguments("avatar.png", "https://euterpe-org.com/avatar.png")]
    [Arguments("path/to/avatar.jpg", "https://euterpe-org.com/path/to/avatar.jpg")]
    public async Task AvatarUrl_CurrentUserWithAvatarPath_BuildsAbsoluteUrl(string? avatarUrl, string expected)
    {
        var state = new AuthState
        {
            CurrentUser = TestUser with { AvatarUrl = avatarUrl }
        };

        await Assert.That(state.AvatarUrl).IsEqualTo(expected);
    }

    [Test]
    public async Task AvatarUrl_WhenCurrentUserNull_UsesEmptyPath()
    {
        var state = new AuthState();
        await Assert.That(state.AvatarUrl).IsEqualTo("https://euterpe-org.com/");
    }

    [Test]
    public async Task CurrentUser_ValueChanged_RaisesAvatarUrlPropertyChanged()
    {
        var state = new AuthState();
        var changedProperties = new List<string?>();
        state.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        state.CurrentUser = TestUser;

        await Assert.That(changedProperties).Contains(nameof(AuthState.AvatarUrl));
    }
}
