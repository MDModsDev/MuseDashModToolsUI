using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Core;

[Category("LinuxPlatformInfoTests")]
[TestSubject(typeof(LinuxPlatformInfo))]
[RunOn(OS.Linux)]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
public sealed class LinuxPlatformInfoTest
{
    [Test]
    public async Task OsString_LinuxPlatformInfo_ReturnsLinux()
    {
        var info = new LinuxPlatformInfo();
        await Assert.That(info.OsString).IsEqualTo("linux");
    }
}
