namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(LabeledSection))]
[Category("LabeledSectionTests")]
public sealed class LabeledSectionTest : HeadlessTest
{
    [Test]
    public Task ApplyTemplate_TitleAndDescriptionSet_BindsTemplateTextBlocks() => RunOnUI(async () =>
    {
        var section = new LabeledSection { Title = "Hello", Description = "World" };
        var window = new Window { Content = section, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var textBlocks = section.GetVisualDescendants().OfType<TextBlock>().ToList();
        using var _ = Assert.Multiple();
        await Assert.That(textBlocks).Count().IsEqualTo(2);
        await Assert.That(textBlocks[0].Text).IsEqualTo("Hello");
        await Assert.That(textBlocks[1].Text).IsEqualTo("World");
    });

    [Test]
    public Task Title_ChangedAfterTemplateApply_UpdatesTemplateTextBlock() => RunOnUI(async () =>
    {
        var section = new LabeledSection { Title = "Initial" };
        var window = new Window { Content = section, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        section.Title = "Updated";
        Dispatcher.UIThread.RunJobs();

        var titleTextBlock = section.GetVisualDescendants().OfType<TextBlock>().First();
        await Assert.That(titleTextBlock.Text).IsEqualTo("Updated");
    });
}
