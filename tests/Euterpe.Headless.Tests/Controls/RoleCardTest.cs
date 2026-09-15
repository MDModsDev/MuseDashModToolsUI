namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(RoleCard))]
[Category("RoleCardTests")]
public sealed class RoleCardTest : HeadlessTest
{
    [Test]
    public Task IsSelected_NewControl_IsFalse() => RunOnUI(async () =>
    {
        var card = new RoleCard();
        await Assert.That(card.IsSelected).IsFalse();
    });

    [Test]
    public Task AccentColor_ValueSet_BindsToIconBadgeBackground() => RunOnUI(async () =>
    {
        var card = new RoleCard { AccentColor = new SolidColorBrush(Colors.Red) };
        var window = new Window { Content = card, Width = 300, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var iconBadge = card.GetVisualDescendants()
            .OfType<Border>()
            .First(b => b.Name is "IconBadgeBg");

        var brush = iconBadge.Background as ISolidColorBrush;
        await Assert.That(brush?.Color).IsEqualTo(Colors.Red);
    });

    [Test]
    public Task IsSelected_SetToTrue_SwitchesIconBadgeToWhite() => RunOnUI(async () =>
    {
        var card = new RoleCard { AccentColor = new SolidColorBrush(Colors.Red) };
        var window = new Window { Content = card, Width = 300, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var iconBadge = card.GetVisualDescendants()
            .OfType<Border>()
            .First(b => b.Name is "IconBadgeBg");

        card.IsSelected = true;
        Dispatcher.UIThread.RunJobs();

        var brush = iconBadge.Background as ISolidColorBrush;
        await Assert.That(brush?.Color).IsEqualTo(Colors.White);
    });
}
