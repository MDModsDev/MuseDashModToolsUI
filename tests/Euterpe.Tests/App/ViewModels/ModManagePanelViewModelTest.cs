using System.Reactive.Linq;
using DynamicData;
using Euterpe.Features.Modding;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("ModManagePanelViewModelTests")]
[TestSubject(typeof(ModManagePanelViewModel))]
public sealed class ModManagePanelViewModelTest
{
    [Test]
    public async Task Constructor_DefaultState_InitializesWithEmptyMods()
    {
        var vm = NewViewModel();

        using var assertions = Assert.Multiple();
        await Assert.That(vm.Mods).IsEmpty();
        await Assert.That(vm.Filter.SearchText).IsNull();
        await Assert.That(vm.Filter.ModFilter).IsEqualTo(ModFilterType.All);
        await Assert.That(vm.AllModsLoaded).IsFalse();
    }

    [Test]
    public async Task ModFilters_DefaultOptions_HasSixOptions() =>
        await Assert.That(ModManagePanelViewModel.ModFilters).Count().IsEqualTo(6);

    [Test]
    public async Task OpenConfigFileCommand_ModHasConfigFile_OpensComposedPath()
    {
        var launcher = IPlatformLauncher.Mock();
        var openedFiles = new List<string>();
        launcher.OpenFileAsync(Any<string>()).Callback(p => openedFiles.Add(p));
        var gameConfig = new MuseDashConfig { Folder = "/games/musedash" };
        var vm = new ModManagePanelViewModel
        {
            Launcher = launcher,
            Logger = NullLogger<ModManagePanelViewModel>.Instance,
            Config = new Config { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() },
            GameConfig = gameConfig,
            ModManageService = IModManageService.Mock(),
            SelectedMod = new ModDto { Name = "TestMod", ConfigFile = "config.json" }
        };

        await vm.OpenConfigFileCommand.ExecuteAsync(null);

        using var assertions = Assert.Multiple();
        await Assert.That(openedFiles).Count().IsEqualTo(1);
        await Assert.That(openedFiles[0]).EndsWith("config.json");
        await Assert.That(openedFiles[0]).Contains(gameConfig.UserDataFolder);
    }

    [Test]
    public async Task InstallModCommand_ModProvided_DelegatesToService()
    {
        var modManageService = IModManageService.Mock();
        var vm = NewViewModel(modManageService);
        var mod = new ModDto { Name = "TestMod" };

        await vm.InstallModCommand.ExecuteAsync(mod);

        modManageService.InstallModAsync(mod).WasCalled(Times.Once);
    }

    [Test]
    public async Task UpdateModCommand_ModProvided_DelegatesToService()
    {
        var modManageService = IModManageService.Mock();
        var vm = NewViewModel(modManageService);
        var mod = new ModDto { Name = "TestMod" };

        await vm.UpdateModCommand.ExecuteAsync(mod);

        modManageService.UpdateModAsync(mod).WasCalled(Times.Once);
    }

    [Test]
    public async Task ReinstallModCommand_ModProvided_DelegatesToService()
    {
        var modManageService = IModManageService.Mock();
        var vm = NewViewModel(modManageService);
        var mod = new ModDto { Name = "TestMod" };

        await vm.ReinstallModCommand.ExecuteAsync(mod);

        modManageService.ReinstallModAsync(mod).WasCalled(Times.Once);
    }

    [Test]
    public async Task UninstallModCommand_ModProvided_DelegatesToService()
    {
        var modManageService = IModManageService.Mock();
        var vm = NewViewModel(modManageService);
        var mod = new ModDto { Name = "TestMod" };

        await vm.UninstallModCommand.ExecuteAsync(mod);

        modManageService.UninstallModAsync(mod).WasCalled(Times.Once);
    }

    [Test]
    public async Task ToggleModCommand_ModProvided_DelegatesToService()
    {
        var modManageService = IModManageService.Mock();
        var vm = NewViewModel(modManageService);
        var mod = new ModDto { Name = "TestMod" };

        await vm.ToggleModCommand.ExecuteAsync(mod);

        modManageService.ToggleModAsync(mod).WasCalled(Times.Once);
    }

    [Test]
    public async Task InitializeAsync_ModServiceAvailable_InitializesModsAndSetsAllModsLoaded()
    {
        var modManageService = IModManageService.Mock();
        modManageService.Connect().Returns(Observable.Empty<IChangeSet<ModDto, string>>());
        var vm = NewViewModel(modManageService);

        await vm.InitializeAsync();

        using var _ = Assert.Multiple();
        modManageService.InitializeModsAsync().WasCalled(Times.Once);
        await Assert.That(vm.AllModsLoaded).IsTrue();
    }

    private static ModManagePanelViewModel NewViewModel(IModManageService? modManageService = null) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<ModManagePanelViewModel>.Instance,
        Config = new Config { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() },
        GameConfig = new MuseDashConfig(),
        ModManageService = modManageService ?? IModManageService.Mock()
    };
}
