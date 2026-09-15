using Euterpe.Shell;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("CrashWindowViewModelTests")]
[TestSubject(typeof(CrashWindowViewModel))]
public sealed class CrashWindowViewModelTest
{
    [Test]
    public async Task Close_True_RaisesRequestCloseWithTrue()
    {
        var vm = NewViewModel();
        bool? captured = null;
        vm.RequestClose += (_, value) => captured = value;

        vm.Close(true);

        await Assert.That(captured).IsTrue();
    }

    [Test]
    public async Task Close_False_RaisesRequestCloseWithFalse()
    {
        var vm = NewViewModel();
        bool? captured = null;
        vm.RequestClose += (_, value) => captured = value;

        vm.Close(false);

        await Assert.That(captured).IsFalse();
    }

    [Test]
    public async Task ContinueCommand_SubscriberRegistered_RaisesRequestCloseWithTrue()
    {
        var vm = NewViewModel();
        bool? captured = null;
        vm.RequestClose += (_, value) => captured = value;

        vm.ContinueCommand.Execute(null);

        await Assert.That(captured).IsTrue();
    }

    [Test]
    public async Task ExitCommand_SubscriberRegistered_RaisesRequestCloseWithFalse()
    {
        var vm = NewViewModel();
        bool? captured = null;
        vm.RequestClose += (_, value) => captured = value;

        vm.ExitCommand.Execute(null);

        await Assert.That(captured).IsFalse();
    }

    [Test]
    public async Task SetException_InvalidOperationException_PopulatesCrashDetails()
    {
        var vm = NewViewModel();
        var ex = new InvalidOperationException("test crash message");

        vm.SetException(ex);

        using var _ = Assert.Multiple();
        await Assert.That(vm.ExceptionType).IsEqualTo(typeof(InvalidOperationException).FullName);
        await Assert.That(vm.ExceptionMessage).IsEqualTo("test crash message");
        await Assert.That(vm.ExceptionDetails).IsEqualTo(ex.ToString());
        await Assert.That(vm.CrashTime).IsNotEmpty();
    }

    private static CrashWindowViewModel NewViewModel() => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<CrashWindowViewModel>.Instance,
        MessageBoxService = IMessageBoxService.Mock(),
        TopLevel = null!
    };
}
