using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("EssentialModsStepTests")]
[TestSubject(typeof(EssentialModsStep))]
public sealed class EssentialModsStepTest
{
    private readonly MockLogger<EssentialModsStep> _logger = Mock.Logger<EssentialModsStep>();

    private EssentialModsStep CreateStep(IModManageService modManageService) =>
        new()
        {
            ModManageService = modManageService,
            Logger = _logger
        };

    [Test]
    public async Task Kinds_EssentialModsStep_ReturnsEssentialMods()
    {
        var step = CreateStep(IModManageService.Mock());

        await Assert.That(step.Kinds).IsEqualTo(SetupOptionKinds.EssentialMods);
    }

    [Test]
    public async Task ExecuteAsync_ModServiceAvailable_InitializesMods()
    {
        var modManageService = IModManageService.Mock();
        var step = CreateStep(modManageService);

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        modManageService.InitializeModsAsync().WasCalled(Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ProgressProvided_ReportsInitializationMessage()
    {
        var modManageService = IModManageService.Mock();
        var step = CreateStep(modManageService);
        var reports = new List<string>();
        var reported = new TaskCompletionSource();
        var progress = new Progress<string>(s =>
        {
            reports.Add(s);
            reported.TrySetResult();
        });

        await step.ExecuteAsync(progress);
        await reported.Task.WaitAsync(TimeSpan.FromSeconds(2));

        await Assert.That(reports).Contains("Initializing essential mods ...");
    }
}
