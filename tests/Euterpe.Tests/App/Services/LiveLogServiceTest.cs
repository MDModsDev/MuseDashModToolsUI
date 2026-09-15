using System.Collections;
using Euterpe.Core.Logger;
using Euterpe.Services;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLog;
using NLog.Config;

namespace Euterpe.Tests.App.Services;

[Category("LiveLogServiceTests")]
[TestSubject(typeof(LiveLogService))]
public sealed class LiveLogServiceTest
{
    private static List<LogMessage> MaterializeView(IEnumerable view)
    {
        var list = new List<LogMessage>();
        foreach (LogMessage item in view)
        {
            list.Add(item);
        }

        return list;
    }

    private static (LiveLogService Service, LogFactory Factory) CreateWiredService()
    {
        var target = new LiveLogTarget();
        var service = new LiveLogService(target);
        var logFactory = new LogFactory();
        var configuration = new LoggingConfiguration(logFactory);
        configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, target);
        logFactory.Configuration = configuration;
        return (service, logFactory);
    }

    [Test]
    public async Task Ctor_NoMessages_ViewIsEmpty()
    {
        var service = new LiveLogService(new LiveLogTarget());

        await Assert.That(service.LogMessagesView).IsEmpty();
    }

    [Test]
    public async Task LogMessagesView_MessageReceived_AppendsMessage()
    {
        var (service, factory) = CreateWiredService();
        using (factory)
        {
            factory.GetLogger("Euterpe.Tests").Info("hello world");
        }

        var view = MaterializeView(service.LogMessagesView);
        using var _ = Assert.Multiple();
        await Assert.That(view).HasSingleItem();
        await Assert.That(view[0].Message).IsEqualTo("hello world");
        await Assert.That(view[0].LogLevel).IsEqualTo(MicrosoftLogLevel.Information);
    }

    [Test]
    public async Task LogMessagesView_MultipleMessagesReceived_PreservesAppendOrder()
    {
        var (service, factory) = CreateWiredService();
        using (factory)
        {
            var logger = factory.GetLogger("Euterpe.Tests");
            logger.Info("first");
            logger.Info("second");
            logger.Info("third");
        }

        var view = MaterializeView(service.LogMessagesView);
        using var _ = Assert.Multiple();
        await Assert.That(view).Count().IsEqualTo(3);
        await Assert.That(view[0].Message).IsEqualTo("first");
        await Assert.That(view[1].Message).IsEqualTo("second");
        await Assert.That(view[2].Message).IsEqualTo("third");
    }

    [Test]
    public async Task LogMessagesView_CapacityExceeded_DropsOldestMessages()
    {
        var (service, factory) = CreateWiredService();
        using (factory)
        {
            var logger = factory.GetLogger("Euterpe.Tests");
            for (var i = 0; i < 55; i++)
            {
                logger.Info("msg-{0}", i);
            }
        }

        var view = MaterializeView(service.LogMessagesView);
        using var _ = Assert.Multiple();
        await Assert.That(view).Count().IsEqualTo(50);
        await Assert.That(view[0].Message).IsEqualTo("msg-5");
        await Assert.That(view[49].Message).IsEqualTo("msg-54");
    }

    [Test]
    public async Task TargetEvent_FilteredCategory_DoesNotAppendToView()
    {
        var (service, factory) = CreateWiredService();
        using (factory)
        {
            factory.GetLogger("Euterpe.Services.NavigationService").Info("routed");
        }

        await Assert.That(service.LogMessagesView).IsEmpty();
    }
}
