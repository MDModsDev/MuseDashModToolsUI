using Euterpe.Features.Setup;
using Euterpe.Features.Wizard;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("WizardDialogViewModelTests")]
[TestSubject(typeof(WizardDialogViewModel))]
public sealed class WizardDialogViewModelTest
{
    [Test]
    public async Task BackCommand_CurrentPageAfterFirst_DecrementsCurrentPageIndex()
    {
        var vm = NewViewModel();
        vm.CurrentPageIndex = 2;

        vm.BackCommand.Execute(null);

        await Assert.That(vm.CurrentPageIndex).IsEqualTo(1);
    }

    [Test]
    public async Task Close_SubscriberRegistered_RaisesRequestClose()
    {
        var vm = NewViewModel();
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.Close();

        await Assert.That(closed).IsTrue();
    }

    [Test]
    public async Task CurrentPageIndex_ValueChanged_RaisesPropertyChangedForDerivedProperties()
    {
        var vm = NewViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        vm.CurrentPageIndex = 1;

        using var _ = Assert.Multiple();
        await Assert.That(changed).Contains(nameof(WizardDialogViewModel.CurrentPageIndex));
        await Assert.That(changed).Contains(nameof(WizardDialogViewModel.CurrentPage));
        await Assert.That(changed).Contains(nameof(WizardDialogViewModel.CanGoBack));
        await Assert.That(changed).Contains(nameof(WizardDialogViewModel.IsLastPage));
    }

    [Test]
    public async Task PrepareForFullSetupAsync_PreviousSetupFinished_PopulatesPagesAndResetsState()
    {
        var state = new SetupState();
        state.Steps.Add(new SetupStepState { Kinds = SetupOptionKinds.MelonLoader, DisplayName = "stale" });
        state.Stage = SetupExecutionStage.Finished;

        var gameConfig = new MuseDashConfig();
        var gamePathPage = NewGamePathPage(gameConfig);
        var rolePage = NewRolePage(gameConfig);
        var executionPage = NewExecutionPage(gameConfig, state);
        var vm = NewFullViewModel(gameConfig, state, gamePathPage, rolePage, executionPage);

        await vm.PrepareForFullSetupAsync();

        using var _ = Assert.Multiple();
        await Assert.That(state.Steps).IsEmpty();
        await Assert.That(state.Stage).IsEqualTo(SetupExecutionStage.NotStarted);
        await Assert.That(vm.Pages).Count().IsEqualTo(3);
        await Assert.That(vm.Pages[0]).IsSameReferenceAs(gamePathPage);
        await Assert.That(vm.Pages[1]).IsSameReferenceAs(rolePage);
        await Assert.That(vm.Pages[2]).IsSameReferenceAs(executionPage);
        await Assert.That(vm.CurrentPageIndex).IsEqualTo(0);
        await Assert.That(vm.CurrentPage).IsSameReferenceAs(gamePathPage);
    }

    [Test]
    public async Task NextCommand_NotLastPage_AdvancesIndex()
    {
        var state = new SetupState();
        var gameConfig = new MuseDashConfig();
        var vm = NewFullViewModel(gameConfig, state,
            NewGamePathPage(gameConfig), NewRolePage(gameConfig), NewExecutionPage(gameConfig, state));
        await vm.PrepareForFullSetupAsync();

        await vm.NextCommand.ExecuteAsync(null);

        using var _ = Assert.Multiple();
        await Assert.That(vm.CurrentPageIndex).IsEqualTo(1);
        await Assert.That(gameConfig.SetupCompleted).IsFalse();
    }

    [Test]
    public async Task NextCommand_LastPage_MarksSetupCompletedAndCloses()
    {
        var state = new SetupState();
        var gameConfig = new MuseDashConfig();
        var vm = NewFullViewModel(gameConfig, state,
            NewGamePathPage(gameConfig), NewRolePage(gameConfig), NewExecutionPage(gameConfig, state));
        await vm.PrepareForFullSetupAsync();
        vm.CurrentPageIndex = 2;
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        await vm.NextCommand.ExecuteAsync(null);

        using var _ = Assert.Multiple();
        await Assert.That(gameConfig.SetupCompleted).IsTrue();
        await Assert.That(closed).IsTrue();
    }

    private static GamePathPageViewModel NewGamePathPage(GameConfig gameConfig) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<GamePathPageViewModel>.Instance,
        State = null!,
        GameConfig = gameConfig,
        GamePaths = IGamePathDiscovery.Mock(),
        FileSystemPickerService = null!
    };

    private static RolePageViewModel NewRolePage(GameConfig gameConfig) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<RolePageViewModel>.Instance,
        State = null!,
        GameConfig = gameConfig
    };

    private static ExecutionPageViewModel NewExecutionPage(GameConfig gameConfig, SetupState state) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<ExecutionPageViewModel>.Instance,
        GameConfig = gameConfig,
        SetupSteps = [],
        State = state,
        GameSettingService = IGameSettingService.Mock()
    };

    private static WizardDialogViewModel NewFullViewModel(
        GameConfig gameConfig,
        SetupState state,
        GamePathPageViewModel gamePathPage,
        RolePageViewModel rolePage,
        ExecutionPageViewModel executionPage) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<WizardDialogViewModel>.Instance,
        GameConfig = gameConfig,
        ExecutionPage = executionPage,
        GamePathPage = gamePathPage,
        RolePage = rolePage,
        State = state
    };

    private static WizardDialogViewModel NewViewModel() => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<WizardDialogViewModel>.Instance,
        GameConfig = null!,
        ExecutionPage = null!,
        GamePathPage = null!,
        RolePage = null!,
        State = null!
    };
}
