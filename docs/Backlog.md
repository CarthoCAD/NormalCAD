# Backlog

## Create `EntityProperties.resx` (priority: high)

Create `NormalCAD\Resources\EntityProperties.resx` with all entity property display names and category labels currently defined as `[DisplayName]` and `[Category]` attribute strings in `NormalCAD.Core`: General category properties (`"Layer"`, `"Color"`, `"Linetype"`, `"Lineweight"`, `"Linetype Scale"`, `"Transparency"`, `"Visible"`), Geometry category properties for each entity type (`"Start X"`, `"Start Y"`, `"Start Z"`, `"End X"`, `"End Y"`, `"End Z"`, `"Length"`, `"Radius"`, `"Center X"`, `"Center Y"`, `"Start Angle"`, `"End Angle"`, `"Elevation"`, `"Vertices"`, `"Area"`, `"Closed"`), and the category headers themselves (`"General"`, `"Geometry"`). Use key format `<Entity>.<Category>.<Property>` (e.g., `LINE.GEOMETRY.STARTX`, `CIRCLE.GEOMETRY.RADIUS`, `ENTITY.GENERAL.LAYER`). This resource will be consumed by the `EntityPropertyManager` and its providers (not by entity classes directly), so the Core assembly remains free of presentation concerns. Depends on the Entity Property Manager implementation so providers have a resource-backed string source to query instead of hardcoded attributes.

## Decouple `PropertyPalette` — Use DataTemplates (priority: high)

The `PropertyPalette` control builds its entire UI programmatically in ~330 lines of C#: TextBox for string properties, ComboBox for enums and booleans, Border containers, and Grid row/column definitions — all created with `new` in code-behind. This bypasses Avalonia's styling system, prevents compiled bindings, and makes theming and layout changes unnecessarily difficult. Replace with an `ItemsControl` bound to a collection of lightweight data-only structs (carrying category, display name, property type, current value, and a write-back callback or identifier), plus XAML DataTemplates with `x:DataType` for compiled binding support. The palette becomes a pure presentation layer with zero knowledge of entities, providers, or the database. This also enables reusing the same palette control for non-entity property sources (layers, blocks, external references) with no UI code changes. Depends on the Entity Property Manager so the struct-based property descriptors are available to bind against.

## Refactor `ICadCommand` Interface (priority: high)

Remove Avalonia framework types (`PointerPressedEventArgs`, `KeyEventArgs`) from the `ICadCommand` contract and replace them with plain POCO structs defined in the Core assembly (e.g., `PointerEventData`, `KeyEventData`). Fix the `async void Activate()` mismatch — `OpenCommand`, `SaveCommand`, and `SaveAsCommand` declare `async void` while the interface declares `void`, risking unhandled exception process crashes. Add `CommandType` (Interactive / Immediate) to formalize the lifecycle distinction already present in the code, and `CommandFlags` (with `UsePickSet` for pick-first selection, extensible for `Session`, `Transparent`, `NoMultiple`, `NoUndoMarker` when those features are needed). Currently every command improvises its own lifecycle — `EraseCommand`, `CleanAllCommand`, `Quit`, and `Theme` do all their work synchronously inside `Activate()` and immediately call `SetCommand(new BaseCommand())`, breaking the intended Activate/Deactivate contract and making it impossible to implement transparent commands or undo markers later without touching every command.

## Extract Idle State from `BaseCommand` (priority: high)

`BaseCommand` is a pseudo-command that implements `ICadCommand` only so it can be set as the active command via `SetCommand()`. This leaks into three layers: the `ICadCommand` interface (`IsInternal` property exists solely to block it), the `CmdManager` (special-case logic rejects `IsInternal` commands from user input), and 10 concrete commands (all call `controller.SetCommand(new BaseCommand())` upon completion, coupling them to the concrete idle type). Extract selection logic (single-click picking, box selection, entity hit-testing, `found`/`removed` messaging) into an `IdleState` class managed internally by `CadController`. Expose a `CadController.FinishCommand()` method so commands return to idle without constructing or knowing about the idle type. This is a prerequisite for transparent commands (which require restoring a paused command rather than jumping to idle) and for an undo system (which needs the active command to register its own undo group, not delegate to a catch-all idle handler).

## Refactor Command Input System (priority: high)

Replace direct event handling (`OnPointerPressed`, `OnPointerMoved`, `OnKeyDown`) on each command with a callback registration pattern through `InputManager`. Instead of the command receiving raw Avalonia events and extracting coordinates itself, it registers typed callbacks with the `InputManager`: a point-pick callback invoked when the user clicks in the viewport (receives a parsed `Point3d`), a mouse-move callback (receives a `Point3d` for live previews), a coordinate string callback invoked when the user types coordinates in the command bar (e.g., `"100,200"` — `InputManager` parses and passes a `Point3d`), a numeric callback for commands expecting a single value or distance, and a general string callback for arbitrary command-line input. This decouples commands from input hardware details, enables typed coordinate entry (essential for precision CAD drafting), and makes the command state machine testable without a running Avalonia renderer.

## Introduce `DrawingCommandBase` (priority: high)

DrawLine, DrawCircle, DrawArc, and DrawPolyline share near-identical `Activate()` and `Deactivate()` boilerplate: cursor state transitions (`PickCross` → `Crosshair` and back), `ActiveCommandPreview` teardown, keyword cleanup, and repeating the same pattern of checking `_controller == null`. Extract an abstract `DrawingCommandBase : ICadCommand` that handles cursor state management, preview clearing on deactivate, keyword reset, and standardized prompt setup via `InputManager`. Circle and Polyline already use the prompt/keyword system; Line and Arc were written before that system existed and currently show no prompts at all — they should be upgraded to use it through the base class so all drawing commands follow a consistent interactive pattern. This eliminates ~30 duplicated lines per command and ensures every future drawing command (Rectangle, Ellipse, Spline, etc.) inherits correct behavior automatically.

## Complete `BlockReference` Entity Pipeline (priority: high)

`BlockReference` exists in `NormalCAD.Core` but is not yet a fully selectable, drawable entity — it must be wired through the complete entity pipeline (see `AddingNewEntities.md` for the full step-by-step). Currently `BlockReference` and its nested sub-entities are not rendered in the viewport, making block insertion functionally invisible. Deliver every stage of the pipeline for this entity:

- **Renderer** — add a `BlockReferenceRenderer : IEntityRenderer` and register it in `DrawingService`, transforming and drawing each nested sub-entity through the block transform. Investigate the current failure: the cause could be in `DrawingService.DrawEntity` (entity type dispatch may not handle `BlockReference`), in `BlockReference.GetGeometricCurve()` / `GeometricExtents` (computing empty bounds that get culled), or in the renderer's coordinate transform chain for nested entities.
- **Provider** — add a `BlockReferencePropertyProvider : IEntityPropertyProvider` and register it in `EntityPropertyManager`, exposing the AutoCAD INSERT palette properties (Position X/Y/Z, Scale X/Y/Z, Rotation, Block Name, etc.).
- **Converter** — verify/fix `BlockReferenceConverter` so the DWG/DXF reader correctly populates sub-entities and the block transform on round-trip.
- **Draw command** — implement the `INSERT` command (`InsertCommand : ICadCommand`) that lets the user pick a block, place it interactively (with live preview), and set rotation/scale, following the same interactive pattern as the other drawing commands.

This is the first end-to-end exercise of the "add a new entity" pipeline against an already-modeled Core entity, so it doubles as validation of `AddingNewEntities.md`.

## Undo System (priority: high)

Implement a full undo/redo stack using the AutoCAD command-group model: each interactive command or immediate action registers an `UndoGroup` that wraps the set of database modifications it performs. The `TransactionManager` must track object state snapshots (before/after values for modified properties, or pre-modification clones for structural changes like adding/removing entities) so that undo can restore them. The undo stack is managed per-document by the `Database`, with `Undo()` and `Redo()` methods exposed through the `Editor`. A `NoUndoMarker` flag on commands (already reserved in the planned `CommandFlags`) should suppress undo recording for non-destructive operations like ZOOM, REGEN, or inquiry commands. Depends on the `ICadCommand` refactoring (to add `CommandFlags.NoUndoMarker`) and on the idle state extraction (so `BaseCommand` doesn't interfere with undo group boundaries).

## Centralize Document and Database Access (priority: medium)

`CadController.Database` and `CadController.Document` are accessed directly from commands, UI controls, services, and converters scattered across the codebase. When active document switching is implemented, any code holding a stale reference to a previous document will operate on the wrong data. All access to the current document and database must route through `Application.DocumentManager.ActiveDocument` so that a document switch automatically redirects every consumer. This affects at least `FileService`, all `ICadCommand` implementations, `PropertyPalette`, `LayerPalette`, `DrawingService`, and `CadViewport`. The `CadController` should become a thin facade that delegates to `DocumentManager.ActiveDocument` rather than holding its own mutable `Document` field.

## Cache Brushes and Pens in `DrawingService` (priority: medium)

`DrawingService.DrawEntity` allocates a new `SolidColorBrush` and a new `Pen` for every entity on every frame. With 1000 entities at 60fps, that is approximately 120,000 allocations per second just for rendering brushes and pens, driving significant GC pressure. Cache brushes and pens in a `ConcurrentDictionary` keyed by `(Avalonia.Media.Color, double thickness, DashStyle?)` or similar tuple. Invalidate and clear the cache on theme change (Light ↔ Dark), since theme tokens resolve to different colors.

## DBObject API Compatibility (priority: medium)

The `DBObject` class in `NormalCAD.Core` currently exposes only 7 of the 16 properties and 1 of the 11 methods defined in the AutoCAD .NET `DBObject` base class. The most impactful gap is `UpgradeOpen()` (promote from `ForRead` to `ForWrite` within a transaction) — without it, any code that obtains an object as read-only must re-open it for write access, adding boilerplate and risking stale references. Also missing: `DowngradeOpen()`, `Cancel()`, `HandOverTo(ObjectId)` (transfer ownership, needed for moving entities between block records), `DeepClone(...)` (needed for copy/paste between documents), and state-tracking properties like `IsWriteEnabled`, `IsTransactionResident`, `IsUndoing`, and `IsCancelling`. Implement the critical subset (`UpgradeOpen`, `DowngradeOpen`, `HandOverTo`, `Cancel`, `IsWriteEnabled`) and leave the rest as stubs for future undo/wblock support.

## Implement `LinetypeTable` and `LinetypeTableRecord` (priority: medium)

Currently entity linetypes are stored as plain strings (`"ByLayer"`, `"Continuous"`, etc.) directly on `Entity.Linetype` and resolved ad-hoc in the ACadSharp converter, with no database-side registry. Create `LinetypeTable : SymbolTable<LinetypeTableRecord>` and `LinetypeTableRecord : SymbolTableRecord` in `NormalCAD.Core.DatabaseServices`, following the same pattern as `LayerTable`/`LayerTableRecord`. Each `LinetypeTableRecord` should store the linetype name, description, and a pattern definition (dash lengths, dots, text, shapes) compatible with DXF group codes 49/74/75. The `Database` should own a `LinetypeTable` property (defaulting to a table containing at least "ByLayer", "ByBlock", and "Continuous"), and the `EntityPropertyProvider` should query the linetype table dynamically to populate the `ComboValues` for the Linetype dropdown instead of using a hardcoded list. Depends on the `SymbolTable` base class being already in place.

## Full `Polyline` Bulge and Width Support (priority: medium)

The `Polyline` entity already stores per-vertex `Bulge`, `StartWidth`, and `EndWidth` in its internal vertex struct (round-tripped through `PolylineConverter`), but these values currently have no effect on geometry or rendering — every segment is treated as a straight, zero-width line. Extend the whole pipeline so bulge and width are fully honored:

- **Geometry** — when a vertex has a non-zero bulge, `GetGeometricCurve()` must emit a `CircularArc3d` for that segment (deriving center, radius, and start/end angles from the bulge value) instead of a `LineSegment3d`, so length, area, closest-point, intersection, and osnap all follow the true arc. Midpoint osnap and grip/stretch behavior should account for the arc.
- **Renderer** — `PolylineRenderer` must draw arc segments for bulged vertices and render segment widths (tapered/constant thickness ribbons) instead of hairlines, honoring `ConstantWidth` and per-vertex `StartWidth`/`EndWidth`.
- **Extents** — `GeometricExtents` must include the arc bulge of each segment, not just the vertex positions, so bounds are not under-computed.
- Remove the "does NOT yet affect …" caveat comment from the `Vertex` struct once these paths are implemented.

The provider palette fields for bulge/width already exist, so this item is purely about making the stored data drive geometry and rendering.

## Fix `DispatcherTimer` Leaks (priority: medium)

`MainWindow.OnSidebarPointerExited` creates a new `DispatcherTimer` instance every time the pointer leaves the sidebar area, and `BottomBar.ShowFloatingPrompt` / `HideFloatingPrompt` each create new timers on every call. Over a single editing session, dozens of orphaned timer instances accumulate — each still wired to its Tick handler via closure, preventing garbage collection. Create the timers once in the constructor of each class, store them as instance fields, and reuse them via `Start()` / `Stop()` with updated intervals or callbacks as needed.

## Extract `GetModelSpace()` Helper (priority: medium)

The pattern `db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt` followed by `bt[BlockTableRecord.ModelSpace]` and another `TryGetObject` to get `BlockTableRecord btr` appears 8 times across `BaseCommand`, `EraseCommand`, `CleanAllCommand`, `DrawingService`, `TableParsers`, `CadViewport`, and `CadController`. Add an extension method `Database.TryGetModelSpace(out BlockTableRecord)` in the Core assembly. This reduces duplication and provides a single point of change if model space retrieval logic ever needs adjustment.

## Language Switcher and `CmdManager` Reindex (priority: medium)

Implement a runtime language switching mechanism: a UI entry point (menu item, command, or settings option), persistent storage of the chosen culture in application settings, and application of the culture via `Thread.CurrentThread.CurrentUICulture` and `CultureInfo.DefaultThreadCurrentUICulture`. Since all command strings are now expression-bodied properties backed by `ResourceManager.Get()`, the resource lookup will automatically resolve to the new culture for any string accessed after the switch. However, `CmdManager`'s internal command dictionary was populated with `cmd.LocalName` and `cmd.Alias` values from the previous culture at startup, so typed command names in the new language will not resolve. Extend `CmdManager` with a `RebuildIndex()` method that clears `_commands` and re-registers every `ICadCommand` instance — since `LocalName` and `Alias` are live properties, re-reading them picks up the new culture. Call this method after every language switch.

## Active Document Switching (priority: medium)

Allow users to create, open, and switch between multiple documents within a single application session, managed by `Application.DocumentManager`. Requires: UI for displaying open documents (tabs, a window menu, or a document list dropdown), per-document viewport state save and restore (camera position, zoom, active layer, selection set), ensuring all subsystems (`DrawingService`, `PropertyPalette`, `LayerPalette`, `InputManager`, active command) respond correctly to a document transition, and handling the edge case where the last document is closed (return to a "no document" startup state). Depends on the "Centralize Document and Database Access" item so that consumers do not hold stale references.

## `LayerPalette` — Use `ObservableCollection` (priority: medium)

`LayerPalette.OnDatabaseChanged` rebuilds the entire layer list from scratch and reassigns `ItemsSource` on every database change event (adding a layer, modifying a layer, loading a file). This discards the user's scroll position, collapses any expanded UI elements, and deselects whatever the user had selected. Replace with an `ObservableCollection<LayerItem>` bound to the ListBox via compiled binding. Instead of reconstructing the collection, mutate it: add/remove individual items, update properties in place. The ListBox will reflect changes incrementally without resetting its view state.

## Active Space Switching (Model / Paper Space) (priority: medium)

Allow users to toggle between model space and paper space layouts within a document. Requires: reading `BlockTableRecord.IsPaperSpace` to distinguish spaces, exposing paper space block records alongside model space in the UI (a tab bar or dropdown), switching the active `BlockTableRecord` in `CadController` so all entity operations target the correct space, and adjusting viewport rendering per space type (model space: infinite grid, world coordinates; paper space: sheet boundary, layout-relative coordinates, viewport objects displayed as clipped windows into model space). Implementation should also handle the case where a document has no paper space layouts defined yet.

## Commands.resx Portuguese Localization (priority: low)

Create `NormalCAD\Resources\Commands.pt-BR.resx` with Portuguese (Brazil) translations for all 43 keys defined in the neutral `Commands.resx`. This includes translating command prompts (`CIRCLE.PROMPT.CENTERPOINT` to `"Especifique o ponto central do círculo "`), status messages (`CLEANALL.MSG.CLEARED` to `"Desenho limpo com sucesso."`), keywords (`PLINE.KEY.CLOSE` to `"Fechar"`), and adding locale-specific aliases not present in the English resource (`THEME.ALIAS` to `"TEMA,TH"`, `QUIT.ALIAS` to `"SAIR,Q,EXIT"`). Depends on the Language Switcher for end-to-end validation of the resource fallback chain.

## Move `CadCursorState` to Controller (priority: low)

The `CadCursorState` enum (`PickCross`, `Crosshair`) is defined in the `View.Controls` namespace alongside UI controls, but it is referenced by every `ICadCommand` implementation in the `Controller` namespace — inverting the dependency direction (Controller should not depend on View). Move it to the Controller namespace or a shared enums location so that the Controller layer does not import View types.

## Rename Inconsistencies (priority: low)

Three naming issues create unnecessary confusion: `CmdManager` abbreviates "Command" while `InputManager` in the same namespace does not abbreviate — rename to `CommandManager` for consistency. `CleanAllCommand` uses "Clean" while `EraseCommand` uses "Erase" — the AutoCAD convention is ERASE, so rename to `EraseAllCommand` to keep the family consistent (future `WipeoutCommand`, `DeleteCommand`, etc. will benefit from clear naming). `NormalCAD.Core.DatabaseServices.Culture` is a static geometry parse utility that has no dependency on the database layer and shadows `System.Globalization.CultureInfo` — move it to the Core root namespace or a `Utilities` sub-namespace and rename to something unambiguous like `ParseUtility` or `InvariantParseHelper`.

## Change `Alias` to `IReadOnlyList<string>` (priority: low)

Currently `ICadCommand.Alias` is a comma-separated string (`"C,CI"`, `"EXIT,Q"`, `"TEMA,TH"`) parsed by `CmdManager.RegisterCommand` via `Split(',')`, `Trim()`, and empty-string filtering. This pushes parsing and validation logic into the command manager instead of keeping it with the command that defines the aliases. Change the property type to `IReadOnlyList<string>` so each command provides a clean, pre-parsed list. The `CmdManager` simply iterates the list without string manipulation.

## Remove Unused `IConverter` Properties (priority: low)

`IConverter.CanConvertToAcad` and `IConverter.CanConvertToNormal` return `true` in every implementation (`EntityConverter`, `ArcConverter`, `CircleConverter`, `LineConverter`, `PolylineConverter`, `BlockReferenceConverter`, `LayerConverter`, `VPortConverter`) and are never referenced anywhere in the `ConverterService` dispatch logic. They suggest a filtering mechanism that was planned but never implemented. Remove the two properties from the interface and all eight implementations to reduce noise.
