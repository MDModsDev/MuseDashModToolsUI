namespace Euterpe.Tests.Models;

[Category("ConfigTests")]
[TestSubject(typeof(Config))]
public sealed class ConfigTest
{
    [Test]
    public async Task MinimizeToTrayOnClose_Default_IsFalse() =>
        await Assert.That(new Config().MinimizeToTrayOnClose).IsFalse();

    [Test]
    public async Task UpdateChannel_NewConfig_IsStable() =>
        await Assert.That(new Config { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() }.UpdateChannel).IsEqualTo(UpdateChannel.Stable);
}
