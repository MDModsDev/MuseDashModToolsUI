using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("LinuxGamePathEnvironmentTests")]
[TestSubject(typeof(LinuxGamePathEnvironment))]
[RunOn(OS.Linux)]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
public sealed class LinuxGamePathEnvironmentTest
{
    private const string TestFolder = "/tmp/test-musedash-folder";
    private readonly MockLogger<LinuxGamePathEnvironment> _logger = Mock.Logger<LinuxGamePathEnvironment>();
    private string _envName = null!;

    [Before(Test)]
    public void GenerateUniqueEnvName() => _envName = $"EUTERPE_TEST_{Guid.NewGuid():N}";

    [After(Test)]
    public void ClearEnv() => Environment.SetEnvironmentVariable(_envName, null);

    private LinuxGamePathEnvironment CreateService(IMessageBoxService? messageBox = null)
    {
        var config = GameConfig.Mock();
        config.PathEnvironmentVariableName.Returns(_envName);
        config.Object.Folder = TestFolder;
        return new LinuxGamePathEnvironment
        {
            GameConfig = config,
            Logger = _logger,
            MessageBoxService = messageBox ?? IMessageBoxService.Mock()
        };
    }

    [Test]
    [Arguments(null, false)]
    [Arguments(TestFolder, true)]
    [Arguments("/some/other/path", false)]
    public async Task IsSet_EnvironmentVariableValues_MatchesConfiguredGameFolder(string? envValue, bool expected)
    {
        var service = CreateService();
        Environment.SetEnvironmentVariable(_envName, envValue);
        await Assert.That(service.IsSet()).IsEqualTo(expected);
    }

    [Test]
    public async Task Set_GameFolderConfigured_LogsInstructionsAndReturnsTrue()
    {
        var service = CreateService();
        var result = service.Set();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsTrue();
        _logger.VerifyLog().ContainingMessage($"Ask user to set {_envName}");
    }
}
