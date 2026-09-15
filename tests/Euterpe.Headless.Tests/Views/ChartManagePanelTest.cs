using Avalonia.Controls.Primitives;
using Euterpe.Features.Charting;
using Euterpe.Models.Charts;
using TUnit.Assertions.Enums;

namespace Euterpe.Headless.Tests.Views;

[Category("ChartManagePanelTests")]
[TestSubject(typeof(ChartManagePanel))]
public sealed class ChartManagePanelTest : HeadlessTest
{
    [Test]
    public Task ApplyTemplate_ChartManagePanel_CreatesOneFilterPerDifficulty() => RunOnUI(async () =>
    {
        var difficulties = DifficultyToggles(Show())
            .Select(static toggle => toggle.GetVisualDescendants().OfType<DifficultyStar>().Single().Difficulty);

        await Assert.That(difficulties).IsEquivalentTo(
            ChartDifficultyExtensions.GetValues(),
            EqualityComparer<ChartDifficulty>.Default,
            CollectionOrdering.Matching);
    });

    [Test]
    public Task DifficultyFilter_Toggled_DimsTheStarWithoutResizingIt() => RunOnUI(async () =>
    {
        var toggle = DifficultyToggles(Show())[0];
        var star = toggle.GetVisualDescendants().OfType<DifficultyStar>().Single();
        var litSize = star.Bounds.Size;

        toggle.IsChecked = false;
        Dispatcher.UIThread.RunJobs();
        var dimmed = star.Opacity;
        var dimmedSize = star.Bounds.Size;

        toggle.IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(dimmed).IsLessThan(1);
        await Assert.That(dimmedSize).IsEqualTo(litSize);
        await Assert.That(star.Opacity).IsEqualTo(1);
    });

    private static List<ToggleButton> DifficultyToggles(ChartManagePanel view) =>
        view.GetVisualDescendants()
            .OfType<ToggleButton>()
            .Where(static toggle => toggle.Classes.Contains("DifficultyFilter"))
            .ToList();

    private static ChartManagePanel Show()
    {
        var view = new ChartManagePanel();
        var window = new Window { Content = view, Width = 1000, Height = 700 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return view;
    }
}
