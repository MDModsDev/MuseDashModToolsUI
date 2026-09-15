using Euterpe.Core.Logger;
using NLog;
using NLog.Targets;

namespace Euterpe.Tests.Core.Logger;

[Category("AppLoggingConfigurationTests")]
[TestSubject(typeof(AppLoggingConfiguration))]
public sealed class AppLoggingConfigurationTest
{
    [Test]
    public async Task Create_LiveLogTargetProvided_RegistersFileAndLiveLogTargets()
    {
        var liveLogTarget = new LiveLogTarget();
        var configuration = AppLoggingConfiguration.Create(liveLogTarget);

        using var _ = Assert.Multiple();
        await Assert.That(configuration.AllTargets.OfType<FileTarget>().Count()).IsEqualTo(1);
        await Assert.That(configuration.AllTargets.Contains(liveLogTarget)).IsTrue();
    }

    [Test]
    public async Task Create_LogEventWithException_FileLayoutIncludesLevelCategoryMessageAndException()
    {
        var configuration = AppLoggingConfiguration.Create(new LiveLogTarget());
        var fileTarget = configuration.AllTargets.OfType<FileTarget>().Single();
        var logEvent = new LogEventInfo(NLog.LogLevel.Error, "TestCategory", "file failed")
        {
            Exception = new InvalidOperationException("file boom")
        };

        var output = fileTarget.Layout.Render(logEvent);

        using var _ = Assert.Multiple();
        await Assert.That(output).Contains("[Error]");
        await Assert.That(output).Contains("(TestCategory)");
        await Assert.That(output).Contains("file failed");
        await Assert.That(output).Contains("InvalidOperationException");
        await Assert.That(output).Contains("file boom");
    }
}
