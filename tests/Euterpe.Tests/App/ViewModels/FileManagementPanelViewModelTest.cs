using Euterpe.Features.Setting;

namespace Euterpe.Tests.App.ViewModels;

[Category("FileManagementPanelViewModelTests")]
[TestSubject(typeof(FileManagementPanelViewModel))]
public sealed class FileManagementPanelViewModelTest
{
    [Test]
    public async Task ChangeGameFolderCommand_FolderPicked_WritesPathToGameConfig()
    {
        var gameLocal = IGameLocalService.Mock();
        gameLocal.GetGameFolderAsync().Returns("/games/musedash");
        var gameConfig = new MuseDashConfig { Folder = "/old" };
        var vm = NewViewModel(gameLocal: gameLocal, gameConfig: gameConfig);

        await vm.ChangeGameFolderCommand.ExecuteAsync(null);

        await Assert.That(gameConfig.Folder).IsEqualTo("/games/musedash");
    }

    [Test]
    public async Task ChangeCacheFolderCommand_FolderPicked_WritesPathToConfig()
    {
        var appLocal = IAppLocalService.Mock();
        appLocal.GetCacheFolderAsync().Returns("/cache/new");
        var config = NewConfig();
        config.CacheFolder = "/cache/old";
        var vm = NewViewModel(appLocal, config: config);

        await vm.ChangeCacheFolderCommand.ExecuteAsync(null);

        await Assert.That(config.CacheFolder).IsEqualTo("/cache/new");
    }

    private static Config NewConfig() =>
        new() { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() };

    private static FileManagementPanelViewModel NewViewModel(
        IAppLocalService? appLocal = null,
        IGameLocalService? gameLocal = null,
        Config? config = null,
        GameConfig? gameConfig = null) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        AppLocalService = appLocal ?? IAppLocalService.Mock(),
        GameLocalService = gameLocal ?? IGameLocalService.Mock(),
        Config = config ?? NewConfig(),
        GameConfig = gameConfig ?? new MuseDashConfig(),
        Logger = Mock.Logger<FileManagementPanelViewModel>()
    };
}
