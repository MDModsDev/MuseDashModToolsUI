using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests.Core;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task LogoutAsync_LoggedIn_ClearsStateAndResetsReady()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Returns(new AppTokenResponse(ValidAccessToken, ValidRefreshToken, TestUser));
        var sut = CreateLoginReadyService(authClientMock);

        await sut.LoginAsync();
        await sut.LogoutAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsFalse();
        await Assert.That(sut.AuthState.AccessToken).IsNull();
        await Assert.That(sut.AuthState.RefreshToken).IsNull();
    }

    [Test]
    public async Task LogoutAsync_WhenLogoutApiFails_ShouldStillClearState()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Returns(new AppTokenResponse(ValidAccessToken, ValidRefreshToken, TestUser));
        authClientMock.LogoutAsync(Any<LogoutRequest>(), Any<CancellationToken>())
            .Throws(new HttpRequestException("Network error"));
        var sut = CreateLoginReadyService(authClientMock);

        await sut.LoginAsync();
        await sut.LogoutAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsFalse();
        await Assert.That(sut.AuthState.AccessToken).IsNull();
    }
}
