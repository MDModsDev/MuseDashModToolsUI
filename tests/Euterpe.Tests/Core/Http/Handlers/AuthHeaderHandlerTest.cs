using System.Net;
using Euterpe.Core.Http.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe.Tests.Core.Http.Handlers;

[Category("AuthHeaderHandlerTests")]
[TestSubject(typeof(AuthHeaderHandler))]
public sealed class AuthHeaderHandlerTest
{
    private static IServiceProvider BuildServices(IAuthService authService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(authService);
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task SendAsync_AccessTokenAvailable_AddsBearerTokenHeader()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("token-123");
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond();
        using var handler = new AuthHeaderHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        await Assert.That(inner.Requests.Single().Headers["Authorization"].Single()).IsEqualTo("Bearer token-123");
    }

    [Test]
    public async Task SendAsync_OnUnauthorized_RenewsTokenAndRetries()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("renewed");
        var inner = Mock.HttpHandler();
        var sequence = inner.OnAnyRequest();
        sequence.Respond(HttpStatusCode.Unauthorized);
        sequence.Respond();
        using var handler = new AuthHeaderHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(inner.Requests.Count).IsEqualTo(2);
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Once);
        await Assert.That(inner.Requests[0].Headers["Authorization"].Single()).IsEqualTo("Bearer expired");
        await Assert.That(inner.Requests[1].Headers["Authorization"].Single()).IsEqualTo("Bearer renewed");
    }

    [Test]
    public async Task SendAsync_RetryStillUnauthorized_ReturnsSecondResponse()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("still-bad");
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond(HttpStatusCode.Unauthorized);
        using var handler = new AuthHeaderHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(inner.Requests.Count).IsEqualTo(2);
    }

    [Test]
    public async Task SendAsync_NonUnauthorizedFailure_DoesNotRenew()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("token");
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond(HttpStatusCode.InternalServerError);
        using var handler = new AuthHeaderHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(inner.Requests.Count).IsEqualTo(1);
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Never);
    }
}
