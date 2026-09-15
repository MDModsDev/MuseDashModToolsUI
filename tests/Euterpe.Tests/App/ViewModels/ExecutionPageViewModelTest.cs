using Euterpe.Features.Setup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("ExecutionPageViewModelTests")]
[TestSubject(typeof(ExecutionPageViewModel))]
public sealed class ExecutionPageViewModelTest
{
    [Test]
    public async Task CanGoBack_NewViewModel_IsFalse() =>
        await Assert.That(NewViewModel(new MuseDashConfig(), []).CanGoBack).IsFalse();

    [Test]
    public async Task OnEnterAsync_SelectedSetupOptions_PopulatesSteps()
    {
        var gameConfig = new MuseDashConfig();
        SelectOnlyFirst(gameConfig);
        var firstKinds = gameConfig.SetupOptions[0].Kinds;
        var vm = NewViewModel(gameConfig, [SuccessfulStep(firstKinds)]);

        await vm.OnEnterAsync();

        using var _ = Assert.Multiple();
        await Assert.That(vm.State.Steps).Count().IsEqualTo(1);
        await Assert.That(vm.State.Steps[0].Kinds).IsEqualTo(firstKinds);
    }

    [Test]
    public async Task OnEnterAsync_AllStepsSucceed_MarksAllSucceededAndStageFinished()
    {
        var gameConfig = new MuseDashConfig();
        SelectOnlyFirst(gameConfig);
        var firstKinds = gameConfig.SetupOptions[0].Kinds;
        var vm = NewViewModel(gameConfig, [SuccessfulStep(firstKinds)]);

        await vm.OnEnterAsync();

        using var _ = Assert.Multiple();
        await Assert.That(vm.State.Steps[0].Status).IsEqualTo(SetupStepStatus.Succeeded);
        await Assert.That(vm.State.Stage).IsEqualTo(SetupExecutionStage.Finished);
        await Assert.That(vm.State.AllSucceeded).IsTrue();
        await Assert.That(vm.Progress).IsEqualTo(100d);
    }

    [Test]
    public async Task OnEnterAsync_StepThrows_StepMarkedFailedWithErrorMessage()
    {
        var gameConfig = new MuseDashConfig();
        SelectOnlyFirst(gameConfig);
        var firstKinds = gameConfig.SetupOptions[0].Kinds;
        var vm = NewViewModel(gameConfig, [FailingStep(firstKinds)]);

        await vm.OnEnterAsync();

        using var _ = Assert.Multiple();
        await Assert.That(vm.State.Steps[0].Status).IsEqualTo(SetupStepStatus.Failed);
        await Assert.That(vm.State.Steps[0].ErrorMessage).IsNotNull();
        await Assert.That(vm.State.Stage).IsEqualTo(SetupExecutionStage.Finished);
        await Assert.That(vm.State.AllSucceeded).IsFalse();
    }

    [Test]
    public async Task OnEnterAsync_PartialFailure_OnlyFailingStepMarkedFailed()
    {
        var gameConfig = new MuseDashConfig();
        foreach (var o in gameConfig.SetupOptions)
        {
            o.IsSelected = false;
        }

        gameConfig.SetupOptions[0].IsSelected = true;
        gameConfig.SetupOptions[1].IsSelected = true;
        var first = gameConfig.SetupOptions[0].Kinds;
        var second = gameConfig.SetupOptions[1].Kinds;

        var vm = NewViewModel(gameConfig, [SuccessfulStep(first), FailingStep(second)]);

        await vm.OnEnterAsync();

        using var _ = Assert.Multiple();
        await Assert.That(vm.State.Steps[0].Status).IsEqualTo(SetupStepStatus.Succeeded);
        await Assert.That(vm.State.Steps[1].Status).IsEqualTo(SetupStepStatus.Failed);
    }

    [Test]
    public async Task RetryCommand_RunsSpecifiedStep_AndUpdatesStatus()
    {
        var gameConfig = new MuseDashConfig();
        SelectOnlyFirst(gameConfig);
        var firstKinds = gameConfig.SetupOptions[0].Kinds;
        var vm = NewViewModel(gameConfig, [SuccessfulStep(firstKinds)]);

        var failedStep = new SetupStepState
        {
            Kinds = firstKinds,
            DisplayName = "test",
            Status = SetupStepStatus.Failed,
            ErrorMessage = "previous failure"
        };
        vm.State.Steps.Add(failedStep);
        vm.State.Stage = SetupExecutionStage.Finished;

        await vm.RetryCommand.ExecuteAsync(failedStep);

        using var _ = Assert.Multiple();
        await Assert.That(failedStep.Status).IsEqualTo(SetupStepStatus.Succeeded);
        await Assert.That(failedStep.ErrorMessage).IsNull();
        await Assert.That(vm.State.Stage).IsEqualTo(SetupExecutionStage.Finished);
    }

    private static void SelectOnlyFirst(GameConfig gameConfig)
    {
        foreach (var o in gameConfig.SetupOptions)
        {
            o.IsSelected = false;
        }

        gameConfig.SetupOptions[0].IsSelected = true;
    }

    private static ISetupStep SuccessfulStep(SetupOptionKinds kinds)
    {
        var step = ISetupStep.Mock();
        step.Kinds.Returns(kinds);
        return step;
    }

    private static ISetupStep FailingStep(SetupOptionKinds kinds)
    {
        var step = ISetupStep.Mock();
        step.Kinds.Returns(kinds);
        step.ExecuteAsync(Any<IProgress<string>>(), Any<CancellationToken>())
            .Throws<InvalidOperationException>();
        return step;
    }

    private static ExecutionPageViewModel NewViewModel(GameConfig gameConfig, IEnumerable<ISetupStep> steps) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<ExecutionPageViewModel>.Instance,
        State = new SetupState(),
        GameConfig = gameConfig,
        SetupSteps = steps,
        GameSettingService = IGameSettingService.Mock()
    };
}
