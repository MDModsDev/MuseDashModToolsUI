using Autofac;
using Euterpe.Core.Extensions;
using Euterpe.Core.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Euterpe.Tests.Core.Extensions;

[Category("CoreServiceExtensionsTests")]
[TestSubject(typeof(CoreServiceExtensions))]
public sealed partial class CoreServiceExtensionsTest
{
    [Test]
    public async Task RegisterLogger_EmptyServiceCollection_RegistersLoggerServices()
    {
        var services = new ServiceCollection();
        services.RegisterLogger();

        using var _ = Assert.Multiple();
        await Assert.That(services.Any(s => s.ServiceType == typeof(LiveLogTarget))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(ILoggerFactory))).IsTrue();
    }

    [Test]
    [Arguments(GameId.MuseDash, typeof(MuseDashConfig))]
    [Arguments(GameId.MuseDash2, typeof(MuseDash2Config))]
    public async Task RegisterPerGameCoreServices_GameIdProvided_ResolvesMatchingGameConfig(GameId gameId, Type expectedConcrete)
    {
        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        builder.RegisterPerGameCoreServices(gameId);

        await using var container = builder.Build();
        var resolved = container.Resolve<GameConfig>();

        await Assert.That(resolved.GetType()).IsEqualTo(expectedConcrete);
    }

    [Test]
    public async Task RegisterAppCoreServices_EmptyContainerBuilder_RegistersAllAppLevelSingletons()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        await using var container = builder.Build();

        Type[] expected =
        [
            typeof(IAppDownloadManager),
            typeof(IAppLocalService),
            typeof(IAppSettingService),
            typeof(IArchiveService),
            typeof(IAuthService),
            typeof(ICrashLogUploadService),
            typeof(IFileSystemService),
            typeof(IFileSystemPickerService),
            typeof(IJsonSerializationService),
            typeof(IMessageBoxService),
            typeof(INotificationService),
            typeof(IResourceService),
            typeof(ITelemetryService),
            typeof(IUpdateService),
            typeof(IVdfSerializationService),
            typeof(AuthState),
            typeof(Config),
            typeof(MuseDashConfig),
            typeof(MuseDash2Config)
        ];

        using var _ = Assert.Multiple();
        foreach (var t in expected)
        {
            await Assert.That(container.IsRegistered(t)).IsTrue();
        }
    }
}
