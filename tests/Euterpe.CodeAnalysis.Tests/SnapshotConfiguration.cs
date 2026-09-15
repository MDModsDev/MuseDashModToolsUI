using System.Runtime.CompilerServices;
using Meziantou.Framework.SnapshotTesting.Roslyn;

namespace Euterpe.CodeAnalysis.Tests;

public static class SnapshotConfiguration
{
    [ModuleInitializer]
    public static void Initialize() => SnapshotSettings.Default.AddRoslyn();
}
