# NormalCAD

> 2D CAD system prototype developed in **C#** with **Avalonia UI**, structured in an **MVC** architecture with read/write support for **DXF/DWG** files via the **ACadSharp** library.

---

## Overview

**NormalCAD** is an open-source 2D technical drawing (CAD) application prototype. Its goal is to demonstrate how to build a functional CAD system using modern, open .NET technologies without relying on proprietary libraries such as the AutoCAD SDK.

The internal architecture was modeled after the **AutoCAD .NET API** (ObjectARX/Managed), using the same concepts of `Database`, `ObjectId`, `Transaction`, `BlockTable`, `LayerTable`, `ViewportTable`, and entities (`Entity`, `Curve`), making the project familiar to developers in the AEC (Architecture, Engineering and Construction) field. See [ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full entity API reference, namespace mapping, and project structure.

---

## Features

### Drawing Tools

- **Line** — Chain drawing with dashed dynamic preview during placement.
- **Circle** — Center + radius, with `Radius`/`Diameter` toggle via keyword at the prompt.
- **Arc** — Center + radius + start/end angle, with dynamic preview.
- **Polyline** — Successive clicks add vertices; `Undo` and `Close` keywords with prefix matching.
- **Selection** — Click to add, `Shift + Click` to remove. Drag left→right for Window Select, right→left for Crossing Select. Uses R*-tree spatial index for performance on large drawings.
- **Deletion** — `Delete` key removes all selected objects.
- **Clear** — Erase the entire current drawing.

### Integrated Command Line (AutoCAD-style)

- **Command Prompt** — Type a command name or alias and press `Enter`/`Space` to execute. `Escape` cancels the active command.
- **Keyword System** — Commands register prompt options (e.g. `[Undo/Close]`, `[Diameter/Radius]`). Prefix matching (type `U` for Undo); ambiguous prefixes are rejected.
- **Dynamic Prompt** — AutoCAD format: `"PLINE Specify next point or [Undo/Close]:"`. Prefix updates per active command. Selection feedback: `"1 found, 3 total"`.
- **Aliases** — Auto-discovered via reflection (e.g. `C`/`CI` for `CIRCLE`).
- **Floating Popup** — System feedback messages appear above the command bar with fade-out.

### Object Snapping

- **Endpoint** — Ends of lines, arcs and polyline vertices (green box).
- **Midpoint** — Midpoint of lines and polyline segments (green triangle).
- **Center** — Center of circles and arcs (green circle).

### Viewport and Navigation

- **Zoom** — Mouse scroll focused on cursor position.
- **Pan** — Middle mouse button drag.
- **Adaptive Grid** — Cartesian grid that adapts automatically to zoom level.
- **Viewport Persistence** — View position saved in the `*ACTIVE` record of `ViewportTable`.

### DXF and DWG Import / Export

- **Open (OPEN)** — Opens DXF/DWG files. Detects format by extension and imports lines, circles, arcs, polylines (`LwPolyline`) and block insertions (`Insert` → `BlockReference`).
- **Save (SAVE)** — Saves to the current document path. New documents (no path) behave as SAVEAS.
- **Save As (SAVEAS)** — Exports in AutoCAD, LibreCAD, QCAD-compatible format (DXF or DWG based on chosen extension), preserving entity properties (layer, color, linetype, lineweight, transparency).

### Interface

- **Property Palette** — Displays and edits properties of selected entities. Supports multi-selection merge (shows `*VARIES*` when values differ) and entity-specific Geometry/Misc categories. Category labels and property names sourced from localization-ready `.resx` files.
- **Layer Manager** — Create and manage layers. Entities inherit layer properties.
- **Status Bar** — Displays cursor coordinates in real time.
- **Dark Mode / Light Mode** — Toggle theme at runtime without restart (`THEME` command).

---

## Project Structure

```bash
NormalCAD.sln
├── docs/                    # Backlog, contributing guide, architecture docs
├── NormalCAD.Core/          # Class Library — data model, zero dependencies
│   ├── ApplicationServices/ # Application, Document, DocumentCollection
│   ├── DatabaseServices/    # Entity, Curve, Line, Circle, Arc, Polyline, Database, Transaction, tables
│   ├── EditorInput/         # Editor and prompt result types
│   ├── Geometry/            # Point3d, Vector3d, Matrix3d, Curve3d primitives
│   └── Spatial/             # R*-tree spatial index
├── NormalCAD/               # Avalonia UI application
│   ├── Controller/          # CadController, CmdManager, InputManager, Commands, Providers, Converters
│   ├── Resources/           # .resx localization files and ResourceManager helpers
│   ├── View/                # UI controls (viewport, palettes, menus) and entity renderers
│   ├── Host/                # Application host implementation
│   └── Utilities/           # Cross-cutting helpers (AngleConverter, etc.)
└── NormalCAD.Tests/         # xUnit tests (Core geometry, database, spatial)
```

`NormalCAD.Core` is a **pure Class Library** (no UI or ACadSharp dependency), mirroring the AutoCAD `AcDbMgd.dll` / `AcMgd.dll` separation. For the full file-level tree, see [ARCHITECTURE.md](docs/ARCHITECTURE.md).

---

## Dependencies

| Package | Project | License | Usage |
| --- | --- | --- | --- |
| `Avalonia` 12.0.4 | NormalCAD | MIT | Cross-platform UI framework |
| `Avalonia.Desktop` 12.0.4 | NormalCAD | MIT | Windows/Linux/macOS support |
| `Avalonia.Themes.Fluent` 12.0.4 | NormalCAD | MIT | Visual theme |
| `ACadSharp` 3.6.29 | NormalCAD | MIT | DXF and DWG read/write |
| `xUnit` 2.9.3 | NormalCAD.Tests | Apache 2.0 | Testing framework |
| `NormalCAD.Core` | — | MIT | **No external dependencies** |

---

## Getting Started

### Prerequisites

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)

### Clone and Run

```bash
git clone https://github.com/CarthoCAD/NormalCAD.git
cd NormalCAD

# Restore dependencies
dotnet restore

# Build the entire solution
dotnet build

# Run the application
dotnet run --project NormalCAD/NormalCAD.csproj
```

### Run Tests

```bash
dotnet test NormalCAD.Tests/NormalCAD.Tests.csproj
```

---

## How to Use

| Action | Command | Aliases | Menu |
| --- | --- | --- | --- |
| Draw Line | `LINE` | `L` | Draw → Line |
| Draw Circle | `CIRCLE` | `C`, `CI` | Draw → Circle |
| Draw Arc | `ARC` | `A` | Draw → Arc |
| Draw Polyline | `PLINE` | `PL` | Draw → Polyline |
| Delete Selected | `ERASE` | `E` | Edit → Erase |
| Clear All | `CLEANALL` | `CLA` | Edit → Clean All |
| Open File | `OPEN` | — | File → Open... |
| Save | `SAVE` | — | File → Save |
| Save As | `SAVEAS` | — | File → Save As... |
| Toggle Theme | `THEME` | `TH` | Base → Change Theme |
| Quit | `QUIT` | `EXIT`, `Q` | File → Exit |
| **Pan** | Middle mouse button drag | | |
| **Zoom** | Mouse scroll (focused on cursor) | | |
| **Select** | Click to add, `Shift + Click` to remove | | |
| **Window Select** | Drag left→right (Window) or right→left (Crossing) | | |
| **Cancel** | `Escape` | | Edit → Select |

---

## Contributing

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for commit conventions, branching, and development workflow.

---

## License

Distributed under the **MIT** license. See the `LICENSE` file for details.
