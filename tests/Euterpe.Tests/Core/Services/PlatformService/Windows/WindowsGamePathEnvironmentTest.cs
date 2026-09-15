using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("WindowsGamePathEnvironmentTests")]
[TestSubject(typeof(WindowsGamePathEnvironment))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsGamePathEnvironmentTest
{
    private const string TestFolder = @"C:\Test\MuseDash";
    private readonly MockLogger<WindowsGamePathEnvironment> _logger = Mock.Logger<WindowsGamePathEnvironment>();
    private string _envName = null!;

    [Before(Test)]
    public void GenerateUniqueEnvName() => _envName = $"EUTERPE_TEST_{Guid.NewGuid():N}";

    [After(Test)]
    public void ClearEnv() => Environment.SetEnvironmentVariable(_envName, null, EnvironmentVariableTarget.User);

    private WindowsGamePathEnvironment CreateService(IMessageBoxService? messageBox = null)
    {
        var config = GameConfig.Mock();
        config.PathEnvironmentVariableName.Returns(_envName);
        config.Object.Folder = TestFolder;
        return new WindowsGamePathEnvironment
        {
            GameConfig = config,
            Logger = _logger,
            MessageBoxService = messageBox ?? IMessageBoxService.Mock()
        };
    }

    [Test]
    [Arguments(null, false)]
    [Arguments(TestFolder, true)]
    [Arguments(@"C:\different\path", false)]
    public async Task IsSet_EnvironmentVariableValues_MatchesConfiguredGameFolder(string? envValue, bool expected)
    {
        var service = CreateService();
        Environment.SetEnvironmentVariable(_envName, envValue);
        await Assert.That(service.IsSet()).IsEqualTo(expected);
    }

    [Test]
    public async Task Set_GameFolderConfigured_WritesUserEnvironmentVariableAndReturnsTrue()
    {
        var service = CreateService();
        var result = service.Set();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsTrue();
        await Assert.That(Environment.GetEnvironmentVariable(_envName, EnvironmentVariableTarget.User)).IsEqualTo(TestFolder);
        _logger.VerifyLog().ContainingMessage($"Set {_envName}");
    }
}
