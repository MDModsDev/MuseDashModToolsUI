using Euterpe.Core.Extensions;

namespace Euterpe.Tests.Core.Extensions;

[Category("ByteExtensionsTests")]
[TestSubject(typeof(ByteExtensions))]
public sealed class ByteExtensionsTest
{
    public static IEnumerable<Func<(byte[] bytes, string upperHex, string lowerHex)>> HexCases()
    {
        yield return () => ([], "", "");
        yield return () => ([0x00], "00", "00");
        yield return () => ([0xFF], "FF", "ff");
        yield return () => ([0xDE, 0xAD, 0xBE, 0xEF], "DEADBEEF", "deadbeef");
        yield return () => ([0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF], "0123456789ABCDEF", "0123456789abcdef");
    }

    [Test]
    [MethodDataSource(nameof(HexCases))]
    public async Task ToHexString_ByteSequences_ReturnsUpperHex((byte[] bytes, string upperHex, string lowerHex) data) =>
        await Assert.That(data.bytes.ToHexString()).IsEqualTo(data.upperHex);

    [Test]
    [MethodDataSource(nameof(HexCases))]
    public async Task ToHexStringLower_ByteSequences_ReturnsLowerHex((byte[] bytes, string upperHex, string lowerHex) data) =>
        await Assert.That(data.bytes.ToHexStringLower()).IsEqualTo(data.lowerHex);

    public static IEnumerable<Func<(byte[] bytes, string base64Url)>> Base64UrlCases()
    {
        yield return () => ([], "");
        yield return () => ([0x00], "AA");
        yield return () => ([0xFF], "_w");
        yield return () => ([0xFB, 0xF0], "-_A");
        yield return () => ([0xDE, 0xAD, 0xBE, 0xEF], "3q2-7w");
        yield return () => ([0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF], "ASNFZ4mrze8");
    }

    [Test]
    [MethodDataSource(nameof(Base64UrlCases))]
    public async Task ToBase64Url_ByteSequences_ReturnsUrlSafeUnpaddedBase64((byte[] bytes, string base64Url) data) =>
        await Assert.That(data.bytes.ToBase64Url()).IsEqualTo(data.base64Url);
}
