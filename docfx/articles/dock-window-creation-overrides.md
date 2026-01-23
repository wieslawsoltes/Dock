# Window Creation Override Analysis

## Scope and goals
This analysis focuses on every place the code base creates an Avalonia `Window` (or a derived type) and how those creation points can be overridden to support a managed window system or custom window implementations. The goal is to make all window creation paths consistent and easy to override, especially for floating dock windows created via factories.

## Current window creation points

### Library defaults
- `DockControl.InitializeFactory` assigns `HostWindowLocator` and `DefaultHostWindowLocator` to `new HostWindow()` when no factory is provided. This is the primary source of floating window creation for docking. (`src/Dock.Avalonia/Controls/DockControl.axaml.cs`)
- `HostAdapter.Present` lazily requests a host window via `Factory.GetHostWindow(id)` when an `IDockWindow` is shown. (`src/Dock.Model/Adapters/HostAdapter.cs`)
- The diagnostics helper creates a debug window directly via `new Window` and does not use a factory or locator. (`src/Dock.Avalonia.Diagnostics/Controls/RootDockDebugExtensions.cs`)

### Internal helper windows (overlay/preview/pinned)
- `AdornerHelper<T>` creates a floating `DockAdornerWindow` when `DockSettings.UseFloatingDockAdorner` is enabled. (`src/Dock.Avalonia/Internal/AdornerHelper.cs`, `src/Dock.Avalonia/Controls/DockAdornerWindow.axaml.cs`, `src/Dock.Settings/DockSettings.cs`)
- `DragPreviewHelper` creates and manages a singleton `DragPreviewWindow` for drag previews. (`src/Dock.Avalonia/Internal/DragPreviewHelper.cs`, `src/Dock.Avalonia/Controls/DragPreviewWindow.axaml.cs`)
- `PinnedDockControl` creates a `PinnedDockWindow` when `DockSettings.UsePinnedDockWindow` is enabled. (`src/Dock.Avalonia/Controls/PinnedDockControl.axaml.cs`, `src/Dock.Avalonia/Controls/PinnedDockWindow.axaml.cs`, `src/Dock.Settings/DockSettings.cs`)

### App/sample entry points
- Code-only samples set `desktop.MainWindow = new Window { ... }`. (`samples/DockCodeOnlySample/Program.cs`, `samples/DockCodeOnlyMvvmSample/Program.cs`, `samples/DockFluentCodeOnlySample/Program.cs`)
- XAML samples declare `<Window ...>` roots and code-behind `MainWindow : Window` classes. (multiple `samples/**/MainWindow.axaml*`)

### Model/window separation
- Floating window instances are `IDockWindow` (model objects) created by `IFactory.CreateDockWindow`. These are not UI `Window` instances but control the `IHostWindow` that is created later. (`src/Dock.Model.Avalonia/Factory.cs` and similar in other model packages)

## Existing extension points
- `IFactory.HostWindowLocator` and `IFactory.DefaultHostWindowLocator` are the intended customization points for floating window creation. (`src/Dock.Model/FactoryBase.Locator.cs`)
- `FactoryBase.InitLayout` will create a default `HostWindowLocator` from `DefaultHostWindowLocator` if none was provided. (`src/Dock.Model/FactoryBase.Init.cs`)
- Custom `IHostWindow` implementations are supported, but most helpers assume an Avalonia `Window`/`TopLevel` underneath.

## Coupling to Avalonia.Window
These call sites assume concrete `Window` behavior, which means custom host windows should be `Window`-derived (or provide equivalents) to keep features working:
- Window activation ordering uses `Window.SortWindowsByZOrder` and `Window.Activate`. (`src/Dock.Avalonia/Internal/WindowActivationHelper.cs`)
- Z-ordering for docking controls uses `Window.SortWindowsByZOrder`. (`src/Dock.Avalonia/Internal/DockHelpers.cs`)
- Controls expect `TopLevel.GetTopLevel(this)` to be an `IHostWindow` (so host windows must be `TopLevel` at minimum). (`src/Dock.Avalonia/Controls/DockControl.axaml.cs` and related controls)
- Window drag logic and chrome integrations are specialized for `HostWindow` and `Window`. (`src/Dock.Avalonia/Internal/WindowDragHelper.cs`, `src/Dock.Avalonia/Controls/ToolChromeControl.axaml.cs`, `src/Dock.Avalonia/Controls/DocumentTabStrip.axaml.cs`)
- Overlay resolution in Avalonia services explicitly checks `HostWindow` to resolve services from the DataContext. (`src/Dock.Model.ReactiveUI.Services.Avalonia/Services/AvaloniaHostServiceResolver.cs`)
- Pinned dock window hosting assumes a `Window` owner and attaches to `Window` events. (`src/Dock.Avalonia/Controls/PinnedDockControl.axaml.cs`)

## How windows are used at runtime
This section maps the window lifecycle and the code paths that depend on a real OS `Window` vs a managed in-app window.

### Lifecycle and tracking
- `IDockWindow` is a model object; `IHostWindow` is the runtime UI host. `HostAdapter.Present` creates/attaches the host and pushes layout, size, and title. (`src/Dock.Model/Adapters/HostAdapter.cs`)
- `HostWindow` registers itself in `Factory.HostWindows` on open/close and drives `WindowOpened/Closed` events. (`src/Dock.Avalonia/Controls/HostWindow.axaml.cs`)
- `DockControl` wires the root window to the root dock and raises window lifecycle events when the main window closes. (`src/Dock.Avalonia/Controls/DockControl.axaml.cs`)

### Activation and z-order
- Global activation uses `Window.SortWindowsByZOrder` and `Window.Activate`. (`src/Dock.Avalonia/Internal/WindowActivationHelper.cs`)
- Dock control z-order computations assume OS windows and `Window.SortWindowsByZOrder`. (`src/Dock.Avalonia/Internal/DockHelpers.cs`)

### Dragging, resizing, and chrome
- Floating window dragging uses `HostWindowState` + `BeginMoveDrag` and assumes a concrete `HostWindow`. (`src/Dock.Avalonia/Internal/HostWindowState.cs`, `src/Dock.Avalonia/Controls/HostWindow.axaml.cs`)
- Tool/document chrome controls attach to `HostWindow` and use OS drag behavior for Windows/macOS, with a `WindowDragHelper` fallback on Linux. (`src/Dock.Avalonia/Controls/ToolChromeControl.axaml.cs`, `src/Dock.Avalonia/Internal/WindowDragHelper.cs`)
- Managed MDI windows already include drag/resize/min/max logic within `MdiDocumentWindow`, which does not require an OS `Window`. (`src/Dock.Avalonia/Controls/MdiDocumentWindow.axaml.cs`)

### Overlays, diagnostics, and helper windows
- Debug windows and overlay helpers create their own `Window` instances and rely on `TopLevel` to host overlays. (`src/Dock.Avalonia.Diagnostics/Controls/RootDockDebugExtensions.cs`, `src/Dock.Avalonia.Diagnostics/DockDebugOverlayManager.cs`)
- Floating adorner, drag preview, and pinned dock windows are implemented as OS windows. (`src/Dock.Avalonia/Internal/AdornerHelper.cs`, `src/Dock.Avalonia/Internal/DragPreviewHelper.cs`, `src/Dock.Avalonia/Controls/PinnedDockControl.axaml.cs`)

### Service resolution and data context
- Overlay services resolve `HostWindow` and its `DataContext` explicitly. Managed windows must surface equivalent data for overlays/services. (`src/Dock.Model.ReactiveUI.Services.Avalonia/Services/AvaloniaHostServiceResolver.cs`)

## Native vs managed window requirements
To ensure both OS windows and managed windows behave correctly, the same lifecycle contracts must be satisfied.

### Required capabilities (both modes)
- **Lifecycle**: `Present`/`Exit` must add/remove the hosted layout and raise window events consistently.
- **Tracking**: size and position must flow between `IDockWindow` and the host.
- **Activation**: active window tracking and activation events must match dock state (`ActiveDockable`, focus).
- **Ownership**: floating windows should respect owner/main window ordering and modal behavior where applicable.

### Native (OS) window specifics
- Use OS activation/z-order via `Window.SortWindowsByZOrder`, `Window.Activate`.
- Use OS move/resize via `BeginMoveDrag` and pointer capture.
- Use owner relationships for modal/float windows when `DockSettings.UseOwnerForFloatingWindows` is enabled.

### Managed (in-app) window specifics
- Replace OS z-order with a managed ordering (e.g., `MdiZIndex`) and update it on activation.
- Replace OS drag/resize with MDI logic (already in `MdiDocumentWindow` + `IMdiLayoutManager`).
- Provide a managed “host layer” that can be queried for current window order and activation state.
- Bridge data context/service resolution (overlay services should resolve from managed host/root).
- Ensure managed window templates mirror native window themes (title bar, borders, shadows, resize grips, chrome buttons).
- Ensure input routing (pointer capture, drag handles, resize grips) mirrors native behavior so managed windows feel identical.

## Gaps and friction points
- `DockControl.InitializeFactory` overwrites any `HostWindowLocator` or `DefaultHostWindowLocator` that might already be configured on the factory, making it hard to inject custom window types unless `InitializeFactory` is disabled.
- The diagnostics debug window is hardcoded as `new Window` and cannot be swapped without modifying code.
- Overlay/preview/pinned windows are hardcoded to `DockAdornerWindow`, `DragPreviewWindow`, and `PinnedDockWindow` with no factory hook.
- There is no single window factory abstraction shared across DockControl defaults, diagnostics, and other window creation sites.
- `HostWindowLocator` uses string IDs only; it cannot easily choose a window implementation based on richer context (e.g., tool window vs document window) unless IDs are manually encoded.
- Several runtime behaviors are hardcoded to OS windows (`HostWindowState`, z-order helpers, debug helpers, adorner/preview/pinned windows) and must be abstracted for managed windows.
- Overlay services and diagnostics assume `TopLevel`/`Window` roots, which do not exist for managed windows.

## Proposed solutions

### A. Add a host window factory hook to DockControl
- Introduce a property such as `Func<IHostWindow?> HostWindowFactory` (or `Func<string, IHostWindow?> HostWindowLocatorFactory`).
- In `InitializeFactory`, only assign `HostWindowLocator` or `DefaultHostWindowLocator` if they are null, and use `HostWindowFactory` when provided.
- Consider an overload that provides richer context, such as `Func<IDockWindow, IHostWindow?>`, so callers can map to different host window types without string IDs.

### B. Centralize window creation via a shared factory service
- Add an `IWindowFactory` (or `IHostWindowFactory`) in `Dock.Avalonia` and register a default implementation that returns `HostWindow`.
- Use this factory in DockControl defaults and in diagnostics (`RootDockDebugExtensions`), with optional overrides via DI or explicit parameters.

### C. Make diagnostics and helper windows overridable
- Add an overload to `AttachDockDebug` that accepts a `Func<Window>` or `Func<TopLevel, Window>`.
- Default to the current behavior when no delegate is provided, keeping compatibility.
- Provide similar factory hooks for `DockAdornerWindow`, `DragPreviewWindow`, and `PinnedDockWindow` (for example via `DockSettings`, a small `IWindowingServices` interface, or an `AppBuilder` extension).

### D. Managed window implementation (reuse MDI)
Goal: allow floating windows to be hosted inside the main window (managed/MDI-style) instead of creating OS windows, while preserving existing `IDockWindow` + `IHostWindow` plumbing.

Key idea: reuse the existing MDI system (`IMdiDocument`, `MdiLayoutPanel`, `IMdiLayoutManager`, `MdiDocumentWindow`) as the managed window surface, with a thin adapter so the `IHostWindow` contract can drive it.

Proposed shape:
- Add a `ManagedHostWindow : IHostWindow` that does **not** derive from `Window`. It inserts a managed “window” control into an in-app layer and mirrors size/position/title to `IMdiDocument` state.
- Add a `ManagedWindowLayer` (or similar) control that hosts MDI windows inside the main window. This can be an `ItemsControl` + `MdiLayoutPanel` pair, reusing existing `MdiDocumentWindow` templates.
- Provide a factory switch (`DockSettings.UseManagedWindows` or `IFactory.WindowingMode`) that swaps `HostWindowLocator`/`DefaultHostWindowLocator` from `HostWindow` to `ManagedHostWindow`.
- Adapt the MDI window control to be less `IDock`-specific:
  - Either introduce a small `IMdiHost` interface (ActiveItem/Items/InvalidateLayout) and use it instead of `IDock`.
  - Or provide a lightweight `ManagedWindowDock : IDock` implementation in `Dock.Avalonia` for the managed layer, so existing `MdiDocumentWindow` logic keeps working without major changes.

#### Managed window details (from analysis)
- **Reusable MDI components**: `IMdiDocument`, `MdiLayoutPanel`, `IMdiLayoutManager`, and `ClassicMdiLayoutManager` are already generic and can be reused as-is. (`src/Dock.Avalonia/Mdi/*`, `src/Dock.Model/Core/IMdiDocument.cs`)
- **Current coupling**: `MdiDocumentWindow` implements drag/resize/min/max logic, but assumes `IMdiDocument.Owner is IDock` and uses `IDock.ActiveDockable` for active state. (`src/Dock.Avalonia/Controls/MdiDocumentWindow.axaml.cs`)

#### Refactors needed for reuse
- **Decouple host assumptions**: introduce a small host contract (for example `IMdiHost` with `ActiveItem`, `Items`, and `InvalidateLayout`) or provide an adapter dock used only by managed windows.
- **Managed window layer**: add a `ManagedWindowLayer` control (standalone or inside `DockControl`) to host managed windows inside the main window.
- **Managed host implementation**: create an `IHostWindow` implementation that inserts a managed window control into the layer rather than spawning an OS window.

#### Minimal concrete approach
1. Create `ManagedWindowDock : IDock` (or a lighter `IMdiHost`) in `Dock.Avalonia` to own managed windows and track `ActiveDockable`. This allows `MdiDocumentWindow` to keep working with minimal changes.
2. Create `ManagedHostWindow : IHostWindow` that:
   - Wraps a managed dock window model that implements `IMdiDocument`.
   - On `Present`, inserts a `MdiDocumentWindow` into the `ManagedWindowLayer`.
   - On `Exit`, removes the managed window from the layer.
   - Maps `IDockWindow` bounds to `IMdiDocument.MdiBounds` (and back on save).
3. Add a factory hook to choose managed vs native windows:
   - `HostWindowLocator` / `DefaultHostWindowLocator` returns `ManagedHostWindow` when managed mode is enabled.
   - A `DockSettings` flag or `IFactory.WindowingMode` (`Native | Managed`) selects the path.

#### Why this fits managed windows
- Managed windows render inside the main window (no OS window), and the MDI layout manager already provides drag/resize/min/max and z-ordering.
- The standard floating-window pipeline (`IDockWindow` + `IHostWindow`) remains intact, so app code does not need to change.

## Detailed work required for native + managed parity
This list captures the additional refactors needed to ensure both window systems behave consistently.

### Window lifecycle abstraction
- Introduce a small abstraction layer (for example `IWindowingServices`) that exposes:
  - Create host window (native/managed)
  - Activate window / bring to front
  - Z-order query/update
  - Optional owner/parent relationships
- Update `WindowActivationHelper` and `DockHelpers` to use this abstraction instead of `Window.SortWindowsByZOrder`.

### Host state and drag handling
- Extend the existing `IHostWindowState` contract with a managed implementation (or provide a managed `HostWindowState` variant) so both native and managed hosts can drive the same docking events.
- For managed windows, reuse `MdiDocumentWindow` drag/resize handling and ensure it raises the same docking events (`WindowMoveDragBegin/Move/End`).

### Overlay and diagnostics compatibility
- Refactor `AvaloniaHostServiceResolver` to resolve services from a managed host layer as well as `HostWindow`.
- Add factory hooks for debug/overlay windows to allow managed equivalents or in-tree overlays.

### Helper windows
- Add explicit creation hooks for `DockAdornerWindow`, `DragPreviewWindow`, and `PinnedDockWindow`, and provide managed equivalents where appropriate.
- Ensure helper windows can attach to either a `TopLevel` or a managed host layer.
- Provide managed counterparts that can reuse the same styling resources as native windows.

### Data context and layout ownership
- Managed host windows must expose a data context path for overlays and document tooling that currently assume `HostWindow`.
- Maintain the owner chain and active dockable in managed mode (either via `IMdiHost` or `ManagedWindowDock`).

## Managed window theming and template parity
Managed windows should look and feel like real windows. The goal is to reuse existing window themes (Fluent/Simple) and chrome resources so managed substitutes are visually consistent and require minimal additional styling.

### Theming goals
- Reuse existing `HostWindow` and window chrome resources where possible.
- Share title bar, button, and border templates between native and managed windows.
- Keep overlay/preview/pinned managed windows consistent with app themes.
- Support the same pseudo-classes (`:active`, `:maximized`, `:minimized`, `:dragging`) for consistent styling.

### Template reuse strategy
- Extract common window chrome into reusable control themes or styles (for example `WindowChromeTheme`, `TitleBarTheme`, `WindowButtonTheme`) that can be applied by both `HostWindow` and managed window controls (or expose common brush keys that both templates consume).
- Ensure `MdiDocumentWindow` and managed helper windows (adorner/preview/pinned) use the same resource keys as `HostWindow` templates.
- Avoid duplicating theme XAML by referencing shared resources in `Dock.Avalonia.Themes.*` (Fluent/Simple).

### Theme resource locations (current)
- `DockFluentTheme.axaml` includes window-related templates from `src/Dock.Avalonia.Themes.Fluent/Controls/*.axaml` (for example `HostWindow.axaml`, `HostWindowTitleBar.axaml`, `DragPreviewWindow.axaml`, `PinnedDockWindow.axaml`, `DockAdornerWindow.axaml`, `MdiDocumentWindow.axaml`, `ManagedWindowLayer.axaml`, `WindowChromeResources.axaml`).
- `DockSimpleTheme.axaml` includes the same `/Controls/*.axaml` resources; the Simple theme links Fluent control resources via `Dock.Avalonia.Themes.Simple.csproj` (see `AvaloniaResource` link to `Dock.Avalonia.Themes.Fluent/Controls`).
- New managed-window templates/resources should be added to `src/Dock.Avalonia.Themes.Fluent/Controls/` and referenced in both `DockFluentTheme.axaml` and `DockSimpleTheme.axaml`.

### Managed substitutes that need templates
- **Managed host surface**: the managed window surface is the existing `MdiDocumentWindow` hosted by `ManagedWindowLayer`; no separate `ManagedHostWindowControl` is required unless future design calls for it.
- **Managed dock adorner overlay**: overlay-hosted `DockTarget`/`GlobalDockTarget` visuals should reuse the same brush keys as window chrome where applicable.
- **Managed drag preview host**: `DragPreviewControl` overlays should use the same theme resources as the native preview window.
- **Managed pinned dock host**: `ToolDockControl` overlays should use the same theme resources as the native pinned dock window.
- **Managed debug/diagnostic window** (optional): if added later, reuse the same window chrome resource keys for parity.

### Theme integration checklist
- Define shared resource keys for window chrome (background, border, shadow, title bar, buttons).
- Apply those keys in both native `HostWindow` templates and managed window controls.
- Provide theme resources for managed helper windows in `Dock.Avalonia.Themes.Fluent` and `Dock.Avalonia.Themes.Simple`.
- Verify visual parity across light/dark variants and scaling.
- Verify pointer hit targets, padding, and resize grip thickness match native windows.

## App and sample guidance
- Document a supported pattern for custom host windows:
  - Set `Factory.DefaultHostWindowLocator = () => new MyHostWindow();`
  - Disable `DockControl.InitializeFactory` if needed to avoid overrides.
- Provide at least one sample showing custom host windows (for example, a managed-window subclass or a themed host window).

## Backward compatibility
- Preserve existing defaults when no custom factory/locator is supplied.
- Introduce new properties/overloads instead of changing existing signatures.

## Suggested implementation steps
This is a concise summary. Use the checklist below as the source of truth for execution.

1. Add a `HostWindowFactory` (or similar) property to `DockControl` and update `InitializeFactory` to respect existing locators and custom factories.
2. Add an optional factory parameter to `RootDockDebugExtensions.AttachDockDebug`.
3. Add creation hooks for `DockAdornerWindow`, `DragPreviewWindow`, and `PinnedDockWindow` (factory delegate or service).
4. Implement managed window hosting (MDI reuse) via `ManagedWindowLayer` + `ManagedHostWindow`.
5. Abstract activation/z-order/drag operations so managed windows can participate without `Window.SortWindowsByZOrder`.
6. Add managed-aware service resolution for overlays/diagnostics.
7. Extract shared chrome resources and apply them to managed window templates (host/adorner/preview/pinned/debug).
8. Update docs and samples to demonstrate managed windows and custom host windows with theme parity.

## Implementation plan (checkable tasks)
1. [x] Add managed window mode flag (`DockSettings.UseManagedWindows` or `IFactory.WindowingMode`) and document default behavior.
2. [x] Create `ManagedWindowLayer` control to host MDI windows in-app.
3. [x] Add `ManagedHostWindow : IHostWindow` that maps `IDockWindow` to a managed MDI window item.
4. [x] Wire `HostWindowLocator`/`DefaultHostWindowLocator` to return `ManagedHostWindow` when managed mode is enabled.
5. [x] Update `MdiDocumentWindow` dependencies:
   - Option A: introduce `IMdiHost` and refactor to use it.
   - Option B: add a `ManagedWindowDock : IDock` implementation used by `ManagedWindowLayer`.
6. [x] Add factory hooks for `DockAdornerWindow`, `DragPreviewWindow`, and `PinnedDockWindow`.
7. [x] Add overloads/DI hooks for `RootDockDebugExtensions.AttachDockDebug`.
8. [x] Add abstraction for activation/z-order so managed windows can be ordered/activated without `Window.SortWindowsByZOrder`.
9. [x] Make overlay/service resolution work for managed windows (no `TopLevel`/`HostWindow`).
10. [x] Define shared window chrome resources in `src/Dock.Avalonia.Themes.Fluent/Controls/*.axaml` and include them in `DockFluentTheme.axaml`.
11. [x] Ensure `DockSimpleTheme.axaml` includes the same resources (linked from Fluent controls) and validate Simple theme overrides.
12. [x] Apply shared chrome resources to managed windows (host, adorner, drag preview, pinned dock, debug).
13. [x] Update docs (`dock-window-creation-overrides.md`, `dock-host-window-locator.md`) with managed window guidance and theming notes.
14. [x] Add/update a sample showing managed window hosting (MDI-backed floating windows) with theme parity.
15. [ ] Validate: drag/resize, activation, z-order, docking/undocking, theming consistency, input routing, and cleanup on window close.

## Reference locations
- `src/Dock.Avalonia/Controls/DockControl.axaml.cs`
- `src/Dock.Model/FactoryBase.Locator.cs`
- `src/Dock.Model/FactoryBase.Init.cs`
- `src/Dock.Model/Adapters/HostAdapter.cs`
- `src/Dock.Avalonia.Diagnostics/Controls/RootDockDebugExtensions.cs`
- `src/Dock.Avalonia/Internal/AdornerHelper.cs`
- `src/Dock.Avalonia/Internal/DragPreviewHelper.cs`
- `src/Dock.Avalonia/Controls/PinnedDockControl.axaml.cs`
- `src/Dock.Avalonia/Internal/WindowActivationHelper.cs`
- `src/Dock.Avalonia/Internal/DockHelpers.cs`
- `src/Dock.Avalonia/Internal/WindowDragHelper.cs`
- `src/Dock.Model.ReactiveUI.Services.Avalonia/Services/AvaloniaHostServiceResolver.cs`
- `samples/*/Program.cs`
- `samples/**/MainWindow.axaml*`
