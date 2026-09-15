using Avalonia.Data;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Euterpe.Headless.Tests.Integration;

[Category("ViewModelBindingTests")]
public sealed partial class ViewModelBindingTest : HeadlessTest
{
    [Test]
    public Task MouseClick_ExecutesBoundCommand_AndIncrementsCounter() => RunOnUI(async () =>
    {
        var vm = new CounterViewModel();
        var button = new Button { Content = "go", Width = 100, Height = 40 };
        button.Bind(Button.CommandProperty,
            CompiledBinding.Create((CounterViewModel viewModel) => viewModel.IncrementCommand));

        var window = new Window { DataContext = vm, Content = button, Width = 200, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var center = button.TranslatePoint(new Point(50, 20), window) ?? default;
        window.MouseMove(center);
        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(vm.Count).IsEqualTo(1);
    });

    [Test]
    public Task Bind_ViewModelPropertyChanges_UpdatesTextBlock() => RunOnUI(async () =>
    {
        var vm = new CounterViewModel { Title = "initial" };
        var textBlock = new TextBlock();
        textBlock.Bind(TextBlock.TextProperty,
            CompiledBinding.Create((CounterViewModel viewModel) => viewModel.Title));

        var window = new Window { DataContext = vm, Content = textBlock, Width = 200, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(textBlock.Text).IsEqualTo("initial");

        vm.Title = "updated";
        Dispatcher.UIThread.RunJobs();
        await Assert.That(textBlock.Text).IsEqualTo("updated");
    });

    [Test]
    public Task TextBoxInput_TwoWayBinds_BackToViewModel() => RunOnUI(async () =>
    {
        var vm = new CounterViewModel();
        var textBox = new TextBox { Width = 200, Height = 30 };
        textBox.Bind(TextBox.TextProperty,
            CompiledBinding.Create(
                (CounterViewModel viewModel) => viewModel.Title,
                mode: BindingMode.TwoWay));

        var window = new Window { DataContext = vm, Content = textBox, Width = 300, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        window.KeyTextInput("hello");
        Dispatcher.UIThread.RunJobs();

        await Assert.That(vm.Title).IsEqualTo("hello");
    });

    private sealed partial class CounterViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;

        public int Count { get; private set; }

        [RelayCommand]
        private void Increment() => Count++;
    }
}
