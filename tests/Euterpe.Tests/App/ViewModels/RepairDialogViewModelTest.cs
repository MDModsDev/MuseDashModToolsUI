using Euterpe.Features.Setup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("RepairDialogViewModelTests")]
[TestSubject(typeof(RepairDialogViewModel))]
public sealed class RepairDialogViewModelTest
{
    [Test]
    public async Task ApplyCommand_SubscriberRegistered_RaisesRequestClose()
    {
        var vm = NewViewModel();
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ApplyCommand.Execute(null);

        await Assert.That(closed).IsTrue();
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
    public Task Close_WithNoSubscriber_DoesNotThrow()
    {
        var vm = NewViewModel();

        vm.Close();

        return Task.CompletedTask;
    }

    [Test]
    public async Task OpenFileCommand_FilePathProvided_DelegatesToLauncher()
    {
        var launcher = IPlatformLauncher.Mock();
        var vm = NewViewModel(launcher);

        await vm.OpenFileCommand.ExecuteAsync("/path/to/file");

        launcher.OpenFileAsync("/path/to/file").WasCalled(Times.Once);
    }

    [Test]
    public async Task OpenUrlCommand_UrlProvided_DelegatesToLauncher()
    {
        var launcher = IPlatformLauncher.Mock();
        var vm = NewViewModel(launcher);

        await vm.OpenUrlCommand.ExecuteAsync("https://example.com");

        launcher.OpenUriAsync("https://example.com").WasCalled(Times.Once);
    }

    [Test]
    public async Task PrepareForGamePathAsync_PreviousSetupFinished_ResetsStateAndPresentsGamePathPage()
    {
        var state = new SetupState();
        state.Steps.Add(new SetupStepState { Kinds = SetupOptionKinds.MelonLoader, DisplayName = "stale" });
        state.Stage = SetupExecutionStage.Finished;

        var gamePathPage = NewGamePathPage();
        var vm = NewFullViewModel(state: state, gamePathPage: gamePathPage);

        await vm.PrepareForGamePathAsync();

        using var _ = Assert.Multiple();
        await Assert.That(vm.Content).IsSameReferenceAs(gamePathPage);
        await Assert.That(state.Steps).IsEmpty();
        await Assert.That(state.Stage).IsEqualTo(SetupExecutionStage.NotStarted);
    }

    [Test]
    public async Task PrepareForOptionAsync_AllOptionsSelected_ResetsStateAndSelectsOnlyMatchingOption()
    {
        var state = new SetupState();
        var gameConfig = new MuseDashConfig();
        foreach (var option in gameConfig.SetupOptions)
        {
            option.IsSelected = true;
        }

        var executionPage = NewExecutionPage(gameConfig, state);
        var vm = NewFullViewModel(gameConfig, state, executionPage: executionPage);

        await vm.PrepareForOptionAsync(SetupOptionKinds.MelonLoader);

        using var _ = Assert.Multiple();
        await Assert.That(vm.Content).IsSameReferenceAs(executionPage);
        await Assert.That(gameConfig.SetupOptions.Single(o => o.IsSelected).Kinds).IsEqualTo(SetupOptionKinds.MelonLoader);
        await Assert.That(gameConfig.SetupOptions.Where(o => o.Kinds is not SetupOptionKinds.MelonLoader).All(o => !o.IsSelected)).IsTrue();
    }

    [Test]
    public async Task PrepareForOptionAsync_NoMatchingOption_AllOptionsDeselected()
    {
        var state = new SetupState();
        var gameConfig = new MuseDashConfig();
        gameConfig.SetupOptions[0].IsSelected = true;

        var executionPage = NewExecutionPage(gameConfig, state);
        var vm = NewFullViewModel(gameConfig, state, executionPage: executionPage);

        await vm.PrepareForOptionAsync(SetupOptionKinds.None);

        await Assert.That(gameConfig.SetupOptions.All(o => !o.IsSelected)).IsTrue();
    }

    private static GamePathPageViewModel NewGamePathPage() => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<GamePathPageViewModel>.Instance,
        State = null!,
        GameConfig = new MuseDashConfig(),
        GamePaths = IGamePathDiscovery.Mock(),
        FileSystemPickerService = null!
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

    private static RepairDialogViewModel NewFullViewModel(
        GameConfig? gameConfig = null,
        SetupState? state = null,
        GamePathPageViewModel? gamePathPage = null,
        ExecutionPageViewModel? executionPage = null)
    {
        var resolvedState = state ?? new SetupState();
        var resolvedConfig = gameConfig ?? new MuseDashConfig();
        return new RepairDialogViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            Logger = NullLogger<RepairDialogViewModel>.Instance,
            GameConfig = resolvedConfig,
            State = resolvedState,
            GamePathPage = gamePathPage ?? NewGamePathPage(),
            ExecutionPage = executionPage ?? NewExecutionPage(resolvedConfig, resolvedState)
        };
    }

    private static RepairDialogViewModel NewViewModel(IPlatformLauncher? launcher = null) => new()
    {
        Launcher = launcher ?? IPlatformLauncher.Mock(),
        Logger = NullLogger<RepairDialogViewModel>.Instance,
        GameConfig = null!,
        ExecutionPage = null!,
        GamePathPage = null!,
        State = null!
    };
}
