# Backlog

Completed items are removed from this backlog. See git history and closed issues for delivered work.

## Complete `BlockReference` Entity Pipeline (priority: high)

`BlockReference` exists in `NormalCAD.Core` but is not yet a fully selectable, drawable entity — it must be wired through the complete entity pipeline (see `AddingNewEntities.md` for the full step-by-step). Currently `BlockReference` and its nested sub-entities are not rendered in the viewport, making block insertion functionally invisible. Deliver every stage of the pipeline for this entity:

- **Renderer** — add a `BlockReferenceRenderer : IEntityRenderer` and register it in `DrawingService`, transforming and drawing each nested sub-entity through the block transform. Investigate the current failure: the cause could be in `DrawingService.DrawEntity` (entity type dispatch may not handle `BlockReference`), in `BlockReference.GetGeometricCurve()` / `GeometricExtents` (computing empty bounds that get culled), or in the renderer's coordinate transform chain for nested entities.
- **Provider** — add a `BlockReferencePropertyProvider : IEntityPropertyProvider` and register it in `EntityPropertyManager`, exposing the AutoCAD INSERT palette properties (Position X/Y/Z, Scale X/Y/Z, Rotation, Block Name, etc.).
- **Converter** — verify/fix `BlockReferenceConverter` so the DWG/DXF reader correctly populates sub-entities and the block transform on round-trip.
- **Draw command** — implement the `INSERT` command (`InsertCommand : ICadCommand`) that lets the user pick a block, place it interactively (with live preview), and set rotation/scale, following the same interactive pattern as the other drawing commands.

This is the first end-to-end exercise of the "add a new entity" pipeline against an already-modeled Core entity, so it doubles as validation of `AddingNewEntities.md`.

## Undo System (priority: high)

Implement a full undo/redo stack using the AutoCAD command-group model: each interactive command or immediate action registers an `UndoGroup` that wraps the set of database modifications it performs. The `TransactionManager` must track object state snapshots (before/after values for modified properties, or pre-modification clones for structural changes like adding/removing entities) so that undo can restore them. The undo stack is managed per-document by the `Database`, with `Undo()` and `Redo()` methods exposed through the `Editor`. A `NoUndoMarker` flag on commands (already reserved in the planned `CommandFlags`) should suppress undo recording for non-destructive operations like ZOOM, REGEN, or inquiry commands. Depends on the `ICadCommand` refactoring (to add `CommandFlags.NoUndoMarker`) and on the idle state extraction (so `BaseCommand` doesn't interfere with undo group boundaries).

## Complete Default Grip System (priority: high)

Entities already report their grip points via `GetGripPoints()` and the idle state already handles entity selection, but grips are not yet rendered or interactive. Implement the full AutoCAD-style grip system:

- **Grip rendering** — when an entity is selected (grip-mode / idle state with a selection), render small colored squares at each grip point returned by `GetGripPoints()`. Line endpoints, circle center/quadrants, arc endpoints/midpoint, and polyline vertices are the baseline. Grips should be drawn on top of all entities, with the hovered grip rendering in a distinct color (pink → red, matching the AutoCAD convention).
- **Grip hover detection** — on pointer move, hit-test against the screen-space rectangles of all visible grips. Update the hovered grip index and redraw.
- **Grip drag (stretch)** — on pointer-press over a grip, enter a stretch mode where pointer-move updates the entity geometry in real time through the transaction system, committing on pointer-release. Stretching a line endpoint, circle radius change via quadrant grip, and polyline vertex relocation are the essential first set.
- **Multi-grip selection** — holding Shift while clicking a grip adds it to a hot-grip set; the stretch operation then deforms the entity from all hot grips simultaneously.
- **Grip-mode cursor** — switch to `PickfirstOrGrips` cursor when grips are visible (no active command, entity selected).
- **Right-click grip menu** — (future / stub) a context popup offering stretch/move/rotate/scale/mirror options for the selected grip set.

The `GetGripPoints()` API, the idle selection infrastructure, and the `CadCursorState` → `CursorType` migration are already in place, so this item is about rendering, hit-testing, and the stretch transaction loop.

## Fix i18n of `PropertyPalette` Combo Box Display Values (priority: high)

The combo box editors in the property palette (LineWeight, Linetype, the boolean Yes/No, and `ByLayer`/`ByBlock` values) appear not to localize correctly — verify and fix. Confirm that each `ComboOption.DisplayName` resolves to the current UI culture and, crucially, that the displayed text updates when the language is switched at runtime. Likely areas to inspect: the option providers (`LineWeightOptionProvider`, `LinetypeOptionProvider`) and the generated boolean options; the `ComboOptions.resx` / `EntityProperties.resx` keys behind them; whether option lists or their display strings are cached/built once instead of re-read per culture; and whether the palette re-projects its rows on `LanguageService.LanguageChanged` so the combos rebuild with new-culture display names (the selected item must also re-match after the rebuild). Add a check that a language switch with an entity selected refreshes every combo's text.

## Cache Brushes and Pens in `DrawingService` (priority: medium)

`DrawingService.DrawEntity` allocates a new `SolidColorBrush` and a new `Pen` for every entity on every frame. With 1000 entities at 60fps, that is approximately 120,000 allocations per second just for rendering brushes and pens, driving significant GC pressure. Cache brushes and pens in a `ConcurrentDictionary` keyed by `(Avalonia.Media.Color, double thickness, DashStyle?)` or similar tuple. Invalidate and clear the cache on theme change (Light ↔ Dark), since theme tokens resolve to different colors.

## DBObject API Compatibility (priority: medium)

The `DBObject` class in `NormalCAD.Core` currently exposes only 7 of the 16 properties and 1 of the 11 methods defined in the AutoCAD .NET `DBObject` base class. The most impactful gap is `UpgradeOpen()` (promote from `ForRead` to `ForWrite` within a transaction) — without it, any code that obtains an object as read-only must re-open it for write access, adding boilerplate and risking stale references. Also missing: `DowngradeOpen()`, `Cancel()`, `HandOverTo(ObjectId)` (transfer ownership, needed for moving entities between block records), `DeepClone(...)` (needed for copy/paste between documents), and state-tracking properties like `IsWriteEnabled`, `IsTransactionResident`, `IsUndoing`, and `IsCancelling`. Implement the critical subset (`UpgradeOpen`, `DowngradeOpen`, `HandOverTo`, `Cancel`, `IsWriteEnabled`) and leave the rest as stubs for future undo/wblock support.

## Implement `LinetypeTable` and `LinetypeTableRecord` (priority: medium)

Currently entity linetypes are stored as plain strings (`"ByLayer"`, `"Continuous"`, etc.) directly on `Entity.Linetype` and resolved ad-hoc in the ACadSharp converter, with no database-side registry. Create `LinetypeTable : SymbolTable<LinetypeTableRecord>` and `LinetypeTableRecord : SymbolTableRecord` in `NormalCAD.Core.DatabaseServices`, following the same pattern as `LayerTable`/`LayerTableRecord`. Each `LinetypeTableRecord` should store the linetype name, description, and a pattern definition (dash lengths, dots, text, shapes) compatible with DXF group codes 49/74/75. The `Database` should own a `LinetypeTable` property (defaulting to a table containing at least "ByLayer", "ByBlock", and "Continuous"), and the `EntityPropertyProvider` should query the linetype table dynamically to populate the `ComboOptions` for the Linetype dropdown via `LinetypeOptionProvider`. Depends on the `SymbolTable` base class being already in place.

## Full `Polyline` Bulge and Width Support (priority: medium)

The `Polyline` entity already stores per-vertex `Bulge`, `StartWidth`, and `EndWidth` in its internal vertex struct (round-tripped through `PolylineConverter`), but these values currently have no effect on geometry or rendering — every segment is treated as a straight, zero-width line. Extend the whole pipeline so bulge and width are fully honored:

- **Geometry** — when a vertex has a non-zero bulge, `GetGeometricCurve()` must emit a `CircularArc3d` for that segment (deriving center, radius, and start/end angles from the bulge value) instead of a `LineSegment3d`, so length, area, closest-point, intersection, and osnap all follow the true arc. Midpoint osnap and grip/stretch behavior should account for the arc.
- **Renderer** — `PolylineRenderer` must draw arc segments for bulged vertices and render segment widths (tapered/constant thickness ribbons) instead of hairlines, honoring `ConstantWidth` and per-vertex `StartWidth`/`EndWidth`.
- **Extents** — `GeometricExtents` must include the arc bulge of each segment, not just the vertex positions, so bounds are not under-computed.
- Remove the "does NOT yet affect …" caveat comment from the `Vertex` struct once these paths are implemented.

The provider palette fields for bulge/width already exist, so this item is purely about making the stored data drive geometry and rendering.

## Fix `DispatcherTimer` Leaks (priority: medium)

`MainWindow.OnSidebarPointerExited` creates a new `DispatcherTimer` instance every time the pointer leaves the sidebar area, and `BottomBar.ShowFloatingPrompt` / `HideFloatingPrompt` each create new timers on every call. Over a single editing session, dozens of orphaned timer instances accumulate — each still wired to its Tick handler via closure, preventing garbage collection. Create the timers once in the constructor of each class, store them as instance fields, and reuse them via `Start()` / `Stop()` with updated intervals or callbacks as needed.

## Active Document Switching (priority: medium)

Allow users to create, open, and switch between multiple documents within a single application session, managed by `Application.DocumentManager`. Requires: UI for displaying open documents (tabs, a window menu, or a document list dropdown), per-document viewport state save and restore (camera position, zoom, active layer, selection set), ensuring all subsystems (`DrawingService`, `PropertyPalette`, `LayerPalette`, `InputManager`, active command) respond correctly to a document transition, and handling the edge case where the last document is closed (return to a "no document" startup state). Depends on the "Centralize Document and Database Access" item so that consumers do not hold stale references.

## Refactor `LayerPalette` — inline editor, color picker, filtering, `ObservableCollection` (priority: medium)

Evolve the layer manager while keeping the current shape (a simple list of layers with name and color). Four parts:

- **`ObservableCollection` + virtualization** (absorbs the former standalone item): `LayerPalette.OnDatabaseChanged` rebuilds the whole list and reassigns `ItemsSource` on every database change, discarding scroll position, expanded rows, and selection. Replace with an `ObservableCollection<LayerItem>` bound via compiled binding and mutated in place (add/remove/update), and ensure the `ListBox` virtualizes so drawings with a large number of layers stay responsive.
- **Functional color swatch**: make the color button open the Color Selection Dialog (see the related item) and apply the chosen color to the `LayerTableRecord` inside a transaction (with `DocumentLock`).
- **Per-row inline property editor**: add an edit/expand toggle (`+`) on each row that reveals, below the layer line, an inline panel showing that layer's properties using the **same grouped structure/DataTemplates as the `PropertyPalette`**. This is the first reuse of the palette's presentation: extract an `IPropertySource` abstraction from `EntityPropertyManager`, add a `LayerPropertyProvider` that produces the layer's `PropertyDescriptor`s (Name, Color, Linetype, Lineweight, On/Off, Freeze, Lock...), and move the row/group `DataTemplate`s into a shared `ResourceDictionary` used by both palettes. Build the groups lazily on expand. Color can become a new `EditorKind.Color` with a picker template. Layer-specific validation (unique name, layer "0" not renamable) is enforced by `TrySetValue` (invalid input auto-reverts, as in the palette).
- **Create + filter text box**: the top text box doubles as create (via `+`) and live filter by name — typing filters the list, `+` creates the layer with the typed name. Essential for CAD files with a huge number of layers.

Depends on generalizing `PropertyPalette` into an `IPropertySource` and on the Color Selection Dialog item.

## AutoCAD-style Color Selection Dialog exposed through Core (priority: medium)

Create a color-selection dialog matching AutoCAD's "Select Color" (index color / true color, plus `ByLayer`/`ByBlock`), and expose access to it **indirectly through `NormalCAD.Core`**, mirroring how the AutoCAD .NET API surfaces `Autodesk.AutoCAD.Windows.ColorDialog` without the caller depending on the UI toolkit. Follow the existing host-abstraction pattern (`IApplicationHost` / `Application.ShowAlertDialog`): add a method on the host interface (e.g. `ShowColorDialog(EntityColor current) : EntityColor?`) that Core-side code and commands can call, with the `NormalCAD` application project providing the actual Avalonia dialog implementation and returning the chosen `EntityColor` (or null on cancel). Consumed by the `LayerPalette` color swatch and by entity color editing in the property palette.

## Active Space Switching (Model / Paper Space) (priority: medium)

Allow users to toggle between model space and paper space layouts within a document. Requires: reading `BlockTableRecord.IsPaperSpace` to distinguish spaces, exposing paper space block records alongside model space in the UI (a tab bar or dropdown), switching the active `BlockTableRecord` so all entity operations target the correct space, and adjusting viewport rendering per space type (model space: infinite grid, world coordinates; paper space: sheet boundary, layout-relative coordinates, viewport objects displayed as clipped windows into model space). Implementation should also handle the case where a document has no paper space layouts defined yet.

## Refactor `CadController` — Decompose God Class (priority: medium)

`CadController` is an orchestration hub that accumulated responsibilities from four distinct AutoCAD .NET API roles: `Application` (bootstrap/document lifecycle), `Editor` (selection/session state), `Document` (viewport persistence), and UI glue (theme, input pass-through). This blocks independent testing and forces every new feature through a single 200+ line class.

Break it down:

- **Theme** — extract `ApplyTheme` into a `ThemeService`. `MainWindow` calls it directly; `CadController` drops `IsLightTheme` and `ApplyTheme`.
- **Viewport persistence** — move `SaveViewportState`/`RestoreViewportState` into `CadCoreHelper` or trigger automatically on `SetDocument`.
- **Selection + session state** — migrate `_selectedEntityIds`, `ActiveLayer`, `ActiveColor`, and selection events to `Editor` (depends on "Extract Idle State from `BaseCommand`").
- **Input pass-through** — remove `OnPointerPressed`/`OnPointerMoved`/`OnKeyDown` indirection (depends on "Refactor Command Input System").

After decomposition, `CadController` should be a thin facade coordinating `CmdManager`, `InputManager`, and `EntityPropertyManager`, with no direct database access, no session state, and no UI concern.

## Refactor `DrawingService.DrawEntity` Preview/Selection Handling (priority: low)

Decouple visual state (selection highlight, preview, rubberband) from `DrawingService.DrawEntity` by introducing `ApplySelection` and `ApplyPreview` helpers that return modified entity clones. `DrawEntity` should then render entities purely from their own properties (`Color`, `Layer`, `LineWeight`, `Linetype`) without boolean flags for selection/preview. This aligns with the AutoCAD .NET API where visual overrides are applied to temporary clones rather than passed as render flags. Depends on `LineWeight` being honored by the renderer and `Linetype` supporting dashed patterns; until then, keep the current flag-based approach.

## Rename Inconsistencies (priority: low)

Three naming issues create unnecessary confusion: `CmdManager` abbreviates "Command" while `InputManager` in the same namespace does not abbreviate — rename to `CommandManager` for consistency. `CleanAllCommand` uses "Clean" while `EraseCommand` uses "Erase" — the AutoCAD convention is ERASE, so rename to `EraseAllCommand` to keep the family consistent (future `WipeoutCommand`, `DeleteCommand`, etc. will benefit from clear naming). `NormalCAD.Core.DatabaseServices.Culture` is a static geometry parse utility that has no dependency on the database layer and shadows `System.Globalization.CultureInfo` — move it to the Core root namespace or a `Utilities` sub-namespace and rename to something unambiguous like `ParseUtility` or `InvariantParseHelper`.

## Make PLINE Command Use Database-Resident Polyline (priority: low)

Currently `DrawPolylineCommand` builds a transient `Polyline` object that is only committed to the database after the command finishes. This prevents the in-progress polyline from being interactive — snap points on its own geometry are unavailable, and the entity is invisible to spatial queries during editing. The `LINE` command does not have this limitation because it consolidates each segment as a database-resident entity immediately.

Change the PLINE command so that the polyline is added to the database as soon as the first vertex is placed, and updated in-place (vertex add/remove) through the transaction system as the command progresses. This makes the polyline discoverable by snap, grip, and spatial queries for its own geometry during construction, matching the AutoCAD behavior where PLINE offers endpoint/intersection osnap to its own segments while the command is still active.

## Remove Unused `IConverter` Properties (priority: low)

`IConverter.CanConvertToAcad` and `IConverter.CanConvertToNormal` return `true` in every implementation (`EntityConverter`, `ArcConverter`, `CircleConverter`, `LineConverter`, `PolylineConverter`, `BlockReferenceConverter`, `LayerConverter`, `VPortConverter`) and are never referenced anywhere in the `ConverterService` dispatch logic. They suggest a filtering mechanism that was planned but never implemented. Remove the two properties from the interface and all eight implementations to reduce noise.

## Additional Language Resources — Spanish, Mandarin, Japanese (priority: low)

Add satellite `.resx` translations for Spanish (`es`), Mandarin Chinese (`zh-CN`), and Japanese (`ja`), mirroring the existing `pt-BR` set across `Commands`, `Panels`, `Dialogs`, `EntityProperties`, and `ComboOptions`. Each new culture must cover every key in the neutral resources, keeping command `LOCALNAME`/`ALIAS` values ASCII where the language allows and otherwise following the conventions of that locale. Wire the new cultures into the language switcher so users can select them at runtime, and verify the resource fallback chain resolves correctly for each. The runtime switching, persistence (`config.json`), and live re-localization infrastructure is already in place, so this item is essentially translation content plus exposing the new options in the switcher.

## Convert `PropertyDescriptor` Items to Enums (priority: low)

Extend the enum-based localization strategy already used for property categories (`PropertyCategory` + `ResourcePrefixAttribute` + `LocalizedEnum`) to the property items themselves. Today each provider sets a localized `DisplayName` (via a per-item `=> Get("...")` string helper) and a manual `Order` index. Replacing those with one enum per (entity × category) group — e.g. `GeneralProperty`, `LineGeometryProperty`, `CircleGeometryProperty`, `ArcGeometryProperty`, `PolylineGeometryProperty`, `PolylineMiscProperty` — each tagged with a `ResourcePrefix`, would remove the per-item string helpers and the manual index (declaration order becomes the display order) and resolve both label and ordering through `LocalizedEnum`. `PropertyDescriptor.DisplayName`/`Order` would collapse into a single `Property` field of type `System.Enum`.

Note: the relevance of this change must be analyzed before committing to it. The behavioral logic (`GetValue`/`TrySetValue`/`PropertyType`/`ComboOptions`) stays in the providers, so the reduction is limited to labels and ordering, while the change introduces an implicit convention coupling enum member names to `.resx` key suffixes (best guarded by a unit test asserting every member resolves to a non-empty string in every supported culture). Weigh the boilerplate savings and added type-safety against the new infrastructure and coupling.

## Value Formatting & Units Service (priority: low)

Introduce a document-scoped service that converts values to/from their string representation according to the drawing's system variables — linear/angular unit type and precision, insertion units, and input tolerance (the equivalent of AutoCAD's `LUNITS`/`LUPREC`, `AUNITS`/`AUPREC`, `INSUNITS`). It must be the single source of truth shared by every consumer that shows or reads values — the property palette, the command line, dynamic input, dimensions, and inquiry commands — so a system-variable change (e.g. increasing precision) reformats everything consistently. `PropertyDescriptor.Format()`/`TryParse()` — the façade added by the PropertyPalette decoupling (#10) — should declare each value's semantic kind (`Distance`, `Angle`, `Area`, `Factor`, `Integer`, `String`, ...) plus its storage unit (e.g. angles in radians) and delegate the actual conversion to this service; this also removes the ad-hoc unit conversion currently scattered across providers (e.g. `AngleConverter` in `GetValue`/`TrySetValue`). Keep display precision (formatting) separate from geometric tolerance (comparison/snap): the service owns formatting and may round input, but equality-by-tolerance stays a geometry concern.
