using System.Net;
using System.Web;
using Euterpe.Core.Http.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe.Tests.Core.Http.Handlers;

[Category("TokenQueryHandlerTests")]
[TestSubject(typeof(TokenQueryHandler))]
public sealed class TokenQueryHandlerTest
{
    private static IServiceProvider BuildServices(IAuthService authService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(authService);
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task SendAsync_DownloadUrlWithoutQuery_AppendsTokenAsQueryParameter()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("token-abc");
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond();
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync($"{EuterpeDownload.BaseUrl}song");

        var query = HttpUtility.ParseQueryString(inner.Requests.Single().RequestUri!.Query);
        await Assert.That(query["t"]).IsEqualTo("token-abc");
    }

    [Test]
    public async Task SendAsync_DownloadUrlWithQuery_PreservesExistingQueryParameters()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("tok");
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond();
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync($"{EuterpeDownload.BaseUrl}song?id=42&kind=audio");

        var query = HttpUtility.ParseQueryString(inner.Requests.Single().RequestUri!.Query);
        using var assertions = Assert.Multiple();
        await Assert.That(query["id"]).IsEqualTo("42");
        await Assert.That(query["kind"]).IsEqualTo("audio");
        await Assert.That(query["t"]).IsEqualTo("tok");
    }

    [Test]
    public async Task SendAsync_OnUnauthorized_RenewsTokenAndRetriesWithNewToken()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("fresh");
        var inner = Mock.HttpHandler();
        var sequence = inner.OnAnyRequest();
        sequence.Respond(HttpStatusCode.Unauthorized);
        sequence.Respond();
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync($"{EuterpeDownload.BaseUrl}song");

        var firstQuery = HttpUtility.ParseQueryString(inner.Requests[0].RequestUri!.Query);
        var secondQuery = HttpUtility.ParseQueryString(inner.Requests[1].RequestUri!.Query);
        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(inner.Requests.Count).IsEqualTo(2);
        await Assert.That(firstQuery["t"]).IsEqualTo("expired");
        await Assert.That(secondQuery["t"]).IsEqualTo("fresh");
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task SendAsync_NonUnauthorizedFailure_DoesNotRenew()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("tok");
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond(HttpStatusCode.NotFound);
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync($"{EuterpeDownload.BaseUrl}missing");

        using var assertions = Assert.Multiple();
        await Assert.That(inner.Requests.Count).IsEqualTo(1);
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task SendAsync_NonDownloadOrigins_DoesNotAppendOrRequestToken()
    {
        var auth = IAuthService.Mock();
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond();
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);
        string[] urls =
        [
            "https://aka.ms/dotnet/6.0/dotnet-runtime-win-x64.zip",
            "https://dl.euterpe-org.com.attacker.test/files/song",
            "http://dl.euterpe-org.com/files/song"
        ];

        foreach (var url in urls)
        {
            await client.GetAsync(url);
        }

        using var assertions = Assert.Multiple();
        foreach (var request in inner.Requests)
        {
            await Assert.That(HttpUtility.ParseQueryString(request.RequestUri!.Query)["t"]).IsNull();
        }

        auth.GetAccessTokenAsync().WasCalled(Times.Never);
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Never);
    }
}
