using Euterpe.Features.Wizard;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("RolePageViewModelTests")]
[TestSubject(typeof(RolePageViewModel))]
public sealed class RolePageViewModelTest
{
    [Test]
    public async Task Roles_DefaultOptions_ContainsAllFourIdentities()
    {
        var identities = RolePageViewModel.Roles.Select(r => r.Identity).ToList();

        using var assertions = Assert.Multiple();
        await Assert.That(identities).Contains(WizardIdentity.Player);
        await Assert.That(identities).Contains(WizardIdentity.Charter);
        await Assert.That(identities).Contains(WizardIdentity.Modder);
        await Assert.That(identities).Contains(WizardIdentity.Custom);
    }

    [Test]
    public async Task SelectedRole_OnlyRequiredOptionsSelected_DefaultsToPlayer()
    {
        // Default MuseDashConfig has the Required options selected, which matches Player preset.
        var vm = NewViewModel();

        await Assert.That(vm.SelectedRole.Identity).IsEqualTo(WizardIdentity.Player);
    }

    [Test]
    public async Task SetSelectedRole_ToModder_TogglesOptionsToMatchPreset()
    {
        var gameConfig = new MuseDashConfig();
        var vm = NewViewModel(gameConfig);
        var modderPreset = gameConfig.WizardPresets[WizardIdentity.Modder];

        vm.SelectedRole = RolePageViewModel.Roles.First(r => r.Identity is WizardIdentity.Modder);

        var actual = gameConfig.SetupOptions
            .Where(o => o.IsSelected)
            .Select(o => o.Kinds)
            .Aggregate(SetupOptionKinds.None, (acc, k) => acc | k);
        await Assert.That(actual).IsEqualTo(modderPreset);
    }

    [Test]
    public async Task SetSelectedRole_ToCustom_DoesNotChangeOptions()
    {
        var gameConfig = new MuseDashConfig();
        var vm = NewViewModel(gameConfig);
        var beforeMask = gameConfig.SetupOptions
            .Where(o => o.IsSelected)
            .Select(o => o.Kinds)
            .Aggregate(SetupOptionKinds.None, (acc, k) => acc | k);

        vm.SelectedRole = RolePageViewModel.Roles.First(r => r.Identity is WizardIdentity.Custom);

        var afterMask = gameConfig.SetupOptions
            .Where(o => o.IsSelected)
            .Select(o => o.Kinds)
            .Aggregate(SetupOptionKinds.None, (acc, k) => acc | k);
        await Assert.That(afterMask).IsEqualTo(beforeMask);
    }

    private static RolePageViewModel NewViewModel(GameConfig? gameConfig = null) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<RolePageViewModel>.Instance,
        State = null!,
        GameConfig = gameConfig ?? new MuseDashConfig()
    };
}
