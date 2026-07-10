# Architecture

## Project Structure

```bash
NormalCAD.sln
├── docs/                        # Solution-wide documentation
│   ├── Backlog.md               # Prioritized backlog
│   ├── CONTRIBUTING.md          # Commit conventions and workflow
│   ├── AddingNewEntities.md     # Step-by-step guide to add a new entity
│   └── ARCHITECTURE.md          # This file
│
├── NormalCAD.Core/              # Class Library — Data model (zero dependencies)
│   ├── NormalCAD.Core.csproj
│   ├── ApplicationServices/     # Application, Document, DocumentCollection, DocumentLock
│   │   ├── Application.cs       # Static facade (singleton), Application.Host
│   │   ├── Document.cs          # Database + Editor + LockDocument()
│   │   ├── DocumentCollection.cs # MdiActiveDocument
│   │   ├── DocumentLock.cs      # IDisposable, Monitor.Enter/Exit
│   │   └── IApplicationHost.cs  # Internal host initialization interface
│   ├── DatabaseServices/        # Database, entities and auxiliary types
│   │   ├── Entity.cs            # Base for all entities (Layer, Color, Linetype, LineWeight, etc.)
│   │   ├── Curve.cs             # Base for curve entities (Length, Area, GetPointAtDist, etc.)
│   │   ├── Line.cs, Circle.cs, Arc.cs, Polyline.cs
│   │   ├── BlockReference.cs    # Block insertion (Position, Rotation, ScaleFactors)
│   │   ├── DBObject.cs          # Base for all database objects
│   │   ├── Database.cs, ObjectId.cs, Transaction.cs, TransactionManager.cs
│   │   ├── BlockTable.cs, BlockTableRecord.cs
│   │   ├── LayerTable.cs, LayerTableRecord.cs
│   │   ├── ViewportTable.cs, ViewportTableRecord.cs
│   │   ├── SymbolTable.cs, SymbolTableRecord.cs
│   │   ├── EntityColor.cs, SnapType.cs, OpenMode.cs, Culture.cs
│   │   ├── Intersect.cs         # Enum Intersect (OnBothOperands, ExtendThis, ExtendArgument, ExtendBoth)
│   │   ├── LineWeight.cs        # Enum LineWeight (ByLayer, ByBlock, Default, W0..W211)
│   │   └── Transparency.cs      # Struct Transparency (ByLayer / alpha 0-255)
│   ├── EditorInput/             # Editor, PromptPointResult, PromptPointOptions, PromptStatus
│   │   ├── Editor.cs            # GetPoint(string), GetPoint(PromptPointOptions) — temporary shell
│   │   └── PromptResult.cs      # PromptStatus (OK/Cancel/Keyword/Error)
│   ├── Geometry/                # Geometric primitives and math
│   │   ├── Point2d.cs, Point3d.cs, Vector3d.cs, Matrix3d.cs, Extents3d.cs, Point3dCollection.cs
│   │   ├── Curve3d.cs           # Abstract class — base for geometric curves
│   │   ├── LineSegment3d.cs     # Line segment (P0→P1)
│   │   ├── CircularArc3d.cs     # Circular arc / full circle (angles in radians)
│   │   └── CompositeCurve3d.cs  # Composite curve (iterates segments)
│   └── Spatial/                 # RTree (R*-tree spatial index)
│
├── NormalCAD/                   # WinExe — Application (Avalonia UI + commands)
│   ├── NormalCAD.csproj          (References NormalCAD.Core)
│   ├── Host/                    # Host implementation
│   │   └── ApplicationHost.cs   # IApplicationHost, creates documents
│   ├── Resources/               # Localization resources (.resx)
│   │   ├── Commands.resx        # Command prompts, messages, keywords, names, aliases
│   │   ├── Panels.resx          # Panel and control UI strings (palettes, menus, bars)
│   │   ├── Dialogs.resx         # Dialog titles, filters, error messages, system strings
│   │   ├── EntityProperties.resx # Entity property display names and category labels
│   │   ├── ComboOptions.resx    # Combo option display names (LineWeight, Linetype, etc.)
│   │   ├── CommandResources.cs  # ResourceManager helper for command strings
│   │   ├── PanelResources.cs    # ResourceManager helper for panel strings
│   │   ├── DialogResources.cs   # ResourceManager helper for dialog strings
│   │   ├── EntityPropertyResources.cs # ResourceManager helper for entity property strings
│   │   └── ComboOptionResources.cs    # ResourceManager helper for combo option strings
│   ├── Controller/              # Command logic and orchestration
│   │   ├── CadController.cs     # Central orchestrator (initializes Application, manages Document)
│   │   ├── CmdManager.cs        # Command discovery, registration and dispatch
│   │   ├── InputManager.cs      # Input + prompt keywords + prefix matching
│   │   ├── Commands/            # ICadCommand implementations
│   │   ├── Providers/           # Entity property providers (PropertyPalette metadata)
│   │   │   ├── IEntityPropertyProvider.cs, EntityPropertyManager.cs
│   │   │   ├── PropertyDescriptor.cs, ComboOption.cs, EntityPropertyProvider.cs
│   │   │   ├── Line/Circle/Arc/PolylinePropertyProvider.cs
│   │   │   └── LineWeightOptionProvider.cs, LinetypeOptionProvider.cs
│   │   └── Services/Converters/ # NormalCAD ↔ ACadSharp converters
│   ├── Utilities/              # Cross-cutting helpers (AngleConverter, etc.)
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

## Assembly Separation

`NormalCAD.Core` is a **pure Class Library** (no UI or ACadSharp dependency). This allows plugins and extensions to reference only the data model, mirroring the AutoCAD `AcDbMgd.dll` / `AcMgd.dll` separation.

- **`NormalCAD.Core`** — Data model, geometry primitives, database services, spatial index. Zero external dependencies.
- **`NormalCAD`** — Avalonia UI, commands, renderers, converters, property providers. References `NormalCAD.Core`, `Avalonia`, and `ACadSharp`.
- **`NormalCAD.Tests`** — xUnit tests. References `NormalCAD.Core` only.

The namespace structure mirrors the AutoCAD .NET API:

| AutoCAD API | NormalCAD |
| --- | --- |
| `Autodesk.AutoCAD.ApplicationServices` | `NormalCAD.Core.ApplicationServices` |
| `Autodesk.AutoCAD.DatabaseServices` | `NormalCAD.Core.DatabaseServices` |
| `Autodesk.AutoCAD.EditorInput` | `NormalCAD.Core.EditorInput` |
| `Autodesk.AutoCAD.Geometry` | `NormalCAD.Core.Geometry` |

## Entity API

The entity model mirrors the AutoCAD .NET `Entity` and `Curve` base classes.

### `Entity`

**Properties:** `Layer`, `LayerId`, `Color`, `Linetype`, `LinetypeId`, `LineWeight`, `LinetypeScale`, `Transparency`, `Visible`, `BlockId`, `BlockTransform`, `Bounds`, `GeometricExtents`

**Methods:** `Clone()`, `GetTransformedCopy()`, `IntersectWith()`, `GetDistanceTo()`, `GetGripPoints()`, `MoveGripPointsAt()`, `GetStretchPoints()`, `MoveStretchPointsAt()`, `GetOsnapPoints()`, `Highlight()` / `Unhighlight()`, `Erase()`, `Draw()`, `List()`, `SetDatabaseDefaults()`

### `Curve`

Adds: `Length`, `Closed`, `Area`, `StartPoint`, `EndPoint`, `GetPointAtDist()`, `GetDistAtPoint()`, `GetClosestPointTo()`, `GetFirstDerivative()`, `GetPointAtParameter()`, `GetParameterAtPoint()`

Intersection and distance are delegated to geometric primitives (`Curve3d` → `LineSegment3d` / `CircularArc3d` / `CompositeCurve3d`) via `GetGeometricCurve()`.

## Adding a New Entity

See [AddingNewEntities.md](AddingNewEntities.md) for the complete checklist covering all layers: Core model, renderer, property provider, converter, and draw command.
