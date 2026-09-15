using System.Text;

namespace Euterpe.Tests.Core;

[Category("JsonSerializationServiceTests")]
[TestSubject(typeof(JsonSerializationService))]
public sealed partial class JsonSerializationServiceTest
{
    private const string ConfigJson = """
                                      {
                                          "SteamFolder": "C:\\Program Files (x86)\\SteamLibrary",
                                          "SteamExecPath": "C:\\Program Files (x86)\\SteamLibrary\\steam.exe",
                                          "CacheFolder": "Cache",
                                          "ActiveGame": "MuseDash2",
                                          "MuseDash": {
                                              "Folder": "C:\\Program Files (x86)\\SteamLibrary\\steamapps\\common\\Muse Dash",
                                              "GameMode": "Vanilla",
                                              "SetupCompleted": true
                                          },
                                          "MuseDash2": {
                                              "Folder": "D:\\Games\\Muse Dash 2",
                                              "GameMode": "Vanilla",
                                              "SetupCompleted": true
                                          },
                                          "LanguageCode": "en-US",
                                          "Theme": "Light",
                                          "ShowConsole": false,
                                          "AlwaysShowScrollBar": false,
                                          "MinimizeToTrayOnClose": true,
                                          "DownloadSource": "GitHub",
                                          "UpdateSource": "GitHubRSS",
                                          "GitHubToken": null,
                                          "UpdateChannel": "Beta",
                                          "IgnoreException": true
                                      }
                                      """;

    private readonly JsonSerializationService _jsonSerializationService = new();

    [Test]
    public Task DeserializeConfig_ValidJson_ReturnsMatchingConfig()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigJson));
        var config = _jsonSerializationService.DeserializeConfig(stream);

        return AssertDeserializedConfig(config);
    }

    [Test]
    public async Task DeserializeConfigAsync_ValidJson_ReturnsMatchingConfig()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigJson));
        var config = await _jsonSerializationService.DeserializeConfigAsync(stream).ConfigureAwait(false);

        await AssertDeserializedConfig(config).ConfigureAwait(false);
    }

    private static async Task AssertDeserializedConfig(Config? config)
    {
        await Assert.That(config).IsNotNull();

        var actual = config!;
        using var _ = Assert.Multiple();
        await Assert.That(actual.SteamFolder).IsEqualTo(@"C:\Program Files (x86)\SteamLibrary");
        await Assert.That(actual.SteamExecPath).IsEqualTo(@"C:\Program Files (x86)\SteamLibrary\steam.exe");
        await Assert.That(actual.CacheFolder).IsEqualTo("Cache");
        await Assert.That(actual.ActiveGame).IsEqualTo(GameId.MuseDash2);
        await Assert.That(actual.MuseDash.Folder).IsEqualTo(@"C:\Program Files (x86)\SteamLibrary\steamapps\common\Muse Dash");
        await Assert.That(actual.MuseDash.GameMode).IsEqualTo(GameMode.Vanilla);
        await Assert.That(actual.MuseDash.SetupCompleted).IsTrue();
        await Assert.That(actual.MuseDash2.Folder).IsEqualTo(@"D:\Games\Muse Dash 2");
        await Assert.That(actual.MuseDash2.GameMode).IsEqualTo(GameMode.Vanilla);
        await Assert.That(actual.MuseDash2.SetupCompleted).IsTrue();
        await Assert.That(actual.LanguageCode).IsEqualTo("en-US");
        await Assert.That(actual.Theme).IsEqualTo("Light");
        await Assert.That(actual.ShowConsole).IsFalse();
        await Assert.That(actual.ShowStartScreen).IsTrue();
        await Assert.That(actual.AlwaysShowScrollBar).IsFalse();
        await Assert.That(actual.MinimizeToTrayOnClose).IsTrue();
        await Assert.That(actual.UpdateChannel).IsEqualTo(UpdateChannel.Beta);
        await Assert.That(actual.IgnoreException).IsTrue();
    }
}
