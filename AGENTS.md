# AGENTS.md — NormalCAD

## Project Overview

NormalCAD is a 2D CAD prototype in **C# / Avalonia UI** with **ACadSharp** for DXF/DWG I/O. Its primary goal is to be highly compatible with AutoCAD files, workflows, and .NET plugins. See the project README and the `/docs` folder for the full structure and entity API reference.

## Solution Structure

```bash
NormalCAD.Core/    # Pure class library, zero external deps (data model, geometry, spatial index)
NormalCAD/         # Avalonia UI, commands, renderers, converters, property providers
NormalCAD.Tests/   # xUnit tests, references Core only
```

Keep Core UI-agnostic and ACadSharp-agnostic. UI concerns (property metadata, rendering, commands) live in `NormalCAD/`.

## Coding Conventions

- Language: **English** for code, identifiers, and comments.
- User-facing strings go to `.resx` resources (`Commands.resx`, `Panels.resx`, `EntityProperties.resx`, etc.) and are read through their `*Resources` helpers.
- Namespace matches folder path. Core namespaces mirror AutoCAD:
  - `NormalCAD.Core.ApplicationServices`
  - `NormalCAD.Core.DatabaseServices`
  - `NormalCAD.Core.EditorInput`
  - `NormalCAD.Core.Geometry`
- **AutoCAD compatibility first:** when adding or modifying features, preserve maximum compatibility with the AutoCAD .NET API and user workflow. Check how AutoCAD solves the same problem and whether `NormalCAD.Core` already has a partial mirror of that API that can be reused or extended. Refer to the AutoCAD .NET Managed Reference at <https://help.autodesk.com/view/OARX/2026/ENU/?guid=OARX-ManagedRefGuide-What_s_New>. Strive for 100% user-facing compatibility, especially for entities and commands.

## Commits & Workflow

- Follow [Conventional Commits](https://www.conventionalcommits.org/): `<type>(<scope>): <description> (#<issue>)`
- Types: `feat`, `fix`, `refactor`, `perf`, `style`, `test`, `docs`, `chore`, `i18n`, `ci`, `build`, `revert`
- Scopes: `commands`, `viewport`, `palettes`, `geometry`, `database`, `io`, `themes`, `ui`, `tests`, `i18n`, `build`, `release`
- Keep commit message lines under **100 characters** to satisfy commitlint.
- **Commit message composition:** write the message to a temporary file first (e.g. `$env:TEMP\commit-msg.txt`), then use `git commit -F $env:TEMP\commit-msg.txt`. This avoids shell-quoting issues with `-m` flags and ensures multi-line bodies are preserved correctly for commitlint validation.
- **Human-in-the-loop workflow:**
  1. User requests changes.
  2. Assistant makes the changes and reports the result.
  3. User manually verifies.
  4. If verification passes, user asks the assistant to compose the commit message.
  5. If the message is approved, user explicitly authorizes commit and push.
- **Do not commit, push, or mutate git history without explicit user authorization.**

## Useful Commands

```bash
dotnet build NormalCAD.sln
dotnet test NormalCAD.Tests/NormalCAD.Tests.csproj
```
