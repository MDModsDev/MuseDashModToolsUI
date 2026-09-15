using Euterpe.Models.VDFs;

namespace Euterpe.Tests.Core;

public sealed partial class VdfSerializationServiceTest
{
    [Test]
    public async Task SerializeToStream_RoundTrip_PreservesAllFields()
    {
        var original = new LibraryFolder
        {
            Path = "C:/SteamLibrary",
            Label = "Main",
            ContentId = "12345",
            TotalSize = "999",
            Apps = new Dictionary<string, string>
            {
                ["774171"] = "1024",
                ["1228610"] = "2048"
            }
        };
        await using var stream = new MemoryStream();

        _service.SerializeToStream(stream, original, "library");
        stream.Position = 0;
        var result = _service.DeserializeFromStream<LibraryFolder>(stream);

        using var _ = Assert.Multiple();
        await Assert.That(result.Path).IsEqualTo(original.Path);
        await Assert.That(result.Label).IsEqualTo(original.Label);
        await Assert.That(result.ContentId).IsEqualTo(original.ContentId);
        await Assert.That(result.TotalSize).IsEqualTo(original.TotalSize);
        await Assert.That(result.Apps).IsEquivalentTo(original.Apps, KeyValuePairComparer<string, string>.Default);
    }

    [Test]
    public async Task SerializeToFile_DeserializedFromFile_PreservesAllFields()
    {
        var original = new LibraryFolder
        {
            Path = "/home/user/SteamLibrary",
            Label = "Linux",
            ContentId = "67890",
            TotalSize = "12345",
            Apps = new Dictionary<string, string>
            {
                ["774171"] = "1024"
            }
        };

        var tempFile = Path.Combine(Path.GetTempPath(), $"vdf_{Guid.NewGuid():N}.vdf");
        try
        {
            _service.SerializeToFile(tempFile, original, "library");
            var roundTripped = _service.DeserializeFromFile<LibraryFolder>(tempFile);

            using var _ = Assert.Multiple();
            await Assert.That(roundTripped.Path).IsEqualTo(original.Path);
            await Assert.That(roundTripped.Label).IsEqualTo(original.Label);
            await Assert.That(roundTripped.ContentId).IsEqualTo(original.ContentId);
            await Assert.That(roundTripped.TotalSize).IsEqualTo(original.TotalSize);
            await Assert.That(roundTripped.Apps).IsEquivalentTo(original.Apps, KeyValuePairComparer<string, string>.Default);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
