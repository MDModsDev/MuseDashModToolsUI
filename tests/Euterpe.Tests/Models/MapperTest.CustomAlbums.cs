using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Tests.Models;

public sealed partial class MapperTest
{
    [Test]
    public async Task ToManifestMeta_CoreFieldsPopulated_MapsCoreFields()
    {
        var info = new InfoJson
        {
            Name = "Song",
            NameRomanized = "Song Romanized",
            Author = "Composer",
            Scene = "scene_01"
        };

        var meta = info.ToManifestMeta(0.5f, []);

        await Assert.That(meta.Name).IsEqualTo("Song");
        await Assert.That(meta.NameRomanized).IsEqualTo("Song Romanized");
        await Assert.That(meta.Author).IsEqualTo("Composer");
        await Assert.That(meta.Scene).IsEqualTo("scene_01");
        await Assert.That(meta.BackgroundVideoOpacity).IsEqualTo(0.5f);
    }

    [Test]
    public async Task ToManifestMeta_BlankOptionalFields_PreservesEmptyValues()
    {
        var info = new InfoJson { Name = "Song", Author = "Composer", Scene = "scene_01" };

        var meta = info.ToManifestMeta(null, []);

        await Assert.That(meta.NameRomanized).IsEqualTo(string.Empty);
        await Assert.That(meta.HideMode).IsEqualTo(string.Empty);
        await Assert.That(meta.HideRatingOverride).IsEqualTo(string.Empty);
        await Assert.That(meta.HideMessage).IsEqualTo(string.Empty);
        await Assert.That(meta.SearchKeywords).IsEmpty();
        await Assert.That(meta.BackgroundVideoOpacity).IsNull();
    }

    [Test]
    [Arguments("120", 120, null, null)]
    [Arguments("120~140", 130, 120, 140)]
    [Arguments("", 0, null, null)]
    [Arguments("not-a-number", 0, null, null)]
    public async Task ToManifestMeta_BpmTextVariations_ParsesBpm(string bpm, int expected, int? expectedMin, int? expectedMax)
    {
        var info = new InfoJson { Name = "Song", Author = "Composer", Bpm = bpm, Scene = "scene_01" };

        var meta = info.ToManifestMeta(null, []);

        await Assert.That(meta.Bpm).IsEqualTo(expected);
        await Assert.That(meta.BpmMin).IsEqualTo(expectedMin);
        await Assert.That(meta.BpmMax).IsEqualTo(expectedMax);
    }

    [Test]
    public async Task ToManifestMeta_SelectedDifficulties_BuildsMapsWithDesignerFallback()
    {
        var info = new InfoJson
        {
            Name = "Song",
            Author = "Composer",
            Scene = "scene_01",
            Difficulty1 = "2",
            Difficulty2 = "5",
            Difficulty3 = "8",
            LevelDesigner = "General",
            LevelDesigner1 = "Alice",
            LevelDesigner3 = ""
        };

        var meta = info.ToManifestMeta(null, [ChartDifficulty.Easy, ChartDifficulty.Master]);

        await Assert.That(meta.Maps.Count).IsEqualTo(2);
        await Assert.That(meta.Maps.ContainsKey("map2")).IsFalse();
        await Assert.That(meta.Maps["map1"].Rating).IsEqualTo("2");
        await Assert.That(meta.Maps["map1"].Charters)
            .IsEquivalentTo(["Alice"], StringComparer.Ordinal, CollectionOrdering.Matching);
        await Assert.That(meta.Maps["map3"].Rating).IsEqualTo("8");
        await Assert.That(meta.Maps["map3"].Charters)
            .IsEquivalentTo(["General"], StringComparer.Ordinal, CollectionOrdering.Matching);
    }

    [Test]
    [Arguments("scene_1", "scene_01")]
    [Arguments("scene_01", "scene_01")]
    [Arguments("scene_12", "scene_12")]
    [Arguments("scene_99", "scene_99")]
    public async Task ToManifestMeta_SceneNumberVariations_NormalizesToTwoDigits(string scene, string expected)
    {
        var info = new InfoJson { Name = "Song", Author = "Composer", Scene = scene };

        var meta = info.ToManifestMeta(null, []);

        await Assert.That(meta.Scene).IsEqualTo(expected);
    }

    [Test]
    [Arguments("")]
    [Arguments("scene")]
    [Arguments("scene-a")]
    [Arguments("scene_")]
    [Arguments("scene_abc")]
    public async Task ToManifestMeta_InvalidScene_Throws(string scene)
    {
        var info = new InfoJson { Name = "Song", Author = "Composer", Scene = scene };
        Action act = () => info.ToManifestMeta(null, []);

        await Assert.That(act).ThrowsExactly<InvalidDataException>();
    }
}
