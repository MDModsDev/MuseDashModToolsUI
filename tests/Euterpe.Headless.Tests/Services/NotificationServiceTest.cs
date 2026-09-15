using Euterpe.Abstractions;
using Euterpe.Core;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Headless.Tests.Services;

[TestSubject(typeof(NotificationService))]
public sealed class NotificationServiceTest : HeadlessTest
{
    private static NotificationService NewWiredService()
    {
        var window = new Window();
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var service = new NotificationService();
        ((INotificationServiceWiring)service).Notifier = new WindowNotificationManager(window);
        return service;
    }

    [Test]
    public Task AllMethods_NotifierWired_DoNotThrow() => RunOnUI(async () =>
    {
        var service = NewWiredService();

        var act = () =>
        {
            service.Success("ok", TimeSpan.Zero);
            service.SuccessLight("ok", TimeSpan.Zero);
            service.Notice("ok", TimeSpan.Zero);
            service.NoticeLight("ok", TimeSpan.Zero);
            service.Warning("ok", TimeSpan.Zero);
            service.WarningLight("ok", TimeSpan.Zero);
            service.Error("ok", TimeSpan.Zero);
            service.ErrorLight("ok", TimeSpan.Zero);
        };

        await Assert.That(act).ThrowsNothing();
        Dispatcher.UIThread.RunJobs();
    });

    [Test]
    public Task FormatString_InvalidFormat_Throws() => RunOnUI(async () =>
    {
        var service = NewWiredService();

        var act = () => service.Success("missing arg {0} {1}", 42);
        await Assert.That(act).Throws<FormatException>();
    });

    [Test]
    public async Task Success_CalledOffUIThread_DispatchesWithoutThrowing()
    {
        var manager = await RunOnUI(() =>
        {
            var window = new Window();
            window.Show();
            Dispatcher.UIThread.RunJobs();
            return Task.FromResult(new WindowNotificationManager(window));
        });

        var service = new NotificationService();
        ((INotificationServiceWiring)service).Notifier = manager;

        var act = () => Task.Run(() => service.Success("from background", TimeSpan.Zero));
        await Assert.That(act).ThrowsNothing();

        await RunOnUI(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Error_NotifierNotWired_DoesNotThrow()
    {
        var service = new NotificationService();

        var act = () => service.Error("no host yet", TimeSpan.Zero);

        await Assert.That(act).ThrowsNothing();
    }
}
