# Euterpe

Avalonia desktop app. Projects under `src/`:

- `Euterpe` — the app: one `Features/<Name>/` folder per feature, holding only views, viewmodels and view-specific code (converters etc.); `Shell/` for app-level windows.
- `Euterpe.Abstractions` — service interfaces.
- `Euterpe.Contracts` — wire contracts: any request/response DTO serialized to or from an external API belongs here.
- `Euterpe.Controls` — reusable Avalonia controls and the app theme. `Themes/EuterpeTheme.axaml` is generated at build time — edit the per-control/per-style files, never the generated file.
- `Euterpe.Core` — service implementations under `Services/`.
- `Euterpe.CodeAnalysis` — Roslyn incremental source generators and analyzers; generators are driven by the marker attributes in `Euterpe.Shared`.
- `Euterpe.Localization` — localized `.resx` string tables: `XAML` for view text, `Interaction` for code-side messages.
- `Euterpe.Models` — app-internal models, DTOs, enums and records (types that cross the network go in `Euterpe.Contracts` instead).
- `Euterpe.Shared` — dependency-free utilities referenced across projects: attributes, collections, extensions, threading helpers, constants.
- `Euterpe.Tasks` — custom MSBuild build tasks (e.g. generating `EuterpeTheme.axaml`).
Tests under `tests/` (TUnit):

- `Euterpe.CodeAnalysis.Tests` — source generator and analyzer tests; generator outputs are snapshot-verified with Meziantou.Framework.SnapshotTesting.Roslyn (`Generators/__snapshots__/`).
- `Euterpe.Headless.Tests` — anything that needs running Avalonia UI (controls, views, bindings, input, theming) on Avalonia.Headless.
- `Euterpe.Releaser.Tests` — unit tests for the release automation under `build/Euterpe.Releaser`.
- `Euterpe.Tests` — plain unit tests; folders mirror the source projects (`App/` = `src/Euterpe`, plus `Core/`, `Models/`, `Shared/`, `Contracts/`).

## Workflow

- Never push to a remote (`jj git push` / `git push`) — commit locally at most; the user reviews and pushes themselves.

## Required rule loading

Before changing code or reviewing architecture, read the applicable rule files:

- Any code change or architecture/design review: `docs/agent-rules/design.md`
- Any C# change: `docs/agent-rules/csharp.md`
- Anything under `tests/`: both `docs/agent-rules/csharp.md` and `docs/agent-rules/tests.md`
- Any XAML/Avalonia UI change: `docs/agent-rules/xaml.md`

Treat these files as mandatory project instructions, not optional references.
