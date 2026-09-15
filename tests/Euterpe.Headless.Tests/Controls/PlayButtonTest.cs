namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(PlayButton))]
[Category("PlayButtonTests")]
public sealed class PlayButtonTest : HeadlessTest
{
    [Test]
    public Task InnerPlayButton_InheritsCommand_ViaTemplateBinding() => RunOnUI(async () =>
    {
        var executed = 0;
        var playButton = new PlayButton
        {
            Content = "PLAY",
            Command = new DelegateCommand(() => executed++)
        };
        var window = new Window { Content = playButton, Width = 300, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var innerButton = playButton.GetVisualDescendants()
            .OfType<Button>()
            .First(b => b.Classes.Contains("Play"));

        innerButton.Command?.Execute(null);
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(innerButton.Command).IsSameReferenceAs(playButton.Command);
        await Assert.That(executed).IsEqualTo(1);
    });

    [Test]
    public Task Content_ValueSet_BindsToInnerPlayButton() => RunOnUI(async () =>
    {
        var playButton = new PlayButton { Content = "PLAY NOW" };
        var window = new Window { Content = playButton, Width = 300, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var innerButton = playButton.GetVisualDescendants()
            .OfType<Button>()
            .First(b => b.Classes.Contains("Play"));

        await Assert.That(innerButton.Content).IsEqualTo("PLAY NOW");
    });
}
