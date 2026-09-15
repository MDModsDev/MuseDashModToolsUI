using System.Text.Json.Nodes;

namespace Euterpe.Tests.Core;

public sealed partial class JsonSerializationServiceTest
{
    private const string SerializedConfigJson = """
                                                {
                                                    "SteamFolder": "C:\\Program Files (x86)\\SteamLibrary",
                                                    "SteamExecPath": "C:\\Program Files (x86)\\SteamLibrary\\steam.exe",
                                                    "CacheFolder": "Cache",
                                                    "ActiveGame": "MuseDash",
                                                    "MuseDash": {
                                                        "Folder": "C:\\Program Files (x86)\\SteamLibrary\\steamapps\\common\\Muse Dash",
                                                        "GameMode": "Vanilla",
                                                        "SetupCompleted": false
                                                    },
                                                    "MuseDash2": {
                                                        "Folder": "",
                                                        "GameMode": "Modded",
                                                        "SetupCompleted": false
                                                    },
                                                    "LanguageCode": "zh-Hans",
                                                    "Theme": "Dark",
                                                    "ShowConsole": true,
                                                    "ShowStartScreen": true,
                                                    "AlwaysShowScrollBar": true,
                                                    "MinimizeToTrayOnClose": true,
                                                    "UpdateChannel": "Stable",
                                                    "IgnoreException": false
                                                }
                                                """;

    private static Config CreateTestConfig() => new()
    {
        SteamFolder = @"C:\Program Files (x86)\SteamLibrary",
        SteamExecPath = @"C:\Program Files (x86)\SteamLibrary\steam.exe",
        CacheFolder = "Cache",
        MuseDash = new MuseDashConfig
        {
            Folder = @"C:\Program Files (x86)\SteamLibrary\steamapps\common\Muse Dash",
            GameMode = GameMode.Vanilla,
            GameVersion = "1.0.0",
            MelonLoaderVersion = "0.6.5"
        },
        MuseDash2 = new MuseDash2Config(),
        LanguageCode = "zh-Hans",
        Theme = "Dark",
        ShowConsole = true,
        AlwaysShowScrollBar = true,
        MinimizeToTrayOnClose = true,
        UpdateChannel = UpdateChannel.Stable,
        IgnoreException = false
    };

    [Test]
    public Task SerializeConfig_PopulatedConfig_WritesValidJson()
    {
        var stream = new MemoryStream();
        _jsonSerializationService.SerializeConfig(stream, CreateTestConfig());
        return AssertSerializedConfig(stream);
    }

    [Test]
    public async Task SerializeConfigAsync_PopulatedConfig_WritesValidJson()
    {
        var stream = new MemoryStream();
        await _jsonSerializationService.SerializeConfigAsync(stream, CreateTestConfig()).ConfigureAwait(false);
        await AssertSerializedConfig(stream).ConfigureAwait(false);
    }

    private static async Task AssertSerializedConfig(Stream stream)
    {
        stream.Position = 0;
        var actual = await JsonNode.ParseAsync(stream).ConfigureAwait(false);
        var expected = JsonNode.Parse(SerializedConfigJson);

        await Assert.That(JsonNode.DeepEquals(actual, expected)).IsTrue();
    }
}
