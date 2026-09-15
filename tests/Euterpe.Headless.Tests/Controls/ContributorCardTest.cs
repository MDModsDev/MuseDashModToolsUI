using Euterpe.Controls.Models;

namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(ContributorCard))]
[Category("ContributorCardTests")]
public sealed class ContributorCardTest : HeadlessTest
{
    [Test]
    public Task AvatarUrl_ValueSet_BindsToAsyncImageSource() => RunOnUI(async () =>
    {
        var card = new ContributorCard { AvatarUrl = "not-a-uri" };
        var window = new Window { Content = card, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var avatar = card.GetVisualDescendants().OfType<AsyncImage>().FirstOrDefault();

        await Assert.That(avatar?.Source).IsEqualTo("not-a-uri");
    });

    [Test]
    public Task ContributorName_ValueSet_BindsToTemplateTextBlock() => RunOnUI(async () =>
    {
        var card = new ContributorCard { ContributorName = "lxymahatma" };
        var window = new Window { Content = card, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var nameTextBlock = card.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(tb => tb.Text is "lxymahatma");
        await Assert.That(nameTextBlock).IsNotNull();
    });

    [Test]
    public Task ContributorDescription_NullValue_HidesDescriptionTextBlock() => RunOnUI(async () =>
    {
        var card = new ContributorCard
        {
            ContributorName = "test",
            ContributorDescription = null
        };
        var window = new Window { Content = card, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var nameTextBlock = card.GetVisualDescendants()
            .OfType<TextBlock>()
            .First(tb => tb.Text is "test");
        var descTextBlock = card.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(tb => !ReferenceEquals(tb, nameTextBlock));

        using var _ = Assert.Multiple();
        await Assert.That(descTextBlock).IsNotNull();
        await Assert.That(descTextBlock!.IsVisible).IsFalse();
    });

    [Test]
    public Task ContributorDescription_ValueSet_ShowsDescriptionTextBlock() => RunOnUI(async () =>
    {
        var card = new ContributorCard
        {
            ContributorName = "test",
            ContributorDescription = "the maintainer"
        };
        var window = new Window { Content = card, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var descTextBlock = card.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(tb => tb.Text is "the maintainer");

        using var _ = Assert.Multiple();
        await Assert.That(descTextBlock).IsNotNull();
        await Assert.That(descTextBlock!.IsVisible).IsTrue();
    });

    [Test]
    public Task ButtonCommand_LinkButtonClicked_ExecutesWithLinkUrl() => RunOnUI(async () =>
    {
        var receivedUrls = new List<object?>();
        var card = new ContributorCard
        {
            ContributorName = "test",
            Links = [new ContributorLink("github", "https://github.com")],
            ButtonCommand = new DelegateCommand(p => receivedUrls.Add(p))
        };
        var window = new Window { Content = card, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var expander = card.GetVisualDescendants().OfType<Expander>().First();
        expander.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();

        var linkButton = card.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => b.CommandParameter is "https://github.com");

        using var _ = Assert.Multiple();
        await Assert.That(linkButton).IsNotNull();
        linkButton!.Command?.Execute(linkButton.CommandParameter);
        Dispatcher.UIThread.RunJobs();
        await Assert.That(receivedUrls).Contains("https://github.com");
    });
}
