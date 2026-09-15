using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("ChartingToolStepTests")]
[TestSubject(typeof(ChartingToolStep))]
public sealed class ChartingToolStepTest
{
    private readonly MockLogger<ChartingToolStep> _logger = Mock.Logger<ChartingToolStep>();

    private ChartingToolStep CreateStep() => new() { Logger = _logger };

    [Test]
    public async Task Kinds_ChartingToolStep_ReturnsChartingTool()
    {
        var step = CreateStep();

        await Assert.That(step.Kinds).IsEqualTo(SetupOptionKinds.ChartingTool);
    }

    [Test]
    public async Task ExecuteAsync_ProgressProvided_CompletesWithoutThrowing()
    {
        var step = CreateStep();

        var act = async () => await step.ExecuteAsync(new Progress<string>(_ => { }));
        await Assert.That(act).ThrowsNothing();
    }

    [Test]
    public async Task ExecuteAsync_ProgressProvided_DoesNotReportProgress()
    {
        var step = CreateStep();
        var reports = new List<string>();
        var progress = new Progress<string>(s => reports.Add(s));

        await step.ExecuteAsync(progress);

        await Task.Yield();
        await Assert.That(reports).IsEmpty();
    }
}
