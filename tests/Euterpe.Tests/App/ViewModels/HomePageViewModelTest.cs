using Euterpe.Features.Home;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("HomePageViewModelTests")]
[TestSubject(typeof(HomePageViewModel))]
public sealed class HomePageViewModelTest
{
    [Test]
    public async Task GameModes_DefaultOptions_HasModdedAndVanilla()
    {
        using var _ = Assert.Multiple();
        await Assert.That(HomePageViewModel.GameModes).Count().IsEqualTo(2);
        await Assert.That(HomePageViewModel.GameModes[0].Value).IsEqualTo(GameMode.Modded);
        await Assert.That(HomePageViewModel.GameModes[1].Value).IsEqualTo(GameMode.Vanilla);
    }

    [Test]
    public async Task LaunchGameCommand_ModdedMode_CallsLaunchModdedGame()
    {
        var launchService = IGameLaunchService.Mock();
        var gameConfig = new MuseDashConfig { GameMode = GameMode.Modded };
        var vm = NewViewModel(gameConfig, launchService);

        await vm.LaunchGameCommand.ExecuteAsync(null);

        launchService.LaunchModdedGameAsync().WasCalled(Times.Once);
        launchService.LaunchVanillaGameAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task LaunchGameCommand_VanillaMode_CallsLaunchVanillaGame()
    {
        var launchService = IGameLaunchService.Mock();
        var gameConfig = new MuseDashConfig { GameMode = GameMode.Vanilla };
        var vm = NewViewModel(gameConfig, launchService);

        await vm.LaunchGameCommand.ExecuteAsync(null);

        launchService.LaunchVanillaGameAsync().WasCalled(Times.Once);
        launchService.LaunchModdedGameAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task LaunchGameCommand_LaunchFails_PropagatesException()
    {
        var launchService = IGameLaunchService.Mock();
        launchService.LaunchModdedGameAsync().Throws<InvalidOperationException>();
        var gameConfig = new MuseDashConfig { GameMode = GameMode.Modded };
        var vm = NewViewModel(gameConfig, launchService);

        var act = async () => await vm.LaunchGameCommand.ExecuteAsync(null);
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    private static HomePageViewModel NewViewModel(GameConfig gameConfig, IGameLaunchService? launchService = null) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<HomePageViewModel>.Instance,
        GameConfig = gameConfig,
        AccountClient = null!,
        DependencyAcquireService = IDependencyAcquireService.Mock(),
        GameLaunchService = launchService ?? IGameLaunchService.Mock(),
        GameSettingService = IGameSettingService.Mock(),
        GameLocalService = IGameLocalService.Mock(),
        ChartManageService = IChartManageService.Mock(),
        MessageBoxService = IMessageBoxService.Mock(),
        RuntimeInstaller = IGameRuntimeInstaller.Mock(),
        NavigationService = null!,
        SetupDialogService = null!
    };
}
