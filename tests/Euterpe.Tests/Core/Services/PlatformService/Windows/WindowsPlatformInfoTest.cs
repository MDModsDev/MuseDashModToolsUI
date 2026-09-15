using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Core;

[Category("WindowsPlatformInfoTests")]
[TestSubject(typeof(WindowsPlatformInfo))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsPlatformInfoTest
{
    [Test]
    public async Task OsString_WindowsPlatformInfo_ReturnsWin()
    {
        var info = new WindowsPlatformInfo();
        await Assert.That(info.OsString).IsEqualTo("win");
    }
}
