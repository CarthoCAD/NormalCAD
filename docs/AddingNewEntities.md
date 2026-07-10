# Adding a New Entity

This guide describes every file and change required to add support for a new
selectable, drawable entity to NormalCAD. Use it as a checklist whenever a new
entity type (Ellipse, Spline, MText, BlockReference, ...) is added.

The work spans two projects:

- **`NormalCAD.Core`** — the UI-agnostic data model. It must mirror the
  `Autodesk.AutoCAD.DatabaseServices` API as closely as possible. It has **no**
  dependency on Avalonia, ACadSharp, or any presentation concern.
- **`NormalCAD`** — the application (Avalonia UI, commands, converters,
  renderers, property providers).

A fully supported entity has **four pillars** in the application project
(Renderer, Provider, Converter, Draw command) on top of its **Core model**.

---

## 1. Core model — `NormalCAD.Core`

### 1.1 Entity class

Create the entity in `NormalCAD.Core/DatabaseServices/<Entity>.cs`.

- Namespace: `NormalCAD.Core.DatabaseServices` (folder must match the namespace).
- Derive from `Entity`, or from `Curve` if it is a curve-like entity
  (line, arc, circle, polyline). `Curve` adds `Length`, `Closed`, `Area`,
  `StartPoint`, `EndPoint`, `GetPointAtDist`, `GetClosestPointTo`, etc.
- **Match the AutoCAD .NET API signature** for this entity as closely as
  possible: same property names, same constructors, angles stored in **radians**,
  no UI metadata (no `System.ComponentModel` attributes — those live in the
  provider). Members that differ from the .NET API should be refactored to match.
- Implement the required overrides:
  - `Clone()` — deep copy (remember to copy entity-specific fields; base
    properties are copied by `CopyEntityPropertiesTo`).
  - `TransformBy(Matrix3d)`.
  - `GeometricExtents` — the world-space bounding box (used for culling and
    zoom-extents; if this is empty the entity may never render).
  - `GetGeometricCurve()` — returns a `Curve3d` primitive
    (`LineSegment3d`, `CircularArc3d`, `CompositeCurve3d`, ...). This single
    method powers hit-testing, intersection, distance, and area, so implement
    it faithfully.
  - Snapping / editing: `GetOsnapPoints()`, `GetGripPoints()`,
    `MoveGripPointsAt()`, `GetStretchPoints()`, `MoveStretchPointsAt()`.
  - `List()` — debug dump.

### 1.2 Supporting geometry (only if needed)

If the entity needs a geometric primitive that does not yet exist, add it under
`NormalCAD.Core/Geometry/` deriving from `Curve3d`. Existing primitives:
`LineSegment3d`, `CircularArc3d`, `CompositeCurve3d`.

### 1.3 Tests

Add unit tests under `NormalCAD.Tests/Core/` (geometry math, area, length,
intersection, etc.). Run with `dotnet test NormalCAD.Tests/NormalCAD.Tests.csproj`.

---

## 2. Renderer — `NormalCAD/View/Drawing`

Draws the entity on the Avalonia canvas.

1. Create `NormalCAD/View/Drawing/<Entity>Renderer.cs` implementing
   `IEntityRenderer`:

   ```csharp
   public void Render(DrawingContext context, Entity entity, Pen pen,
                      Func<Point3d, Point> worldToScreen, double zoom)
   ```

   Cast `entity` to your concrete type, convert world points to screen points
   with `worldToScreen`, and draw with the supplied `pen`.
2. Register it in `DrawingService` (constructor):

   ```csharp
   Register<MyEntity>(new MyEntityRenderer());
   ```

   `DrawingService.DrawEntity` dispatches by `entity.GetType()`, so an
   unregistered type is silently not drawn.

---

## 3. Property provider — `NormalCAD/Controller/Providers`

Feeds the `PropertyPalette`. Property metadata is a UI concern and must **not**
live in Core.

 1. Create `NormalCAD/Controller/Providers/<Entity>PropertyProvider.cs`
    implementing `IEntityPropertyProvider`:

    ```csharp
    public string DisplayName => EntityPropertyResources.Get("<ENTITY>.DISPLAYNAME");
    public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
    ```

    - Guard with `if (entity is not MyEntity e) yield break;`.
    - `yield return new PropertyDescriptor { ... }` for each property, matching
      the order and labels of the AutoCAD properties palette for that entity.
    - Each descriptor carries `Category`, `DisplayName`, `PropertyType`, `Order`,
      `GetValue`, optional `TrySetValue` (omit or `null` for read-only; avoid
      setting `IsReadOnly` explicitly — it is computed from `TrySetValue == null`),
      optional `ComboOptions`, and `SingleSelectionOnly` (set `true` for
      properties that make no sense in a multi-selection, e.g. per-vertex data).
    - Convert units at this boundary (e.g. radians → degrees via
      `NormalCAD.Utilities.AngleConverter`); Core stays in radians.
 2. Add a `<ENTITY>.DISPLAYNAME` key to `NormalCAD/Resources/EntityProperties.resx`
    (e.g. `"MyEntity"`).
 3. If the entity exposes enum or value-list properties, create an option provider
    under `NormalCAD/Controller/Providers/` returning `IReadOnlyList<ComboOption>`,
    add display value keys to `NormalCAD/Resources/ComboOptions.resx`, and set
    `ComboOptions` on the descriptor instead of `ComboValues`.
 4. Register it in `EntityPropertyManager` (constructor):

   ```csharp
   Register<MyEntity>(new MyEntityPropertyProvider());
   ```

   The manager always prepends the common `EntityPropertyProvider` (General
   category: Color, Layer, Linetype, ...), so only add entity-specific
   (Geometry/Misc) properties here.

The palette rebuilds automatically on selection change and merges shared
properties across a multi-selection (`GetMergedProperties`), showing
`*VARIES*` when values differ and skipping `SingleSelectionOnly` descriptors.

---

## 4. Converter — `NormalCAD/Controller/Services/Converters`

Round-trips the entity to/from ACadSharp for DXF/DWG import/export.

1. Create `NormalCAD/Controller/Services/Converters/<Entity>Converter.cs`
   deriving from `EntityConverter<TNormal, TAcad>`:

   ```csharp
   public class MyEntityConverter : EntityConverter<MyEntity, ACadSharp.Entities.MyAcadEntity>
   {
       public override ACadSharp.Entities.MyAcadEntity ConvertToAcad(MyEntity source, CadDocument cadDoc) { ... }
       public override MyEntity ConvertToNormal(ACadSharp.Entities.MyAcadEntity source) { ... }
   }
   ```

   - Call `ApplyEntityPropertiesToAcad` / `ApplyEntityPropertiesToNormal` to
     copy the common properties (layer, color, linetype, lineweight,
     transparency, visibility).
   - Map every entity-specific field (and keep angle units consistent — both
     ACadSharp and Core store angles in radians).
2. Register it in `ConverterService` (constructor):

   ```csharp
   Register(new MyEntityConverter());
   ```

   Dispatch is by concrete type in both directions; an unregistered type is
   skipped during import/export.

---

## 5. Draw / create command — `NormalCAD/Controller/Commands`

Lets the user create the entity interactively.

1. Create `NormalCAD/Controller/Commands/<Verb>Command.cs` implementing
   `ICadCommand` (`Name`, `LocalName`, `Alias`, `IsInternal`, `Activate`,
   `Deactivate`, `OnPointerPressed`, `OnPointerMoved`, `OnKeyDown`).
   - Use `CadCursorState.Crosshair` while active and restore `PickCross` in
     `Deactivate`.
   - Drive a live preview via `_controller.Viewport.ActiveCommandPreview`
     (assign a Core entity instance; reuse a single instance and mutate it
     rather than recreating each frame).
   - Set prompts/keywords through `InputManager.SetCurrentPrompt`.
   - On completion, add the entity with
     `_controller.AddNewEntityToActiveSpace(entity)` and return to idle with
     `_controller.SetCommand(new BaseCommand())`.
2. Command discovery is automatic: `CmdManager.DiscoverCommands` reflects over
   every `ICadCommand` in the assembly and registers `Name`, `LocalName`, and
   comma-separated `Alias`. No manual registration is needed.
3. Add user-facing strings (command name, aliases, prompts, keywords) to
   `NormalCAD/Resources/Commands.resx` (and localized variants) and read them
   through `CommandResources.Get(...)`.
4. Evaluate whether the command needs a matching `MenuBar` entry, preferring to
   add one. When added, the menu item must invoke the command by its invariant
   `Name`.

---

## Checklist

Core (`NormalCAD.Core`):

- [ ] `DatabaseServices/<Entity>.cs` — API-faithful entity model
- [ ] Geometry primitive under `Geometry/` (only if a new one is needed)
- [ ] Unit tests under `NormalCAD.Tests/Core/`

Application (`NormalCAD`):

- [ ] `View/Drawing/<Entity>Renderer.cs` + register in `DrawingService`
- [ ] `Controller/Providers/<Entity>PropertyProvider.cs` (with `DisplayName`,
      `ComboOptions` for enum props, no explicit `IsReadOnly`) + register in `EntityPropertyManager`
- [ ] `EntityProperties.resx` `<ENTITY>.DISPLAYNAME` key
- [ ] `ComboOptions.resx` keys for any combo/enum values + `Controller/Providers/<Entity>OptionProvider.cs`
- [ ] `Controller/Services/Converters/<Entity>Converter.cs` + register in `ConverterService`
- [ ] `Controller/Commands/<Verb>Command.cs` (auto-discovered) + `Commands.resx` strings
- [ ] `MenuBar` entry (evaluate the need, prefer adding one; invoke by invariant `Name`)

Verify with `dotnet build NormalCAD.sln` and
`dotnet test NormalCAD.Tests/NormalCAD.Tests.csproj`.
