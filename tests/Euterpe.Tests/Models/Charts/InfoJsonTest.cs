using System.Text.Json;
using Euterpe.Core.JsonContexts;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Tests.Models.Charts;

[Category("InfoJsonTests")]
[TestSubject(typeof(InfoJson))]
public sealed class InfoJsonTest
{
    [Test]
    public async Task Constructor_DefaultValues_InitializesEmptyStringsAndTags()
    {
        var info = new InfoJson();

        using var _ = Assert.Multiple();
        await Assert.That(info.Author).IsEqualTo(string.Empty);
        await Assert.That(info.Bpm).IsEqualTo(string.Empty);
        await Assert.That(info.Name).IsEqualTo(string.Empty);
        await Assert.That(info.SearchTags).IsEmpty();
    }

    [Test]
    public async Task JsonRoundTrip_PopulatedInfo_PreservesValues()
    {
        var original = new InfoJson
        {
            Author = "tester",
            Bpm = "120",
            Difficulty1 = "1",
            Difficulty2 = "2",
            Difficulty3 = "3",
            Difficulty4 = "4",
            HideBmsMessage = "m",
            HideBmsMode = "mode",
            LevelDesigner = "ld",
            LevelDesigner1 = "ld1",
            LevelDesigner2 = "ld2",
            LevelDesigner3 = "ld3",
            LevelDesigner4 = "ld4",
            Name = "song",
            NameRomanized = "song-romanized",
            Scene = "scene",
            SearchTags = ["tag1", "tag2"],
            UnlockLevel = "5"
        };

        var json = JsonSerializer.Serialize(original, CamelCaseContext.Default.InfoJson);
        var deserialized = JsonSerializer.Deserialize(json, CamelCaseContext.Default.InfoJson)!;

        using var _ = Assert.Multiple();
        await Assert.That(deserialized.Author).IsEqualTo(original.Author);
        await Assert.That(deserialized.Bpm).IsEqualTo(original.Bpm);
        await Assert.That(deserialized.Name).IsEqualTo(original.Name);
        await Assert.That(deserialized.NameRomanized).IsEqualTo(original.NameRomanized);
        await Assert.That(deserialized.SearchTags).IsEquivalentTo(original.SearchTags, EqualityComparer<string>.Default, CollectionOrdering.Matching);
        await Assert.That(deserialized.UnlockLevel).IsEqualTo(original.UnlockLevel);
    }
}
