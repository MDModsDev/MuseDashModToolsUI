using System.Net;
using Downloader;
using Euterpe.Core.Extensions;
using Euterpe.Core.Http.Clients;
using Euterpe.Core.Http.Handlers;
using Euterpe.Shared.Http;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Mocks.Http;

namespace Euterpe.Tests.Core.Extensions;

public sealed partial class CoreServiceExtensionsTest
{
    private static ServiceProvider BuildRefitPipelineProvider(
        string clientName,
        IAuthService authService,
        MockHttpHandler primary,
        INotificationService? notificationService = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.RegisterHttpClients();
        services.AddSingleton(authService);
        services.AddSingleton(notificationService ?? INotificationService.Mock());
        // Refit configures a per-name primary handler, so reconfigure the mock after registration.
        services.AddHttpClient(clientName).ConfigurePrimaryHttpMessageHandler(() => primary);
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task RegisterHttpClients_EmptyServiceCollection_RegistersAllHandlersAndDownloadService()
    {
        var services = new ServiceCollection();
        services.RegisterHttpClients();
        var provider = services.BuildServiceProvider();

        using var _ = Assert.Multiple();

        await Assert.That(provider.GetService<XRequestIdHandler>()).IsNotNull();
        await Assert.That(provider.GetService<AuthHeaderHandler>()).IsNotNull();
        await Assert.That(provider.GetService<LoggingHandler>()).IsNotNull();
        await Assert.That(provider.GetService<ServerErrorHandler>()).IsNotNull();
        await Assert.That(provider.GetService<TokenQueryHandler>()).IsNotNull();
        await Assert.That(services.Any(s => s.ServiceType == typeof(Func<DownloadService>))).IsTrue();
        await Assert.That(provider.GetService<IHttpClientFactory>()).IsNotNull();
    }

    [Test]
    public async Task RegisterHttpClients_EmptyServiceCollection_RegistersAllRefitClientServiceDescriptors()
    {
        var services = new ServiceCollection();
        services.RegisterHttpClients();

        Type[] expectedRefitClients =
        [
            typeof(IEuterpeAccountClient),
            typeof(IEuterpeAuthClient),
            typeof(IEuterpeChartClient),
            typeof(IEuterpeCreditsClient),
            typeof(IEuterpeDistributionClient),
            typeof(IEuterpeLogClient),
            typeof(IEuterpeModClient),
            typeof(IEuterpeTelemetryClient),
            typeof(IEuterpeHealthClient)
        ];

        using var _ = Assert.Multiple();
        foreach (var t in expectedRefitClients)
        {
            await Assert.That(services.Any(s => s.ServiceType == t)).IsTrue();
        }

        await Assert.That(services.Any(s => s.ServiceType == typeof(EuterpeDownloadClient))).IsTrue();
    }

    [Test]
    public async Task RegisterHttpClients_Unauthorized_RenewsTokenOnceAndKeepsRequestId()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("renewed");
        var primary = Mock.HttpHandler();
        var manifest = primary.OnGet("/api/mods/app-manifest");
        manifest.Respond(HttpStatusCode.Unauthorized);
        manifest.RespondWithJson("[]");
        await using var provider = BuildRefitPipelineProvider(nameof(EuterpeApi.Mods), auth, primary);

        var mods = await provider.GetRequiredService<IEuterpeModClient>().GetModManifestAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(mods).IsEmpty();
        await Assert.That(primary.Requests.Count).IsEqualTo(2);
        auth.RenewAccessTokenAsync("expired").WasCalled(Times.Once);
        await Assert.That(primary.Requests[0].Headers["Authorization"].Single()).IsEqualTo("Bearer expired");
        await Assert.That(primary.Requests[1].Headers["Authorization"].Single()).IsEqualTo("Bearer renewed");
        await Assert.That(primary.Requests[0].RequestUri!.AbsoluteUri).IsEqualTo("https://euterpe-org.com/api/mods/app-manifest");
        var firstRequestId = primary.Requests[0].Headers["X-Request-Id"].Single();
        var retryRequestId = primary.Requests[1].Headers["X-Request-Id"].Single();
        await Assert.That(Guid.TryParse(firstRequestId, out _)).IsTrue();
        await Assert.That(retryRequestId).IsEqualTo(firstRequestId);
    }

    [Test]
    public async Task RegisterHttpClients_TransientServerErrorRecovered_RetriesWithoutServerErrorNotification()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("token");
        var notification = INotificationService.Mock();
        var primary = Mock.HttpHandler();
        var manifest = primary.OnGet("/api/mods/app-manifest");
        manifest.Respond(HttpStatusCode.InternalServerError);
        manifest.RespondWithJson("[]");
        await using var provider = BuildRefitPipelineProvider(nameof(EuterpeApi.Mods), auth, primary, notification);

        var mods = await provider.GetRequiredService<IEuterpeModClient>().GetModManifestAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(mods).IsEmpty();
        await Assert.That(primary.Requests.Count).IsEqualTo(2);
        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }
}
