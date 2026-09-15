using System.Text;
using Euterpe.Core.Utils;

namespace Euterpe.Tests.Core.Utils;

[Category("SHA256UtilsTests")]
[TestSubject(typeof(SHA256Utils))]
public sealed class SHA256UtilsTest
{
    // Known SHA-256 hashes from https://en.wikipedia.org/wiki/SHA-2
    private const string EmptyHashUpper = "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855";
    private const string EmptyHashLower = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    private const string AbcHashUpper = "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD";
    private const string AbcHashLower = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

    private string _tempFile = null!;

    [Before(Test)]
    public void CreateTempFile() => _tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [After(Test)]
    public void DeleteTempFile()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }

    [Test]
    [Arguments("", EmptyHashUpper)]
    [Arguments("abc", AbcHashUpper)]
    public async Task HexFromBytes_KnownBytes_ReturnsExpectedUpperCaseHash(string input, string expected)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        await Assert.That(SHA256Utils.HexFromBytes(bytes)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashLower)]
    [Arguments("abc", AbcHashLower)]
    public async Task HexLowerFromBytes_KnownBytes_ReturnsExpectedLowerCaseHash(string input, string expected)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        await Assert.That(SHA256Utils.HexLowerFromBytes(bytes)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashUpper)]
    [Arguments("abc", AbcHashUpper)]
    public async Task HexFromPath_KnownFileContents_ReturnsExpectedUpperCaseHash(string content, string expected)
    {
        await File.WriteAllTextAsync(_tempFile, content);
        await Assert.That(SHA256Utils.HexFromPath(_tempFile)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashLower)]
    [Arguments("abc", AbcHashLower)]
    public async Task HexLowerFromPathAsync_KnownFileContents_ReturnsExpectedLowerCaseHash(string content, string expected)
    {
        await File.WriteAllTextAsync(_tempFile, content);
        await Assert.That(await SHA256Utils.HexLowerFromPathAsync(_tempFile)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashUpper)]
    [Arguments("abc", AbcHashUpper)]
    public async Task HexFromPathAsync_KnownFileContents_ReturnsExpectedUpperCaseHash(string content, string expected)
    {
        await File.WriteAllTextAsync(_tempFile, content);
        await Assert.That(await SHA256Utils.HexFromPathAsync(_tempFile)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashLower)]
    [Arguments("abc", AbcHashLower)]
    public async Task HexLowerFromPath_KnownFileContents_ReturnsExpectedLowerCaseHash(string content, string expected)
    {
        await File.WriteAllTextAsync(_tempFile, content);
        await Assert.That(SHA256Utils.HexLowerFromPath(_tempFile)).IsEqualTo(expected);
    }
}
