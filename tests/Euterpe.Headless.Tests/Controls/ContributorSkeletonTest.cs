namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(ContributorSkeleton))]
[Category("ContributorSkeletonTests")]
public sealed class ContributorSkeletonTest : HeadlessTest
{
    [Test]
    public Task ApplyTemplate_ContributorSkeleton_MatchesContributorCardSize() => RunOnUI(async () =>
    {
        var skeleton = new ContributorSkeleton();
        var card = new ContributorCard { ContributorName = "test", ContributorDescription = "the maintainer" };
        Show(skeleton, card);

        var placeholder = skeleton.GetVisualDescendants().OfType<ContentControl>().First();

        await Assert.That(placeholder.Bounds.Size).IsEqualTo(card.Bounds.Size);
    });

    [Test]
    public Task ApplyTemplate_ContributorSkeleton_FillsPlaceholdersWithThemeBrushes() => RunOnUI(async () =>
    {
        var skeleton = new ContributorSkeleton();
        Show(skeleton);

        var unpainted = skeleton.GetVisualDescendants().OfType<Border>().Where(static border => border.Background is null);

        await Assert.That(unpainted).IsEmpty();
    });

    private static void Show(params Control[] children)
    {
        var panel = new StackPanel();
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        var window = new Window { Content = panel, Width = 900, Height = 700 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
    }
}
