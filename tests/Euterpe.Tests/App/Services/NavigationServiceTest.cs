using Euterpe.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.Services;

[Category("NavigationServiceTests")]
[TestSubject(typeof(NavigationService))]
public sealed class NavigationServiceTest
{
    private const string RouteA = "/__nav_test_a__";
    private const string RouteB = "/__nav_test_b__";

    private static NavigationService NewService() => new()
    {
        Logger = NullLogger<NavigationService>.Instance
    };

    [Test]
    public async Task Constructor_DefaultState_HasNoCurrentRouteAndIsNotReady()
    {
        var service = NewService();

        using var _ = Assert.Multiple();
        await Assert.That(service.CurrentRoute).IsNull();
        await Assert.That(service.Ready.IsSet).IsFalse();
    }

    [Test]
    public async Task NavigateTo_NewRoute_UpdatesCurrentRoute()
    {
        var service = NewService();

        service.NavigateTo(RouteA);

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteA);
    }

    [Test]
    public async Task NavigateTo_SameRoute_DoesNothing()
    {
        var service = NewService();
        service.NavigateTo(RouteA);

        service.NavigateTo(RouteA);

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteA);
    }

    [Test]
    public async Task NavigateTo_DifferentRoute_OverwritesCurrentRoute()
    {
        var service = NewService();
        service.NavigateTo(RouteA);

        service.NavigateTo(RouteB);

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteB);
    }

    [Test]
    public async Task NavigateToAsync_NotReady_WaitsUntilReadyThenNavigates()
    {
        var service = NewService();
        var task = service.NavigateToAsync(RouteA);

        await Assert.That(task.IsCompleted).IsFalse();

        service.Ready.Set();
        await task;

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteA);
    }

    [Test]
    public async Task NavigateToAsync_ReadyAlreadySet_NavigatesImmediately()
    {
        var service = NewService();
        service.Ready.Set();

        await service.NavigateToAsync(RouteB);

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteB);
    }
}
