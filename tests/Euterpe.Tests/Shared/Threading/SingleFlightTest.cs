using Euterpe.Shared.Threading;

namespace Euterpe.Tests.Shared.Threading;

[Category("SingleFlightTests")]
[TestSubject(typeof(SingleFlight<>))]
public sealed class SingleFlightTest
{
    [Test]
    public async Task RunAsync_ConcurrentCallsWithSameKey_ExecuteWorkOnce()
    {
        var flight = new SingleFlight<string>();
        var executions = 0;
        using var release = new SemaphoreSlim(0, 1);

        async Task Work()
        {
            Interlocked.Increment(ref executions);
            await release.WaitAsync().ConfigureAwait(false);
        }

        var t1 = flight.RunAsync("key", Work);
        var t2 = flight.RunAsync("key", Work);
        var t3 = flight.RunAsync("key", Work);

        release.Release();
        await Task.WhenAll(t1, t2, t3);

        await Assert.That(executions).IsEqualTo(1);
    }

    [Test]
    [Timeout(5_000)]
    public async Task RunAsync_DistinctKeys_ExecuteWorkInParallel(CancellationToken cancellationToken)
    {
        var flight = new SingleFlight<string>();
        var executions = 0;
        using var release = new SemaphoreSlim(0, 5);

        async Task Work()
        {
            Interlocked.Increment(ref executions);
            await release.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        var tasks = Enumerable.Range(0, 5)
            .Select(i => flight.RunAsync($"key-{i}", Work))
            .ToArray();

        while (Volatile.Read(ref executions) < 5)
        {
            await Task.Delay(10, cancellationToken);
        }

        release.Release(5);
        await Task.WhenAll(tasks);

        await Assert.That(executions).IsEqualTo(5);
    }

    [Test]
    [Timeout(5_000)]
    public async Task RunAsync_AfterPreviousCompletes_StartsFreshExecution(CancellationToken cancellationToken)
    {
        var flight = new SingleFlight<string>();
        var executions = 0;

        await flight.RunAsync("key", () =>
        {
            Interlocked.Increment(ref executions);
            return Task.CompletedTask;
        }).WaitAsync(cancellationToken);
        await flight.RunAsync("key", () =>
        {
            Interlocked.Increment(ref executions);
            return Task.CompletedTask;
        }).WaitAsync(cancellationToken);

        await Assert.That(executions).IsEqualTo(2);
    }

    [Test]
    public async Task RunAsync_TypedConcurrentCallsWithSameKey_ShareOneExecutionAndResult()
    {
        var flight = new SingleFlight<string>();
        var executions = 0;
        using var release = new SemaphoreSlim(0, 1);

        async Task<int> Work()
        {
            Interlocked.Increment(ref executions);
            await release.WaitAsync().ConfigureAwait(false);
            return 42;
        }

        var t1 = flight.RunAsync("key", Work);
        var t2 = flight.RunAsync("key", Work);

        release.Release();
        var results = await Task.WhenAll(t1, t2);

        using var _ = Assert.Multiple();
        await Assert.That(executions).IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(42);
        await Assert.That(results[1]).IsEqualTo(42);
    }

    [Test]
    [Timeout(5_000)]
    public async Task RunAsync_TypedAfterPreviousCompletes_StartsFreshExecution(CancellationToken cancellationToken)
    {
        var flight = new SingleFlight<string>();

        var first = await flight.RunAsync("key", static () => Task.FromResult(1)).WaitAsync(cancellationToken);
        var second = await flight.RunAsync("key", static () => Task.FromResult(2)).WaitAsync(cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(first).IsEqualTo(1);
        await Assert.That(second).IsEqualTo(2);
    }

    [Test]
    public async Task RunAsync_WhenWorkThrows_PropagatesAndReleasesSlot()
    {
        var flight = new SingleFlight<string>();

        var faulting = () => flight.RunAsync("key", () => Task.FromException(new InvalidOperationException("boom")));
        await Assert.That(faulting).Throws<InvalidOperationException>().WithMessage("boom");

        var ran = false;
        await flight.RunAsync("key", () =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        await Assert.That(ran).IsTrue();
    }

    [Test]
    [Repeat(20)]
    [Timeout(5_000)]
    public async Task RunAsync_ConcurrentCallsWithSameKeyBeforeCompletion_ExecutesOnce(CancellationToken cancellationToken)
    {
        const int callerCount = 64;
        var flight = new SingleFlight<int>();
        var executions = 0;
        using var release = new SemaphoreSlim(0, 1);

        async Task Work()
        {
            Interlocked.Increment(ref executions);
            await release.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        var tasks = Enumerable.Range(0, callerCount)
            .Select(_ => flight.RunAsync(42, Work))
            .ToArray();

        release.Release();
        await Task.WhenAll(tasks);

        await Assert.That(executions).IsEqualTo(1);
    }
}
