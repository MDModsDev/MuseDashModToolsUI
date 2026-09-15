namespace Euterpe.CodeAnalysis.Tests;

internal static class GeneratorTestHelper
{
    public static GeneratorDriverRunResult Run<TGenerator>(string source)
        where TGenerator : IIncrementalGenerator, new()
    {
        var compilation = CSharpCompilation.Create(
            "Tests",
            [CSharpSyntaxTree.ParseText(source)],
            Net100.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return CSharpGeneratorDriver.Create(new TGenerator().AsSourceGenerator()).RunGenerators(compilation).GetRunResult();
    }
}
