# NormalCAD

> 2D CAD system prototype developed in **C#** with **Avalonia UI**, structured in an **MVC** architecture with read/write support for **DXF/DWG** files via the **ACadSharp** library.

---

## Overview

**NormalCAD** is an open-source 2D technical drawing (CAD) application prototype. Its goal is to demonstrate how to build a functional CAD system using modern, open .NET technologies without relying on proprietary libraries such as the AutoCAD SDK.

The internal drawing database architecture was modeled after the **AutoCAD .NET API** (ObjectARX/Managed), using the same concepts of `Database`, `ObjectId`, `Transaction`, `BlockTable`, `LayerTable`, `ViewportTable` and entities (`Entity`), making the project familiar to developers in the AEC (Architecture, Engineering and Construction) field. The namespace structure also mirrors the AutoCAD API: `NormalCAD.Core.ApplicationServices`, `NormalCAD.Core.DatabaseServices`, `NormalCAD.Core.EditorInput` and `NormalCAD.Core.Geometry`.

### Entity API

The `Entity` class implements the properties and methods of the AutoCAD .NET API:

**Properties:** `Layer`, `LayerId`, `Color`, `Linetype`, `LinetypeId`, `LineWeight`, `LinetypeScale`, `Transparency`, `Visible`, `BlockId`, `BlockTransform`, `Bounds`, `GeometricExtents`

**Methods:** `Clone()`, `GetTransformedCopy()`, `IntersectWith()`, `GetDistanceTo()`, `GetGripPoints()`, `MoveGripPointsAt()`, `GetStretchPoints()`, `MoveStretchPointsAt()`, `GetOsnapPoints()`, `Highlight()` / `Unhighlight()`, `Erase()`, `Draw()`, `List()`, `SetDatabaseDefaults()`

The `Curve` class adds: `Length`, `Closed`, `Area`, `StartPoint`, `EndPoint`, `GetPointAtDist()`, `GetDistAtPoint()`, `GetClosestPointTo()`, `GetFirstDerivative()`, `GetPointAtParameter()`, `GetParameterAtPoint()`

Intersection and distance are delegated to geometric primitives (`Curve3d` → `LineSegment3d` / `CircularArc3d` / `CompositeCurve3d`) via `GetGeometricCurve()`.

---

## Features

### Drawing Tools

- **Line** — Chain drawing (end of one line is the start of the next), with dashed dynamic preview during placement.
- **Circle** — Click center, drag to set radius. Supports `Radius`/`Diameter` toggle via keyword at the prompt.
- **Arc** — Click center, drag to set radius + start angle, click for end angle. Dynamic preview.
- **Polyline** — Successive clicks add vertices; `Enter`/`Space` finishes open. `Undo` (removes last vertex) and `Close` (closes polyline) keywords at the prompt, with prefix matching (e.g. type "U" for Undo). Preview updates automatically when using keywords.
- **Selection** — Click to add entities individually to selection; `Shift + Click` to remove. Drag left→right for Window Select or right→left for Crossing Select. Uses R*-tree spatial index for performance on large drawings.
- **Deletion** — `Delete` key removes all selected objects.
- **Clear** — Button to erase the entire current drawing.

### Integrated Command Line (AutoCAD-style)

- **Command Prompt** — `TextBox` on the bottom bar: type a command name or alias and press `Enter` or `Space` to execute it. `Escape` cancels the active command.
- **Keyword System** — Commands can register prompt options (e.g. `[Undo/Close]`, `[Diameter/Radius]`). The user types the full keyword or just the prefix (`U` → Undo). If ambiguous (two keywords share the same prefix), the system rejects and informs. While keywords are active, the prompt blocks new command execution.
- **Dynamic Prompt** — AutoCAD format: `"PLINE Specify next point or [Undo/Close]:"`. Prompt prefix (`CMD:`, `CIRCLE:`, etc.) updates according to the active command. During window selection, displays `"CMD Specify opposite corner:"`. Selection feedback displays `"1 found, 3 total"` or `"2 removed, 1 total"`.
- **Aliases** — Each command automatically registers its aliases (e.g. `C` or `CI` for `CIRCLE`). `CmdManager` resolves aliases via reflection-based discovery.
- **Floating Popup** — System feedback messages appear above the command bar with fade-out.

### Object Snapping

- **Endpoint** — Ends of lines, arcs and polyline vertices (indicator: **green box**).
- **Midpoint** — Midpoint of lines and polyline segments (indicator: **green triangle**).
- **Center** — Center of circles and arcs (indicator: **green circle**).

### Viewport and Navigation

- **Zoom** — Mouse scroll focused on cursor position.
- **Pan** — Middle mouse button drag.
- **Adaptive Grid** — Cartesian grid that adapts automatically to zoom level.
- **Viewport Persistence** — View position saved in the `*ACTIVE` record of `ViewportTable`.

### DXF and DWG Import / Export

- **Open (OPEN)** — Opens DXF/DWG files. Detects format by extension and imports lines, circles, arcs, polylines (`LwPolyline`) and block insertions (`Insert` → `BlockReference`). Creates a new document via `Application.DocumentManager`.
- **Save (SAVE)** — Saves to the current document path. If a new document (no path), behaves as SAVEAS.
- **Save As (SAVEAS)** — Exports in AutoCAD, LibreCAD, QCAD-compatible format (DXF or DWG based on chosen extension), preserving entity properties (layer, color, linetype, lineweight, transparency).

### Interface and Themes

- Property Palette, Layer Manager, Status Bar.
- Dynamic Dark Mode / Light Mode without restart.

---

## Project Structure

```bash
NormalCAD.sln
├── NormalCAD.Core/              # Class Library — Data model (zero dependencies)
│   ├── NormalCAD.Core.csproj
│   ├── ApplicationServices/     # Application, Document, DocumentCollection, DocumentLock
│   │   ├── Application.cs       # Static facade (singleton), Application.Host
│   │   ├── Document.cs          # Database + Editor + LockDocument()
│   │   ├── DocumentCollection.cs # MdiActiveDocument
│   │   └── DocumentLock.cs      # IDisposable, Monitor.Enter/Exit
│   ├── DatabaseServices/        # Database auxiliary types
│   │   ├── Intersect.cs         # Enum Intersect (OnBothOperands, ExtendThis, ExtendArgument, ExtendBoth)
│   │   ├── LineWeight.cs        # Enum LineWeight (ByLayer, ByBlock, Default, W0..W211)
│   │   └── Transparency.cs      # Struct Transparency (ByLayer / alpha 0-255)
│   ├── EditorInput/             # Editor, PromptPointResult, PromptPointOptions, PromptStatus
│   │   ├── Editor.cs            # GetPoint(string), GetPoint(PromptPointOptions) — temporary shell
│   │   └── PromptResult.cs      # PromptStatus (OK/Cancel/Keyword/Error)
│   ├── Entities/                # Concrete entities
│   │   ├── Line.cs, Circle.cs, Arc.cs, Polyline.cs
│   │   └── BlockReference.cs    # Block insertion (Position, Rotation, ScaleFactors)
│   ├── Geometry/                # Geometric primitives and math
│   │   ├── Point2d.cs, Point3d.cs, Vector3d.cs, Matrix3d.cs, Extents3d.cs, Point3dCollection.cs
│   │   ├── Curve3d.cs           # Abstract class — base for geometric curves
│   │   ├── LineSegment3d.cs     # Line segment (P0→P1)
│   │   ├── CircularArc3d.cs     # Circular arc / full circle
│   │   └── CompositeCurve3d.cs  # Composite curve (iterates segments)
│   ├── Spatial/                 # RTree (R*-tree spatial index)
│   ├── DBObject.cs              # Base for all database objects
│   ├── Entity.cs                # Base for all entities (Layer, Color, Linetype, LineWeight, etc.)
│   ├── Curve.cs                 # Base for curve entities (Length, GetPointAtDist, GetClosestPointTo, etc.)
│   ├── Database.cs, ObjectId.cs, Transaction.cs, TransactionManager.cs
│   ├── BlockTable.cs, BlockTableRecord.cs
│   ├── LayerTable.cs, LayerTableRecord.cs
│   ├── ViewportTable.cs, ViewportTableRecord.cs
│   ├── SymbolTable.cs, SymbolTableRecord.cs
│   ├── EntityColor.cs, SnapType.cs, OpenMode.cs, Culture.cs
│   └── IApplicationHost.cs      # Internal host initialization interface
│
├── NormalCAD/                   # WinExe — Application (Avalonia UI + commands)
│   ├── NormalCAD.csproj          (References NormalCAD.Core)
│   ├── Host/                    # Host implementation
│   │   └── ApplicationHost.cs   # IApplicationHost, creates documents
│   ├── Resources/               # Localization resources (.resx)
│   │   ├── Commands.resx        # Command prompts, messages, keywords, names, aliases
│   │   ├── Panels.resx          # Panel and control UI strings (palettes, menus, bars)
│   │   ├── Dialogs.resx         # Dialog titles, filters, error messages, system strings
│   │   ├── CommandResources.cs  # ResourceManager helper for command strings
│   │   ├── PanelResources.cs    # ResourceManager helper for panel strings
│   │   └── DialogResources.cs   # ResourceManager helper for dialog strings
│   ├── Controller/              # Command logic and orchestration
│   │   ├── CadController.cs     # Central orchestrator (initializes Application, manages Document)
│   │   ├── CmdManager.cs        # Command discovery, registration and dispatch
│   │   ├── InputManager.cs      # Input + prompt keywords + prefix matching
│   │   ├── Commands/            # ICadCommand implementations
│   │   └── Services/Converters/ # NormalCAD ↔ ACadSharp converters
│   ├── View/                    # Avalonia UI
│   │   ├── Controls/            # CadViewport, BottomBar, MenuBar, palettes
│   │   └── Drawing/             # Renderers (Line, Circle, Arc, Polyline)
│   ├── MainWindow.axaml, App.axaml, Program.cs
│   └── Themes/
│
└── NormalCAD.Tests/             # Unit tests (xUnit)
    ├── NormalCAD.Tests.csproj    (References NormalCAD.Core)
    └── Core/
        ├── Geometry/
        │   ├── LineSegment3dTests.cs
        │   ├── CircularArc3dTests.cs
        │   └── CompositeCurve3dTests.cs
        ├── DatabaseServices/
        │   └── PolylineTests.cs
        └── Spatial/
            └── RTreeTests.cs
```

`NormalCAD.Core` is a **pure Class Library** (no UI or ACadSharp dependency). This allows plugins and extensions to reference only the data model, mirroring the AutoCAD `AcDbMgd.dll` / `AcMgd.dll` separation.

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

| Action | How to |
| --- | --- |
| **Pan** | Middle mouse button drag |
| **Zoom** | Mouse scroll (focused on cursor position) |
| **Draw Line** | Type `LINE` / `L` and click two points, or menu Draw → Line |
| **Draw Circle** | Type `CIRCLE` / `C` / `CI` — click center; type `D` + Enter for Diameter, `R` + Enter for Radius; click to set radius/diameter |
| **Draw Arc** | Type `ARC` / `A` and click center → radius → end angle, or menu Draw → Arc |
| **Draw Polyline** | Type `PLINE` / `PL` and click vertices; `Enter` finishes open; keywords: `U` (Undo), `C` (Close via prompt) |
| **Select** | Click entity to add to selection; `Shift + Click` to remove |
| **Window Selection** | Drag left → right (Window) or right → left (Crossing) |
| **Delete Selected** | `Delete` key or type `ERASE` / `E` |
| **Cancel / Go Back** | `Escape` (clears prompt and returns to selection) or menu Edit → Select |
| **Clear All** | Type `CLEANALL` / `CLA` or menu Edit → Clean All |
| **Open** | Type `OPEN` or menu File → Open... |
| **Save** | Type `SAVE` or menu File → Save |
| **Save As** | Type `SAVEAS` or menu File → Save As... |
| **Toggle Theme** | Type `THEME` / `TEMA` / `TH` or menu → Change Theme |
| **Quit** | Type `QUIT` / `EXIT` / `Q` or menu File → Exit |

### Commands and Aliases

| Command | Type | Aliases |
| --- | --- | --- |
| Line | `LINE` | `L` |
| Circle | `CIRCLE` | `C`, `CI` |
| Arc | `ARC` | `A` |
| Polyline | `PLINE` | `PL` |
| Erase | `ERASE` | `E` |
| Clean All | `CLEANALL` | `CLA` |
| Open | `OPEN` | — |
| Save | `SAVE` | — |
| Save As | `SAVEAS` | — |
| Toggle Theme | `THEME` | `TH` |
| Quit | `QUIT` | `EXIT`, `Q` |

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for commit conventions, branching, and development workflow.

---

## License

Distributed under the **MIT** license. See the `LICENSE` file for details.
