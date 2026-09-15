using Euterpe.Core.Logger;
using Microsoft.Extensions.Logging;

namespace Euterpe.Tests.Core.Logger;

[Category("LogMessageTests")]
[TestSubject(typeof(LogMessage))]
public sealed class LogMessageTest
{
    [Test]
    [Arguments(LogLevel.Trace, "TRC")]
    [Arguments(LogLevel.Debug, "DBG")]
    [Arguments(LogLevel.Information, "INF")]
    [Arguments(LogLevel.Warning, "WRN")]
    [Arguments(LogLevel.Error, "ERR")]
    [Arguments(LogLevel.Critical, "CRT")]
    [Arguments(LogLevel.None, "NON")]
    public async Task LogLevelAbbreviation_KnownLogLevels_ReturnsMatchingAbbreviation(LogLevel level, string expected)
    {
        var msg = new LogMessage(DateTimeOffset.UtcNow, level, "cat", "m");
        await Assert.That(msg.LogLevelAbbreviation).IsEqualTo(expected);
    }
}
