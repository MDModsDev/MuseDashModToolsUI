using System.Net;
using Euterpe.Core.Http.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Euterpe.Tests.Core.Http.Handlers;

[Category("ServerErrorHandlerTests")]
[TestSubject(typeof(ServerErrorHandler))]
public sealed class ServerErrorHandlerTest
{
    private static IServiceProvider BuildServices(INotificationService notificationService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(notificationService);
        return services.BuildServiceProvider();
    }

    private static ServerErrorHandler CreateHandler(INotificationService notification, ILogger<ServerErrorNotifier> logger, HttpMessageHandler inner) =>
        new(new ServerErrorNotifier(BuildServices(notification), logger)) { InnerHandler = inner };

    [Test]
    public async Task SendAsync_SuccessResponse_DoesNotNotifyOrLog()
    {
        var notification = INotificationService.Mock();
        var logger = Mock.Logger<ServerErrorNotifier>();
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond();
        using var handler = CreateHandler(notification, logger, inner);
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(logger.Entries).IsEmpty();
        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task SendAsync_ClientErrorResponse_DoesNotNotify()
    {
        var notification = INotificationService.Mock();
        var logger = Mock.Logger<ServerErrorNotifier>();
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond(HttpStatusCode.NotFound);
        using var handler = CreateHandler(notification, logger, inner);
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task SendAsync_ServerErrorResponse_LogsWarningAndNotifies()
    {
        var notification = INotificationService.Mock();
        var logger = Mock.Logger<ServerErrorNotifier>();
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond(HttpStatusCode.InternalServerError);
        using var handler = CreateHandler(notification, logger, inner);
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/api/x");

        using var assertions = Assert.Multiple();
        await Assert.That(logger.Entries).HasSingleItem();
        await Assert.That(logger.Entries[0].LogLevel).IsEqualTo(LogLevel.Warning);
        await Assert.That(logger.Entries[0].Message).Contains("500");
        await Assert.That(logger.Entries[0].Message).Contains("https://example.test/api/x");
        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task SendAsync_ConsecutiveServerErrors_DebouncesNotification()
    {
        var notification = INotificationService.Mock();
        var logger = Mock.Logger<ServerErrorNotifier>();
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond(HttpStatusCode.BadGateway);
        using var handler = CreateHandler(notification, logger, inner);
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");
        await client.GetAsync("https://example.test/");
        await client.GetAsync("https://example.test/");

        notification.Warning(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
    }
}
