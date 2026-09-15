using Euterpe.Abstractions;
using Euterpe.Core;

namespace Euterpe.Headless.Tests.Services;

[TestSubject(typeof(DialogService))]
public sealed class DialogServiceTest : HeadlessTest
{
    [Test]
    public Task ShowDialogAsync_VisibleOwnerAndViewModelClosesWithTrue_ReturnsTrue() => RunOnUI(async () =>
    {
        var service = NewService();
        var owner = NewVisibleOwner();
        var vm = new TestDialogVm();

        var task = service.ShowWindowDialogAsync<TestDialogWindow, TestDialogVm, bool>(vm, owner);
        Dispatcher.UIThread.RunJobs();

        vm.Close(true);
        var result = await task;

        await Assert.That(result).IsTrue();
    });

    [Test]
    public Task ShowDialogAsync_VisibleOwnerAndViewModelClosesWithFalse_ReturnsFalse() => RunOnUI(async () =>
    {
        var service = NewService();
        var owner = NewVisibleOwner();
        var vm = new TestDialogVm();

        var task = service.ShowWindowDialogAsync<TestDialogWindow, TestDialogVm, bool>(vm, owner);
        Dispatcher.UIThread.RunJobs();

        vm.Close(false);
        var result = await task;

        await Assert.That(result).IsFalse();
    });

    [Test]
    public Task ShowDialogAsync_HiddenOwner_TakesNonModalPath() => RunOnUI(async () =>
    {
        var service = NewService();
        var hiddenOwner = new Window();
        var vm = new TestDialogVm();

        var task = service.ShowWindowDialogAsync<TestDialogWindow, TestDialogVm, bool>(vm, hiddenOwner);
        Dispatcher.UIThread.RunJobs();

        vm.Close(true);
        var result = await task;

        await Assert.That(result).IsTrue();
    });

    private static DialogService NewService() => new();

    private static Window NewVisibleOwner()
    {
        var owner = new Window();
        owner.Show();
        Dispatcher.UIThread.RunJobs();
        return owner;
    }

    private sealed class TestDialogWindow : Window;

    private sealed class TestDialogVm : IDialog<bool>
    {
        public event EventHandler<bool>? RequestClose;
        public void Close(bool result) => RequestClose?.Invoke(this, result);
    }
}
