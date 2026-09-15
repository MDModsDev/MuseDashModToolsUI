using Euterpe.Shared.Extensions;

namespace Euterpe.Tests.Shared.Extensions;

[Category("StringExtensionsTests")]
[TestSubject(typeof(StringExtensions))]
public sealed class StringExtensionsTest
{
    [Test]
    [Arguments(null, true)]
    [Arguments("", true)]
    [Arguments(" ", false)]
    [Arguments("a", false)]
    [Arguments("hello", false)]
    public async Task IsNullOrEmpty_NullEmptyAndNonEmptyInputs_ReportsWhetherNullOrEmpty(string? input, bool expected) =>
        await Assert.That(input.IsNullOrEmpty()).IsEqualTo(expected);

    [Test]
    [Arguments(null, null)]
    [Arguments("", null)]
    [Arguments("   ", null)]
    [Arguments("a", "a")]
    [Arguments("hello", "hello")]
    public async Task NullIfWhiteSpace_BlankAndNonBlankInputs_ReturnsNullOnlyWhenBlank(string? input, string? expected) =>
        await Assert.That(input.NullIfWhiteSpace()).IsEqualTo(expected);

    [Test]
    [Arguments(null, "fallback", "fallback")]
    [Arguments("", "fallback", "fallback")]
    [Arguments("   ", "fallback", "fallback")]
    [Arguments("value", "fallback", "value")]
    public async Task DefaultIfWhiteSpace_BlankAndNonBlankInputs_ReturnsFallbackOnlyWhenBlank(string? input, string fallback, string expected) =>
        await Assert.That(input.DefaultIfWhiteSpace(fallback)).IsEqualTo(expected);

    [Test]
    [Arguments(@"C:\\Program Files\\App", @"C:\Program Files\App")]
    [Arguments(@"no\\slashes\\here", @"no\slashes\here")]
    [Arguments("nothing-to-replace", "nothing-to-replace")]
    [Arguments("", "")]
    public async Task NormalizeSlashes_EscapedAndPlainPaths_ReplacesDoubleBackslashesWithSingle(string input, string expected) =>
        await Assert.That(input.NormalizeSlashes()).IsEqualTo(expected);

    [Test]
    [Arguments("0", 0)]
    [Arguments("1", 1)]
    [Arguments("42", 42)]
    [Arguments("-7", -7)]
    [Arguments("not-a-number", 0)]
    [Arguments("", 0)]
    [Arguments("3.14", 0)]
    public async Task ParseLevel_IntegerAndInvalidText_ReturnsParsedValueOrZero(string input, int expected) =>
        await Assert.That(input.ParseLevel()).IsEqualTo(expected);

    [Test]
    [Arguments("valid_name.txt", "valid_name.txt")]
    [Arguments("simple", "simple")]
    [Arguments("", "")]
    public async Task RemoveInvalidFileNameChars_NoInvalidChars_ReturnsSameString(string input, string expected) =>
        await Assert.That(input.RemoveInvalidFileNameChars()).IsEqualTo(expected);

    [Test]
    [Arguments("test\0name", "test_name")]
    [Arguments("a/b", "a_b")]
    [Arguments("\0/\0", "___")]
    public async Task RemoveInvalidFileNameChars_WithInvalidChars_ReplacesWithUnderscore(string input, string expected) =>
        await Assert.That(input.RemoveInvalidFileNameChars()).IsEqualTo(expected);

    [Test]
    [Arguments("plain text", "plain text")]
    [Arguments("", "")]
    [Arguments("nothing-special", "nothing-special")]
    public async Task EscapeDesktopExecArgument_NoSpecialChars_ReturnsSameString(string input, string expected) =>
        await Assert.That(input.EscapeDesktopExecArgument()).IsEqualTo(expected);

    [Test]
    [Arguments("say \"hi\"", "say \\\"hi\\\"")]
    [Arguments("$VAR", "\\$VAR")]
    [Arguments("back`tick", "back\\`tick")]
    [Arguments("c:\\path", "c:\\\\path")]
    public async Task EscapeDesktopExecArgument_WithSpecialChars_EscapesEachWithBackslash(string input, string expected) =>
        await Assert.That(input.EscapeDesktopExecArgument()).IsEqualTo(expected);
}
