using Euterpe.Features.Modding;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("ModDevelopPanelViewModelTests")]
[TestSubject(typeof(ModDevelopPanelViewModel))]
public sealed class ModDevelopPanelViewModelTest
{
    [Test]
    [Arguments(true, true, true)]
    [Arguments(false, false, false)]
    [Arguments(true, false, true)]
    public async Task InitializeAsync_InstallationStatusCombinations_PopulatesFlagsFromServices(bool sdk, bool template, bool envSet)
    {
        var sdkInstaller = IDotNetSdkInstaller.Mock();
        sdkInstaller.CheckInstalledAsync().Returns(sdk);
        var modTemplate = IGameModTemplateInstaller.Mock();
        modTemplate.CheckInstalledAsync().Returns(template);
        var pathEnv = IGamePathEnvironment.Mock();
        pathEnv.IsSet().Returns(envSet);

        var vm = NewViewModel(sdkInstaller, modTemplate, pathEnv);
        await vm.InitializeAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(vm.DotNetSdkInstalled).IsEqualTo(sdk);
        await Assert.That(vm.ModTemplateInstalled).IsEqualTo(template);
        await Assert.That(vm.EnvVariableSet).IsEqualTo(envSet);
    }

    [Test]
    public async Task ToggleModTemplateInstallCommand_SdkNotInstalled_IsDisabled()
    {
        var sdkInstaller = IDotNetSdkInstaller.Mock();
        sdkInstaller.CheckInstalledAsync().Returns(false);
        var vm = NewViewModel(sdkInstaller);
        await vm.InitializeAsync();

        await Assert.That(vm.ToggleModTemplateInstallCommand.CanExecute(null)).IsFalse();
    }

    [Test]
    public async Task ToggleModTemplateInstallCommand_SdkInstalled_IsEnabled()
    {
        var sdkInstaller = IDotNetSdkInstaller.Mock();
        sdkInstaller.CheckInstalledAsync().Returns(true);
        var vm = NewViewModel(sdkInstaller);
        await vm.InitializeAsync();

        await Assert.That(vm.ToggleModTemplateInstallCommand.CanExecute(null)).IsTrue();
    }

    private static ModDevelopPanelViewModel NewViewModel(
        IDotNetSdkInstaller? sdkInstaller = null,
        IGameModTemplateInstaller? modTemplate = null,
        IGamePathEnvironment? pathEnv = null) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<ModDevelopPanelViewModel>.Instance,
        GameConfig = new MuseDashConfig(),
        MessageBoxService = IMessageBoxService.Mock(),
        SdkInstaller = sdkInstaller ?? IDotNetSdkInstaller.Mock(),
        ModTemplateInstaller = modTemplate ?? IGameModTemplateInstaller.Mock(),
        PathEnvironment = pathEnv ?? IGamePathEnvironment.Mock()
    };
}
