using System.Collections;
using Euterpe.Shared.Collections;

namespace Euterpe.Tests.Shared.Collections;

[Category("BiDictionaryTests")]
[TestSubject(typeof(BiDictionary<,>))]
public sealed class BiDictionaryTest
{
    private BiDictionary<string, int> _dict = null!;

    [Before(Test)]
    public void Setup() => _dict = new BiDictionary<string, int>
    {
        { "one", 1 },
        { "two", 2 },
        { "three", 3 }
    };

    [Test]
    public async Task Indexer_LookupByKey_ReturnsValue()
    {
        using var _ = Assert.Multiple();
        await Assert.That(_dict["one"]).IsEqualTo(1);
        await Assert.That(_dict["two"]).IsEqualTo(2);
        await Assert.That(_dict["three"]).IsEqualTo(3);
    }

    [Test]
    public async Task Indexer_LookupByValue_ReturnsKey()
    {
        using var _ = Assert.Multiple();
        await Assert.That(_dict[1]).IsEqualTo("one");
        await Assert.That(_dict[2]).IsEqualTo("two");
        await Assert.That(_dict[3]).IsEqualTo("three");
    }

    [Test]
    public async Task Count_PopulatedDictionary_ReflectsNumberOfEntries() =>
        await Assert.That(_dict.Count).IsEqualTo(3);

    [Test]
    public async Task Add_DuplicateKey_Throws()
    {
        var act = () => _dict.Add("one", 100);
        await Assert.That(act).Throws<ArgumentException>().WithMessage("An element with the same key already exists.");
    }

    [Test]
    public async Task Add_DuplicateValue_Throws()
    {
        var act = () => _dict.Add("uno", 1);
        await Assert.That(act).Throws<ArgumentException>().WithMessage("An element with the same value already exists.");
    }

    [Test]
    [Arguments("one", true)]
    [Arguments("two", true)]
    [Arguments("missing", false)]
    public async Task ContainsKey_ExistingAndMissingKeys_ReportsKeyPresence(string key, bool expected) =>
        await Assert.That(_dict.ContainsKey(key)).IsEqualTo(expected);

    [Test]
    [Arguments(1, true)]
    [Arguments(2, true)]
    [Arguments(99, false)]
    public async Task ContainsValue_ExistingAndMissingValues_ReportsValuePresence(int value, bool expected) =>
        await Assert.That(_dict.ContainsValue(value)).IsEqualTo(expected);

    [Test]
    public async Task RemoveByKey_ExistingKey_RemovesBothDirections()
    {
        var removed = _dict.RemoveByKey("two");

        using var _ = Assert.Multiple();
        await Assert.That(removed).IsTrue();
        await Assert.That(_dict.ContainsKey("two")).IsFalse();
        await Assert.That(_dict.ContainsValue(2)).IsFalse();
        await Assert.That(_dict.Count).IsEqualTo(2);
    }

    [Test]
    public async Task RemoveByKey_MissingKey_ReturnsFalse()
    {
        var removed = _dict.RemoveByKey("missing");

        using var _ = Assert.Multiple();
        await Assert.That(removed).IsFalse();
        await Assert.That(_dict.Count).IsEqualTo(3);
    }

    [Test]
    public async Task RemoveByValue_ExistingValue_RemovesBothDirections()
    {
        var removed = _dict.RemoveByValue(3);

        using var _ = Assert.Multiple();
        await Assert.That(removed).IsTrue();
        await Assert.That(_dict.ContainsKey("three")).IsFalse();
        await Assert.That(_dict.ContainsValue(3)).IsFalse();
        await Assert.That(_dict.Count).IsEqualTo(2);
    }

    [Test]
    public async Task TryGetValue_ExistingKey_ReturnsTrueAndValue()
    {
        var found = _dict.TryGetValue("two", out var value);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsTrue();
        await Assert.That(value).IsEqualTo(2);
    }

    [Test]
    public async Task TryGetKey_ExistingValue_ReturnsTrueAndKey()
    {
        var found = _dict.TryGetKey(2, out var key);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsTrue();
        await Assert.That(key).IsEqualTo("two");
    }

    [Test]
    public async Task Clear_PopulatedDictionary_RemovesAllEntries()
    {
        _dict.Clear();

        using var _ = Assert.Multiple();
        await Assert.That(_dict.Count).IsEqualTo(0);
        await Assert.That(_dict.ContainsKey("one")).IsFalse();
        await Assert.That(_dict.ContainsValue(1)).IsFalse();
    }

    [Test]
    public async Task ToFrozenBiDictionary_PopulatedDictionary_PreservesAllMappings()
    {
        var frozen = _dict.ToFrozenBiDictionary();

        using var _ = Assert.Multiple();
        await Assert.That(frozen["one"]).IsEqualTo(1);
        await Assert.That(frozen[2]).IsEqualTo("two");
        await Assert.That(frozen.Count()).IsEqualTo(3);
    }

    [Test]
    public async Task GetEnumerator_PopulatedDictionary_YieldsForwardKeyValuePairs()
    {
        var pairs = _dict.ToArray();

        using var _ = Assert.Multiple();
        await Assert.That(pairs.Length).IsEqualTo(3);
        await Assert.That(pairs).Contains(new KeyValuePair<string, int>("one", 1), KeyValuePairComparer<string, int>.Default);
        await Assert.That(pairs).Contains(new KeyValuePair<string, int>("two", 2), KeyValuePairComparer<string, int>.Default);
        await Assert.That(pairs).Contains(new KeyValuePair<string, int>("three", 3), KeyValuePairComparer<string, int>.Default);
    }

    [Test]
    public async Task GetEnumerator_NonGenericEnumerable_YieldsForwardKeyValuePairs()
    {
        IEnumerable enumerable = _dict;
        var pairs = new List<KeyValuePair<string, int>>();
        foreach (var item in enumerable)
        {
            pairs.Add((KeyValuePair<string, int>)item);
        }

        using var _ = Assert.Multiple();
        await Assert.That(pairs.Count).IsEqualTo(3);
        await Assert.That(pairs).Contains(new KeyValuePair<string, int>("one", 1), KeyValuePairComparer<string, int>.Default);
    }

    [Test]
    public async Task IndexerSetByKey_ExistingKey_OverwritesValueAndReverseEntry()
    {
        _dict["one"] = 100;

        using var _ = Assert.Multiple();
        await Assert.That(_dict["one"]).IsEqualTo(100);
        await Assert.That(_dict[100]).IsEqualTo("one");
    }

    [Test]
    public async Task IndexerSetByValue_ExistingValue_OverwritesKeyAndForwardEntry()
    {
        _dict[1] = "uno";

        using var _ = Assert.Multiple();
        await Assert.That(_dict[1]).IsEqualTo("uno");
        await Assert.That(_dict["uno"]).IsEqualTo(1);
    }

    [Test]
    public async Task TryGetValue_MissingKey_ReturnsFalse()
    {
        var found = _dict.TryGetValue("missing", out var value);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsFalse();
        await Assert.That(value).IsEqualTo(default);
    }

    [Test]
    public async Task TryGetKey_MissingValue_ReturnsFalse()
    {
        var found = _dict.TryGetKey(99, out var key);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsFalse();
        await Assert.That(key).IsNull();
    }

    [Test]
    public async Task RemoveByValue_MissingValue_ReturnsFalse()
    {
        var removed = _dict.RemoveByValue(99);

        using var _ = Assert.Multiple();
        await Assert.That(removed).IsFalse();
        await Assert.That(_dict.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Create_CollectionExpression_BuildsBiDictionary()
    {
        BiDictionary<string, int> dict =
        [
            new KeyValuePair<string, int>("ten", 10),
            new KeyValuePair<string, int>("twenty", 20)
        ];

        using var _ = Assert.Multiple();
        await Assert.That(dict.Count).IsEqualTo(2);
        await Assert.That(dict["ten"]).IsEqualTo(10);
        await Assert.That(dict[20]).IsEqualTo("twenty");
    }

    [Test]
    public async Task FrozenBiDictionary_NonGenericEnumerator_YieldsKeyValuePairs()
    {
        var frozen = _dict.ToFrozenBiDictionary();
        IEnumerable enumerable = frozen;

        var count = 0;
        foreach (var _ in enumerable)
        {
            count++;
        }

        await Assert.That(count).IsEqualTo(3);
    }
}
