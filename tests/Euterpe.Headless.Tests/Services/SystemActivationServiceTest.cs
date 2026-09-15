using System.Runtime.CompilerServices;
using Autofac;
using Euterpe.Abstractions;
using Euterpe.Features.Charting;
using Euterpe.Features.Share;
using Euterpe.Models;
using Euterpe.Models.Progress;
using Euterpe.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;
using TUnit.Mocks.Logging;
using Ursa.Controls;

namespace Euterpe.Headless.Tests.Services;

[TestSubject(typeof(SystemActivationService))]
public sealed class SystemActivationServiceTest : HeadlessTest
{
    private static SystemActivationService NewService(
        ISystemAssociationSetup? setup = null,
        MockLogger<SystemActivationService>? logger = null,
        IModManageService? modManageService = null,
        IChartManageService? chartManageService = null,
        ProgressDialogService? progressDialogService = null,
        ShareImportDialogService? shareImportDialogService = null)
    {
        var builder = new ContainerBuilder();
        if (modManageService is not null)
        {
            builder.RegisterInstance(modManageService).As<IModManageService>();
        }

        if (chartManageService is not null)
        {
            builder.RegisterInstance(chartManageService).As<IChartManageService>();
        }

        if (progressDialogService is not null)
        {
            builder.RegisterInstance(progressDialogService).AsSelf();
        }

        if (shareImportDialogService is not null)
        {
            builder.RegisterInstance(shareImportDialogService).AsSelf();
        }

        var container = builder.Build();
        return new SystemActivationService
        {
            NavigationService = new NavigationService
            {
                Logger = NullLogger<NavigationService>.Instance
            },
            NotificationService = INotificationService.Mock(),
            Logger = logger ?? Mock.Logger<SystemActivationService>(),
            AssociationSetup = setup ?? ISystemAssociationSetup.Mock(),
            GameScope = new BehaviorSubject<ILifetimeScope>(container)
        };
    }

    [Test]
    public Task HandleStartupArgs_Empty_DoesNothing() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<SystemActivationService>();
        var service = NewService(logger: logger);

        service.HandleStartupArgs([]);

        await Assert.That(logger.Entries).IsEmpty();
    });

    [Test]
    public Task HandleStartupArgs_NonUri_LogsWarning() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<SystemActivationService>();
        var service = NewService(logger: logger);

        service.HandleStartupArgs(["not-a-uri-at-all"]);

        var warning = logger.Entries.SingleOrDefault(e => e.LogLevel is LogLevel.Warning);
        using var _ = Assert.Multiple();
        await Assert.That(warning).IsNotNull();
        await Assert.That(warning!.Message).Contains("Unhandled activation");
    });

    [Test]
    public Task HandleActivation_NonAbsoluteUri_LogsWarning() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<SystemActivationService>();
        var service = NewService(logger: logger);

        service.HandleActivation("relative/path");

        var warning = logger.Entries.SingleOrDefault(e => e.LogLevel is LogLevel.Warning);
        using var _ = Assert.Multiple();
        await Assert.That(warning).IsNotNull();
        await Assert.That(warning!.Message).Contains("Unhandled activation");
    });

    [Test]
    public Task HandleActivation_WrongScheme_LogsWarning() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<SystemActivationService>();
        var service = NewService(logger: logger);

        service.HandleActivation("http://example.com/mod/install/foo");

        var warning = logger.Entries.SingleOrDefault(e => e.LogLevel is LogLevel.Warning);
        using var _ = Assert.Multiple();
        await Assert.That(warning).IsNotNull();
        await Assert.That(warning!.Message).Contains("Unhandled activation");
    });

    [Test]
    public Task HandleActivation_InvalidUri_LogsReceivedAtInfoLevel() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<SystemActivationService>();
        var service = NewService(logger: logger);

        service.HandleActivation("not-a-uri");

        var info = logger.Entries.SingleOrDefault(e => e.LogLevel is LogLevel.Information);
        using var _ = Assert.Multiple();
        await Assert.That(info).IsNotNull();
        await Assert.That(info!.Message).Contains("Activation received");
        await Assert.That(info.Message).Contains("not-a-uri");
    });

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "GetEpkPath")]
    private static extern string? InvokeGetEpkPath(SystemActivationService? service, string argument);

    [Test]
    public async Task GetEpkPath_RelativeEpkPath_ReturnsArgument()
    {
        await Assert.That(InvokeGetEpkPath(null, "foo.epk")).IsEqualTo("foo.epk");
    }

    [Test]
    public async Task GetEpkPath_EpkFileUri_ReturnsLocalPathEndingInExtension()
    {
        var result = InvokeGetEpkPath(null, "file:///charts/foo.epk");

        using var _ = Assert.Multiple();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!).EndsWith(".epk");
    }

    [Test]
    [Arguments("euterpe://mod/install/foo")]
    [Arguments("http://example.com/charts/foo.epk")]
    [Arguments("file:///charts/foo.txt")]
    [Arguments("not-a-uri")]
    public async Task GetEpkPath_NonEpk_ReturnsNull(string argument)
    {
        await Assert.That(InvokeGetEpkPath(null, argument)).IsNull();
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "ShouldActivateWindow")]
    private static extern bool InvokeShouldActivateWindow(SystemActivationService? service, string query);

    [Test]
    [Arguments("", true)]
    [Arguments("silent=true", false)]
    [Arguments("?silent=true", false)]
    [Arguments("?silent=TRUE", false)]
    [Arguments("?silent=false", true)]
    [Arguments("?silent=1", true)]
    [Arguments("?silent", true)]
    [Arguments("?other=1&silent=true", false)]
    [Arguments("?presilent=1", true)]
    public async Task ShouldActivateWindow_SilentQueryVariations_HonorsSilentFlag(string query, bool expected)
    {
        await Assert.That(InvokeShouldActivateWindow(null, query)).IsEqualTo(expected);
    }

    // Private HandleModActionAsync / HandleChartActionAsync tests bypass the NavigationService.Ready gate + ActivateMainWindow.

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HandleModActionAsync")]
    private static extern Task InvokeModAction(SystemActivationService service, string path);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HandleChartActionAsync")]
    private static extern Task InvokeChartAction(SystemActivationService service, string path);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HandleActionAsync")]
    private static extern Task InvokeAction(SystemActivationService service, string action, string path);

    [Test]
    public async Task HandleActionAsync_Share_WaitsUntilMainWindowIsReadyAndOpensImportDialog()
    {
        var dialogService = IDialogService.Mock();
        dialogService.ShowOverlayAsync<ShareImportDialog, ShareImportDialogViewModel>(
            Any<ShareImportDialogViewModel>(), Any<OverlayDialogOptions?>(), Any<string?>(), Any<CancellationToken?>());
        var shareService = IGameShareService.Mock();
        var viewModel = new ShareImportDialogViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            GameShareService = shareService,
            Config = new Config(),
            Logger = NullLogger<ShareImportDialogViewModel>.Instance,
            TopLevel = null!
        };
        var dialog = new ShareImportDialogService
        {
            DialogService = dialogService,
            ShareImportDialogViewModel = viewModel
        };
        var service = NewService(shareImportDialogService: dialog);

        var activation = InvokeAction(service, "share", "encoded-package");
        await Assert.That(activation.IsCompleted).IsFalse();

        service.NavigationService.Ready.Set();
        await activation;

        shareService.TryParseShareLink("encoded-package").WasCalled(Times.Once);
    }

    [Test]
    public async Task HandleModAction_UpdateWithoutName_UpdatesAllMods()
    {
        var mods = IModManageService.Mock();
        var service = NewService(modManageService: mods);

        await InvokeModAction(service, "update");

        using var _ = Assert.Multiple();
        mods.UpdateAllModsAsync().WasCalled(Times.Once);
        mods.UpdateModByNameAsync(Any<string>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task HandleModAction_InstallWithName_DelegatesToInstallByName()
    {
        var mods = IModManageService.Mock();
        var service = NewService(modManageService: mods);

        await InvokeModAction(service, "install/Euterpe");

        mods.InstallModByNameAsync("Euterpe").WasCalled(Times.Once);
    }

    [Test]
    public async Task HandleModAction_UpdateWithName_DelegatesToUpdateByName()
    {
        var mods = IModManageService.Mock();
        var service = NewService(modManageService: mods);

        await InvokeModAction(service, "update/Euterpe");

        using var _ = Assert.Multiple();
        mods.UpdateModByNameAsync("Euterpe").WasCalled(Times.Once);
        mods.UpdateAllModsAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task HandleModAction_UninstallWithName_DelegatesToUninstallByName()
    {
        var mods = IModManageService.Mock();
        var service = NewService(modManageService: mods);

        await InvokeModAction(service, "uninstall/Euterpe");

        mods.UninstallModByNameAsync("Euterpe").WasCalled(Times.Once);
    }

    [Test]
    public Task HandleChartAction_Convert_MigratesCustomAlbums() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<SystemActivationService>();
        var charts = IChartManageService.Mock();
        var progressDialogService = new ProgressDialogService
        {
            DialogService = IDialogService.Mock(),
            ProgressDialogViewModel = new ProgressDialogViewModel { Launcher = IPlatformLauncher.Mock() }
        };
        var service = NewService(logger: logger, chartManageService: charts, progressDialogService: progressDialogService);

        await InvokeChartAction(service, "convert");

        using var _ = Assert.Multiple();
        charts.MigrateCustomAlbumsAsync(Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).WasCalled(Times.Once);
        await Assert.That(logger.Entries.Any(e => e.LogLevel is LogLevel.Warning)).IsFalse();
    });

    [Test]
    public async Task HandleChartAction_UnknownPath_LogsWarning()
    {
        var logger = Mock.Logger<SystemActivationService>();
        var service = NewService(logger: logger);

        await InvokeChartAction(service, "bogus");

        await Assert.That(logger.Entries.Any(e => e.LogLevel is LogLevel.Warning && e.Message.Contains("Unknown chart deep link"))).IsTrue();
    }
}
