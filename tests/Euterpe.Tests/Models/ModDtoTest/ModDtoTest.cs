namespace Euterpe.Tests.Models;

[Category("ModDtoTests")]
[TestSubject(typeof(ModDto))]
public sealed partial class ModDtoTest
{
    private static ModDto Create(string fileName = "MyMod.dll", string? localFnWithoutExt = null, bool disabled = true) =>
        new()
        {
            Name = "MyMod",
            FileName = fileName,
            FileNameWithoutExtension = localFnWithoutExt,
            IsDisabled = disabled,
            Version = "1.0.0"
        };

    [Test]
    public async Task AddLocalInfo_WebOnlyMod_PromotesToLocalAndCopiesVersion()
    {
        var mod = Create();

        mod.AddLocalInfo();

        using var _ = Assert.Multiple();
        await Assert.That(mod.LocalVersion).IsEqualTo("1.0.0");
        await Assert.That(mod.State).IsEqualTo(ModState.Normal);
        await Assert.That(mod.FileNameWithoutExtension).IsEqualTo("MyMod");
        await Assert.That(mod.IsDisabled).IsFalse();
        await Assert.That(mod.IsLocal).IsTrue();
    }

    [Test]
    public async Task RemoveLocalInfo_LocalMod_DemotesToWebOnly()
    {
        var mod = Create(localFnWithoutExt: "MyMod", disabled: false);
        mod.LocalVersion = "1.0.0";
        mod.State = ModState.Normal;

        mod.RemoveLocalInfo();

        using var _ = Assert.Multiple();
        await Assert.That(mod.LocalVersion).IsEmpty();
        await Assert.That(mod.State).IsEqualTo(ModState.Normal);
        await Assert.That(mod.FileNameWithoutExtension).IsNull();
        await Assert.That(mod.IsDisabled).IsTrue();
        await Assert.That(mod.IsLocal).IsFalse();
    }
}
