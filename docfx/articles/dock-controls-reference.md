# Avalonia Controls Reference

This reference lists Dock-specific properties for Avalonia controls in the `Dock.Avalonia` packages. Base Avalonia properties (for example, `Width`, `IsVisible`, `Background`) are not repeated here.

Unless noted otherwise, the properties listed are Avalonia styled properties and can be set in XAML or code. Properties labeled "CLR" are regular properties that must be set in code.

## Core host controls

### DockControl

| Property | Type | Description |
| --- | --- | --- |
| `Layout` | `IDock?` | Root layout assigned to the control. |
| `Factory` | `IFactory?` | Factory used when `Layout.Factory` is `null`. |
| `InitializeFactory` | `bool` | When `true`, assigns default locators and fallback delegates. |
| `InitializeLayout` | `bool` | When `true`, calls `InitLayout` on the factory. |
| `DefaultContext` | `object?` | Fallback context for the default context locator. |
| `AutoCreateDataTemplates` | `bool` | Automatically injects Dock DataTemplates when `true`. |
| `IsDockingEnabled` | `bool` | Enables or disables docking interactions (layout lock). |
| `IsDraggingDock` | `bool` | Set while a drag operation is active; useful for styling. |
| `DragOffsetCalculator` | `IDragOffsetCalculator` | CLR property that controls drag preview positioning. |
| `DockManager` | `IDockManager` | CLR property for the dock manager service (read-only). |
| `DockControlState` | `IDockControlState` | CLR property for internal docking state (read-only). |
| `IsOpen` | `bool` | CLR property; `true` when the selector overlay is open (read-only). |

### DockCommandBarHost

| Property | Type | Description |
| --- | --- | --- |
| `MenuBars` | `IReadOnlyList<Control>?` | Rendered menu bar controls. |
| `ToolBars` | `IReadOnlyList<Control>?` | Rendered tool bar controls. |
| `RibbonBars` | `IReadOnlyList<Control>?` | Rendered ribbon bar controls. |
| `BaseCommandBars` | `IReadOnlyList<DockCommandBarDefinition>?` | App-level bar definitions merged with active dockables. |

### DockSelectorOverlay

| Property | Type | Description |
| --- | --- | --- |
| `IsOpen` | `bool` | Shows or hides the overlay. |
| `Items` | `IReadOnlyList<DockSelectorItem>?` | Items displayed in the selector. |
| `SelectedItem` | `DockSelectorItem?` | Currently highlighted item. |
| `Mode` | `DockSelectorMode` | Documents or tools selector mode. |

### DockTargetBase (DockTarget, GlobalDockTarget)

| Property | Type | Description |
| --- | --- | --- |
| `ShowIndicatorsOnly` | `bool` | Hide selector icons and show only drop indicators. |
| `ShowHorizontalTargets` | `bool` | Toggle horizontal indicators. |
| `ShowVerticalTargets` | `bool` | Toggle vertical indicators. |
| `IsGlobalDockAvailable` | `bool` | Indicates global docking availability. |
| `IsGlobalDockActive` | `bool` | Indicates global docking active state. |

`DockTarget` and `GlobalDockTarget` inherit these properties without adding new ones.

## Tab, chrome, and tool controls

### DocumentControl

| Property | Type | Description |
| --- | --- | --- |
| `IconTemplate` | `object?` | Tab icon template. |
| `HeaderTemplate` | `IDataTemplate` | Tab header template. |
| `ModifiedTemplate` | `IDataTemplate` | Modified indicator template. |
| `CloseTemplate` | `IDataTemplate` | Close button template. |
| `CloseButtonTheme` | `ControlTheme?` | Theme for the close button. |
| `IsActive` | `bool` | Active document state (drives `:active`). |
| `TabsLayout` | `DocumentTabLayout` | Tab placement for the document dock. |

### ToolControl

| Property | Type | Description |
| --- | --- | --- |
| `IconTemplate` | `object?` | Tab icon template. |
| `HeaderTemplate` | `IDataTemplate` | Tab header template. |
| `ModifiedTemplate` | `IDataTemplate` | Modified indicator template. |

### DocumentTabStrip

| Property | Type | Description |
| --- | --- | --- |
| `CanCreateItem` | `bool` | `true` when the new-document button is available. |
| `IsActive` | `bool` | Active tab strip state (drives `:active`). |
| `EnableWindowDrag` | `bool` | Allows dragging the host window by the tab strip. |
| `Orientation` | `Orientation` | Tab strip orientation. |
| `MouseWheelScrollOrientation` | `Orientation` | Mouse-wheel scroll axis for tab overflow (`Horizontal` by default). |
| `CreateButtonTheme` | `ControlTheme?` | Theme for the create document button. |

### DocumentTabStripItem

| Property | Type | Description |
| --- | --- | --- |
| `IsActive` | `bool` | Active tab state (drives `:active`). |
| `DocumentContextMenu` | `ContextMenu?` | Context menu for document tabs. |

### ToolTabStrip

| Property | Type | Description |
| --- | --- | --- |
| `CanCreateItem` | `bool` | `true` when the new-tool button is available. |
| `MouseWheelScrollOrientation` | `Orientation` | Mouse-wheel scroll axis for tab overflow (`Horizontal` by default). |

### ToolTabStripItem

| Property | Type | Description |
| --- | --- | --- |
| `TabContextMenu` | `ContextMenu?` | Context menu for tool tabs. |

### ToolChromeControl

| Property | Type | Description |
| --- | --- | --- |
| `Title` | `string` | Tool title displayed in the chrome. |
| `IsActive` | `bool` | Active tool state (drives `:active`). |
| `IsPinned` | `bool` | Pinned state (drives `:pinned`). |
| `IsFloating` | `bool` | Floating state (drives `:floating`). |
| `IsMaximized` | `bool` | Maximized state (drives `:maximized`). |
| `ToolFlyout` | `FlyoutBase?` | Flyout for the grip/menu button. |
| `CloseButtonTheme` | `ControlTheme?` | Theme for the close button. |
| `MaximizeButtonTheme` | `ControlTheme?` | Theme for the maximize/restore button. |
| `PinButtonTheme` | `ControlTheme?` | Theme for the pin button. |
| `MenuButtonTheme` | `ControlTheme?` | Theme for the menu button. |

### ToolPinnedControl

| Property | Type | Description |
| --- | --- | --- |
| `Orientation` | `Orientation` | Orientation for pinned tool tabs. |

### ToolPinItemControl

| Property | Type | Description |
| --- | --- | --- |
| `Orientation` | `Orientation` | Orientation for the pin item. |
| `PinContextMenu` | `ContextMenu?` | Context menu for pinned tabs. |

### DockableControl

| Property | Type | Description |
| --- | --- | --- |
| `TrackingMode` | `TrackingMode` | Registers the control for visible, pinned, or tab tracking. |

## MDI controls

### MdiLayoutPanel

| Property | Type | Description |
| --- | --- | --- |
| `LayoutManager` | `IMdiLayoutManager?` | Arranges MDI windows. |

### MdiDocumentControl

| Property | Type | Description |
| --- | --- | --- |
| `IconTemplate` | `object?` | Window icon template. |
| `HeaderTemplate` | `IDataTemplate` | Window header template. |
| `ModifiedTemplate` | `IDataTemplate` | Modified indicator template. |
| `CloseTemplate` | `IDataTemplate` | Close button template. |
| `CloseButtonTheme` | `ControlTheme?` | Theme for the close button. |
| `LayoutManager` | `IMdiLayoutManager?` | Layout manager forwarded to the MDI panel. |
| `IsActive` | `bool` | Active state (drives `:active`). |

### MdiDocumentWindow

| Property | Type | Description |
| --- | --- | --- |
| `IconTemplate` | `object?` | Window icon template. |
| `HeaderTemplate` | `IDataTemplate` | Window header template. |
| `ModifiedTemplate` | `IDataTemplate` | Modified indicator template. |
| `CloseTemplate` | `IDataTemplate` | Close button template. |
| `CloseButtonTheme` | `ControlTheme?` | Theme for the close button. |
| `DocumentContextMenu` | `ContextMenu?` | Context menu for MDI windows. |
| `IsActive` | `bool` | Active state (drives `:active`). |
| `MdiState` | `MdiWindowState` | `Normal`, `Minimized`, or `Maximized`. |

## Pinned and drag preview

### PinnedDockControl

| Property | Type | Description |
| --- | --- | --- |
| `PinnedDockAlignment` | `Alignment` | Edge alignment for the pinned preview. |
| `PinnedDockDisplayMode` | `PinnedDockDisplayMode` | Controls whether pinned previews overlay content or take layout space. |

### PinnedDockHostPanel

| Property | Type | Description |
| --- | --- | --- |
| `PinnedDockDisplayMode` | `PinnedDockDisplayMode` | Controls whether the pinned preview overlays content or takes layout space. |
| `PinnedDockAlignment` | `Alignment` | Edge alignment used when the preview is arranged inline. |

### DragPreviewControl

| Property | Type | Description |
| --- | --- | --- |
| `ContentTemplate` | `IDataTemplate` | Template for the dragged dockable content. |
| `Status` | `string` | `Dock`, `Float`, or `None` status text. |
| `ShowContent` | `bool` | Toggles rendering the full dockable preview. |
| `PreviewContentWidth` | `double` | Width for the dockable preview content. |
| `PreviewContentHeight` | `double` | Height for the dockable preview content. |

## Window hosts

### HostWindow

| Property | Type | Description |
| --- | --- | --- |
| `IsToolWindow` | `bool` | Marks the window as a tool window. |
| `ToolChromeControlsWholeWindow` | `bool` | Allows tool chrome to drag the full window. |
| `DocumentChromeControlsWholeWindow` | `bool` | Allows document chrome to drag the full window. |
| `HostWindowState` | `IHostWindowState` | CLR property for docking state (read-only). |
| `IsTracked` | `bool` | CLR property that indicates the window is tracked. |
| `Window` | `IDockWindow?` | CLR property for the backing dock window model. |

### HostWindowTitleBar

No Dock-specific properties.

### DockAdornerWindow, DragPreviewWindow, PinnedDockWindow

No Dock-specific properties beyond the base `Window` members.

## Container controls without Dock-specific properties

These controls do not add custom Dock properties:

- `DocumentDockControl`
- `ToolDockControl`
- `DockDockControl`
- `RootDockControl`
- `ProportionalDockControl`
- `StackDockControl`
- `GridDockControl`
- `WrapDockControl`
- `UniformGridDockControl`
- `SplitViewDockControl`
- `DocumentContentControl`
- `ToolContentControl`

For related topics see the [Styling and theming](dock-styling.md) and [Reference guide](dock-reference.md).
