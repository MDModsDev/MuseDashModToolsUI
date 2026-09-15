# Test Rules

All C# rules apply to test code too; the rules below are test-specific.

## Placement

- Plain unit test → `tests/Euterpe.Tests`, at the mirrored path of the type under test (`App/` = `src/Euterpe`, plus `Core/`, `Models/`, `Shared/`, `Contracts/`).
- Release automation test → `tests/Euterpe.Releaser.Tests`.
- Anything needing live Avalonia UI (control, view, binding, input, theme) → `tests/Euterpe.Headless.Tests`; inherit `HeadlessTest` and touch UI only inside `RunOnUI`.
- Source generator or analyzer test → `tests/Euterpe.CodeAnalysis.Tests`; generator outputs are snapshot-verified with Meziantou.Framework.SnapshotTesting.Roslyn (`Generators/__snapshots__/`).

## Conventions

- TUnit: `[Test]` methods named `Method_Scenario_Expectation`, assertions via `await Assert.That(...)`; `Snapshot.Validate(...)` for snapshot assertions. Review `.actual.*` files against the existing `.verified.*` files before accepting snapshot changes.
- TUnit releases fast and its API surface is large (`[Arguments]`, `[MethodDataSource]`, `[ClassDataSource]`, hooks, ...) — don't write TUnit patterns from memory: fetch the docs (https://tunit.dev) to confirm the current idiom for the situation and check it against the version pinned in `Directory.Packages.props`.
- One `sealed class <Type>Test` per type under test, annotated `[TestSubject(typeof(T))]` and a `[Category]`.
- A large test class becomes `partial`, split per tested method into `<Type>Test.<Method>.cs` in a folder named after the type (mirrors the `Name.Topic.cs` source convention).
- Mocks via TUnit.Mocks: `IFoo.Mock()` for interfaces, `Mock.Logger<T>()` for loggers, TUnit.Mocks.Http for HTTP. Build the subject with a `Create<Service>()` helper whose optional parameters default to mocks.
- Layer invariants (visibility, sealed, namespaces) are enforced by `Euterpe.CodeAnalysis/Analyzers/ArchitectureAnalyzer.cs` — extend it when adding a project or layer rule.

## Running

- Test projects are Microsoft.Testing.Platform executables: `dotnet run --project tests/Euterpe.Tests`; filter with `--treenode-filter "/*/*/<ClassName>/*"`, not `--filter`.
