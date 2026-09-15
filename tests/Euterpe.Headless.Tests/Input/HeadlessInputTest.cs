using Avalonia.Input;

namespace Euterpe.Headless.Tests.Input;

[Category("HeadlessInputTests")]
public sealed class HeadlessInputTest : HeadlessTest
{
    [Test]
    public Task MouseClick_OnButton_RaisesClickEvent() => RunOnUI(async () =>
    {
        var clicks = 0;
        var button = new Button { Content = "ok", Width = 100, Height = 40 };
        button.Click += (_, _) => clicks++;
        var window = new Window { Content = button, Width = 200, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var bounds = button.Bounds;
        var center = button.TranslatePoint(
            new Point(bounds.Width / 2, bounds.Height / 2), window) ?? default;

        window.MouseMove(center);
        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(clicks).IsEqualTo(1);
    });

    [Test]
    public Task KeyInput_IntoTextBox_UpdatesText() => RunOnUI(async () =>
    {
        var textBox = new TextBox { Width = 200, Height = 30 };
        var window = new Window { Content = textBox, Width = 300, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        window.KeyTextInput("hello");
        Dispatcher.UIThread.RunJobs();

        await Assert.That(textBox.Text).IsEqualTo("hello");
    });

    [Test]
    public Task KeyPress_Tab_MovesFocusToNextControl() => RunOnUI(async () =>
    {
        var first = new TextBox { Width = 100, Height = 30 };
        var second = new TextBox { Width = 100, Height = 30 };
        var stack = new StackPanel { Children = { first, second } };
        var window = new Window { Content = stack, Width = 200, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        first.Focus();
        Dispatcher.UIThread.RunJobs();

        window.KeyPress(Key.Tab, RawInputModifiers.None, PhysicalKey.Tab, string.Empty);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(second.IsFocused).IsTrue();
    });

    [Test]
    public Task KeyPress_Backspace_DeletesLastCharacterFromTextBox() => RunOnUI(async () =>
    {
        var textBox = new TextBox { Width = 200, Height = 30 };
        var window = new Window { Content = textBox, Width = 300, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        window.KeyTextInput("abc");
        Dispatcher.UIThread.RunJobs();

        window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, string.Empty);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(textBox.Text).IsEqualTo("ab");
    });

    [Test]
    public Task KeyPress_Enter_RaisesClickOnDefaultButton() => RunOnUI(async () =>
    {
        var clicks = 0;
        var textBox = new TextBox { Width = 100, Height = 30 };
        var button = new Button { Content = "ok", IsDefault = true };
        button.Click += (_, _) => clicks++;
        var stack = new StackPanel { Children = { textBox, button } };
        var window = new Window { Content = stack, Width = 200, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        window.KeyPress(Key.Return, RawInputModifiers.None, PhysicalKey.Enter, "\r");
        Dispatcher.UIThread.RunJobs();

        await Assert.That(clicks).IsEqualTo(1);
    });
}
