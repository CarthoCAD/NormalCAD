# Contributing

## Setup

```bash
git clone https://github.com/<seu-usuario>/NormalCAD.git
cd NormalCAD
dotnet restore
npm ci
```

Requires [.NET 9.0 SDK](https://dotnet.microsoft.com/download) and [Node.js LTS](https://nodejs.org/).

## Commit Convention

This project follows [Conventional Commits](https://www.conventionalcommits.org/).

### Format

```
<type>(<scope>): <short description> (#123)

[optional body]
```

Every commit must reference an issue at the end of the subject line. Automated merges (PR merge commits) are exempt.

### Types

| Type       | Usage                                          |
|------------|------------------------------------------------|
| `feat`     | New feature (triggers MINOR release)           |
| `fix`      | Bug fix (triggers PATCH release)               |
| `refactor` | Code change that neither fixes nor adds        |
| `perf`     | Performance improvement                        |
| `style`    | Formatting, themes, UI visual                  |
| `test`     | Adding or updating tests                       |
| `docs`     | Documentation only                             |
| `chore`    | Maintenance, dependencies, tooling             |
| `i18n`     | Translations and localization                  |
| `ci`       | CI/CD configuration                            |
| `build`    | Build system changes                           |
| `revert`   | Revert a previous commit                       |

### Scopes

| Scope       | Area                             |
|-------------|----------------------------------|
| `commands`  | Command system                   |
| `viewport`  | Drawing canvas and rendering     |
| `palettes`  | Property and layer palettes      |
| `geometry`  | Core geometry types              |
| `database`  | Drawing database and entities    |
| `io`        | DXF/DWG file import/export       |
| `themes`    | Light/dark themes                |
| `ui`        | General UI (menus, layout)       |
| `tests`     | Unit tests                       |
| `build`     | Build system and project files   |
| `release`   | Release process                  |

### Examples

```
feat(vieweport): add zoom-to-extents command (#42)
fix(io): prevent crash on empty DXF files (#17)
i18n(pt-BR): translate property palette strings (#58)
refactor(geometry): simplify intersection algorithm (#61)
```

## Branching

```
main         ← integration and releases
feat/*       ← new features
fix/*        ← bug fixes
```

## Workflow

1. Create a branch from `main`
2. Make changes with conventional commits referencing an issue
3. Open a PR to `main` (CI runs build, tests, and commit validation)
4. After review, merge into `main`
5. When ready for release, go to Actions → Release → Run workflow
