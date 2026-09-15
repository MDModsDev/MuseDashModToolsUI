using System.Text;
using Euterpe.Core.Utils;

namespace Euterpe.Tests.Core.Utils;

[Category("SHA512UtilsTests")]
[TestSubject(typeof(SHA512Utils))]
public sealed class SHA512UtilsTest
{
    // Known SHA-512 hashes
    private const string EmptyHashUpper =
        "CF83E1357EEFB8BDF1542850D66D8007D620E4050B5715DC83F4A921D36CE9CE47D0D13C5D85F2B0FF8318D2877EEC2F63B931BD47417A81A538327AF927DA3E";

    private const string EmptyHashLower =
        "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e";

    private const string AbcHashUpper =
        "DDAF35A193617ABACC417349AE20413112E6FA4E89A97EA20A9EEEE64B55D39A2192992A274FC1A836BA3C23A3FEEBBD454D4423643CE80E2A9AC94FA54CA49F";

    private const string AbcHashLower =
        "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f";

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
        await Assert.That(SHA512Utils.HexFromBytes(bytes)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashLower)]
    [Arguments("abc", AbcHashLower)]
    public async Task HexLowerFromBytes_KnownBytes_ReturnsExpectedLowerCaseHash(string input, string expected)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        await Assert.That(SHA512Utils.HexLowerFromBytes(bytes)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashUpper)]
    [Arguments("abc", AbcHashUpper)]
    public async Task HexFromPathAsync_KnownFileContents_ReturnsExpectedUpperCaseHash(string content, string expected)
    {
        await File.WriteAllTextAsync(_tempFile, content);
        await Assert.That(await SHA512Utils.HexFromPathAsync(_tempFile)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashLower)]
    [Arguments("abc", AbcHashLower)]
    public async Task HexLowerFromPath_KnownFileContents_ReturnsExpectedLowerCaseHash(string content, string expected)
    {
        await File.WriteAllTextAsync(_tempFile, content);
        await Assert.That(SHA512Utils.HexLowerFromPath(_tempFile)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashUpper)]
    [Arguments("abc", AbcHashUpper)]
    public async Task HexFromPath_KnownFileContents_ReturnsExpectedUpperCaseHash(string content, string expected)
    {
        await File.WriteAllTextAsync(_tempFile, content);
        await Assert.That(SHA512Utils.HexFromPath(_tempFile)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", EmptyHashLower)]
    [Arguments("abc", AbcHashLower)]
    public async Task HexLowerFromPathAsync_KnownFileContents_ReturnsExpectedLowerCaseHash(string content, string expected)
    {
        await File.WriteAllTextAsync(_tempFile, content);
        await Assert.That(await SHA512Utils.HexLowerFromPathAsync(_tempFile)).IsEqualTo(expected);
    }
}
