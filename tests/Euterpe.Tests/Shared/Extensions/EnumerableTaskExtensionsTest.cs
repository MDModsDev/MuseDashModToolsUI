using Euterpe.Shared.Extensions;

namespace Euterpe.Tests.Shared.Extensions;

[Category("EnumerableTaskExtensionsTests")]
[TestSubject(typeof(EnumerableTaskExtensions))]
public sealed class EnumerableTaskExtensionsTest
{
    [Test]
    public async Task WhenAllAsync_EmptyArray_ReturnsEmptyArray()
    {
        var input = Array.Empty<int>();

        var result = await input.WhenAllAsync(static i => Task.FromResult(i.ToString()));

        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task WhenAllAsync_AppliesSelectorToEveryElement_PreservingOrder()
    {
        var input = new[] { 1, 2, 3, 4, 5 };

        var result = await input.WhenAllAsync(static i => Task.FromResult($"item-{i}"));

        await Assert.That(result).IsEquivalentTo(["item-1", "item-2", "item-3", "item-4", "item-5"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task WhenAllAsync_SelectorReturnsNull_PreservesNullResults()
    {
        var input = new[] { 1, 2, 3 };

        var result = await input.WhenAllAsync(static i => Task.FromResult(i % 2 is 0 ? null : i.ToString()));

        using var _ = Assert.Multiple();
        await Assert.That(result.Length).IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo("1");
        await Assert.That(result[1]).IsNull();
        await Assert.That(result[2]).IsEqualTo("3");
    }

    [Test]
    public async Task WhenAllAsync_SelectorThrows_PropagatesException()
    {
        var input = new[] { 1, 2, 3 };

        Func<Task> act = () => input.WhenAllAsync(static i =>
            i is 2 ? Task.FromException<string>(new InvalidOperationException("boom")) : Task.FromResult(i.ToString()));

        await Assert.That(act).Throws<InvalidOperationException>().WithMessage("boom");
    }

    [Test]
    public async Task WhenAllAsync_LazyEnumerable_ProjectsAllItems()
    {
        var input = Enumerable.Range(1, 3);

        var result = await input.WhenAllAsync(static i => Task.FromResult(i * 2));

        await Assert.That(result).IsEquivalentTo([2, 4, 6], EqualityComparer<int>.Default, CollectionOrdering.Matching);
    }
}
