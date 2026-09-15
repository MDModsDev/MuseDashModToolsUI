using Euterpe.Features.Setup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("GamePathPageViewModelTests")]
[TestSubject(typeof(GamePathPageViewModel))]
public sealed class GamePathPageViewModelTest
{
    [Test]
    public async Task OnInitializeAsync_AutoDetectedFolder_PopulatesSelectedFolder()
    {
        var paths = IGamePathDiscovery.Mock();
        paths.TryGetGameFolder().SetsOutGameFolder("/auto/musedash").Returns(true);
        var vm = NewViewModel(paths);

        await vm.InitializeAsync();

        await Assert.That(vm.SelectedFolder).IsEqualTo("/auto/musedash");
    }

    [Test]
    public async Task OnInitializeAsync_NoFolderDetected_SelectedFolderIsNull()
    {
        var paths = IGamePathDiscovery.Mock();
        paths.TryGetGameFolder().Returns(false);
        var vm = NewViewModel(paths);

        await vm.InitializeAsync();

        await Assert.That(vm.SelectedFolder).IsNull();
    }

    [Test]
    public async Task SelectedFolder_Changed_WritesToGameConfig()
    {
        var gameConfig = new MuseDashConfig();
        var vm = NewViewModel(gameConfig: gameConfig);

        vm.SelectedFolder = "/games/musedash";

        await Assert.That(gameConfig.Folder).IsEqualTo("/games/musedash");
    }

    [Test]
    public async Task ShowInvalidMessage_EmptySelectedFolder_IsFalse()
    {
        var vm = NewViewModel();
        vm.SelectedFolder = string.Empty;

        await Assert.That(vm.ShowInvalidMessage).IsFalse();
    }

    [Test]
    public async Task ShowInvalidMessage_InvalidSelectedFolder_IsTrue()
    {
        var paths = IGamePathDiscovery.Mock();
        paths.CheckIsValidGameFolder(Any<string?>()).Returns(false);
        var vm = NewViewModel(paths);

        vm.SelectedFolder = "/not/a/game/folder";

        await Assert.That(vm.ShowInvalidMessage).IsTrue();
    }

    [Test]
    public async Task ShowInvalidMessage_ValidSelectedFolder_IsFalse()
    {
        var paths = IGamePathDiscovery.Mock();
        paths.CheckIsValidGameFolder(Any<string?>()).Returns(true);
        var vm = NewViewModel(paths);

        vm.SelectedFolder = "/games/musedash";

        await Assert.That(vm.ShowInvalidMessage).IsFalse();
    }

    private static GamePathPageViewModel NewViewModel(
        IGamePathDiscovery? paths = null,
        GameConfig? gameConfig = null) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<GamePathPageViewModel>.Instance,
        State = null!,
        GameConfig = gameConfig ?? new MuseDashConfig(),
        GamePaths = paths ?? IGamePathDiscovery.Mock(),
        FileSystemPickerService = null!
    };
}
