namespace Euterpe.Tests.Models;

[Category("SetupStepStateTests")]
[TestSubject(typeof(SetupStepState))]
public sealed class SetupStepStateTest
{
    private static SetupStepState NewStep(SetupStepStatus status = SetupStepStatus.Pending) =>
        new()
        {
            Kinds = SetupOptionKinds.MelonLoader,
            DisplayName = "step",
            Status = status
        };

    [Test]
    [Arguments(SetupStepStatus.Pending, false)]
    [Arguments(SetupStepStatus.Running, false)]
    [Arguments(SetupStepStatus.Succeeded, false)]
    [Arguments(SetupStepStatus.Failed, true)]
    public async Task CanRetry_StatusVariations_ReturnsTrueOnlyWhenFailed(SetupStepStatus status, bool expected) =>
        await Assert.That(NewStep(status).CanRetry).IsEqualTo(expected);

    [Test]
    public async Task StatusDisplay_AllStatuses_ReturnsDistinctText()
    {
        var pending = NewStep().StatusDisplay.ToString();
        var running = NewStep(SetupStepStatus.Running).StatusDisplay.ToString();
        var succeeded = NewStep(SetupStepStatus.Succeeded).StatusDisplay.ToString();
        var failed = NewStep(SetupStepStatus.Failed).StatusDisplay.ToString();

        var distinct = new[] { pending, running, succeeded, failed }.Distinct().Count();
        await Assert.That(distinct).IsEqualTo(4);
    }

    [Test]
    public async Task Status_ValueChanged_RaisesPropertyChangedForDependents()
    {
        var step = NewStep();
        var changed = new List<string?>();
        step.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        step.Status = SetupStepStatus.Failed;

        using var _ = Assert.Multiple();
        await Assert.That(changed).Contains(nameof(SetupStepState.Status));
        await Assert.That(changed).Contains(nameof(SetupStepState.CanRetry));
        await Assert.That(changed).Contains(nameof(SetupStepState.StatusDisplay));
    }

    [Test]
    public async Task ErrorMessageAndMessage_DefaultNull_AndAreObservable()
    {
        var step = NewStep();
        var changed = new List<string?>();
        step.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        step.ErrorMessage = "boom";
        step.Message = "ok";

        using var _ = Assert.Multiple();
        await Assert.That(step.ErrorMessage).IsEqualTo("boom");
        await Assert.That(step.Message).IsEqualTo("ok");
        await Assert.That(changed).Contains(nameof(SetupStepState.ErrorMessage));
        await Assert.That(changed).Contains(nameof(SetupStepState.Message));
    }
}
