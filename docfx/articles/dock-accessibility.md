# Accessibility and UI Automation

Dock includes a dedicated UI automation layer for docking controls. This improves screen reader discoverability and enables automation clients to query dock state and invoke core actions.

## Control coverage

| Control | Peer | Role (`AutomationControlType`) | Providers |
| --- | --- | --- | --- |
| `DockControl` | `DockControlAutomationPeer` | `Pane` | - |
| `RootDockControl` | `RootDockControlAutomationPeer` | `Pane` | - |
| `DockCommandBarHost` | `DockCommandBarHostAutomationPeer` | `ToolBar` | - |
| `DockTarget` / `GlobalDockTarget` | `DockTargetAutomationPeer` | `Pane` | `IInvokeProvider` |
| `DocumentControl` | `DocumentControlAutomationPeer` | `Pane` | `IInvokeProvider` |
| `ToolControl` | `ToolControlAutomationPeer` | `Pane` | `IInvokeProvider` |
| `MdiDocumentControl` | `MdiDocumentControlAutomationPeer` | `Pane` | `IInvokeProvider` |
| `DocumentTabStrip` | `DocumentTabStripAutomationPeer` | `Tab` | `ISelectionProvider`, `IScrollProvider` |
| `ToolTabStrip` | `ToolTabStripAutomationPeer` | `Tab` | `ISelectionProvider`, `IScrollProvider` |
| `DocumentTabStripItem` | `DocumentTabStripItemAutomationPeer` | `TabItem` | `IInvokeProvider`, selection item support |
| `ToolTabStripItem` | `ToolTabStripItemAutomationPeer` | `TabItem` | `IInvokeProvider`, selection item support |
| `ToolChromeControl` | `ToolChromeControlAutomationPeer` | `TitleBar` | `IInvokeProvider`, `IExpandCollapseProvider` |
| `PinnedDockControl` | `PinnedDockControlAutomationPeer` | `Pane` | `IExpandCollapseProvider` |
| `ToolPinnedControl` | `ToolPinnedControlAutomationPeer` | `Tab` | `ISelectionProvider` |
| `ToolPinItemControl` | `ToolPinItemControlAutomationPeer` | `TabItem` | `IInvokeProvider`, `ISelectionItemProvider` |
| `MdiDocumentWindow` | `MdiDocumentWindowAutomationPeer` | `Window` | `IInvokeProvider` |
| `DockSelectorOverlay` | `DockSelectorOverlayAutomationPeer` | `List` | `IExpandCollapseProvider`, `ISelectionProvider`, `IScrollProvider`, `IValueProvider` |
| `HostWindow` | `HostWindowAutomationPeer` | `Window` | `IInvokeProvider` |
| `HostWindowTitleBar` | `HostWindowTitleBarAutomationPeer` | `TitleBar` | `IInvokeProvider` |
| `PinnedDockWindow` | `PinnedDockWindowAutomationPeer` | `Window` | `IInvokeProvider` |
| `DragPreviewControl` | `DragPreviewControlAutomationPeer` | `Pane` | `IValueProvider` |
| `DragPreviewWindow` | `DragPreviewWindowAutomationPeer` | `Pane` | Decorative only (non-control/non-content element) |
| `DockAdornerWindow` | `DockAdornerWindowAutomationPeer` | `Pane` | Decorative only (non-control/non-content element) |

## Provider behavior contract

- `IInvokeProvider` on dock hosts (`DocumentControl`, `ToolControl`, `MdiDocumentControl`), tab items, chrome, and window peers activates/focuses the represented dockable or window.
- `ISelectionItemProvider` on `DocumentTabStripItem` and `ToolTabStripItem` maps selection actions (`Select`, `AddToSelection`) to the same activation path as `Invoke`.
- `IExpandCollapseProvider` on `DockSelectorOverlay`, `ToolChromeControl`, and `PinnedDockControl` controls selector visibility, flyout visibility, and pinned dock expansion.
- `IValueProvider` on `DockSelectorOverlay` and `DragPreviewControl` is read-only and exposes current selected/status text.
- `IScrollProvider` and `ISelectionProvider` on `DockSelectorOverlay` delegate to the internal `PART_ItemsList` list host.

## Name and ID resolution

Dock peers resolve automation metadata in this order:

1. `AutomationProperties.Name` / `AutomationProperties.AutomationId`
2. Dock model fallback (`IDockable.Title`, `IDockable.Id`)
3. Control fallback (for example, `Document tab`, `Dock target`, `Documents selector`)

This keeps behavior theme-independent and works even when custom templates are used.

You can override generated metadata directly in XAML:

```xml
<DocumentTabStripItem
    AutomationProperties.Name="Editor tab"
    AutomationProperties.AutomationId="editor-tab-primary" />
```

## State exposure

Peers expose state through `HelpText` and provider properties. Examples:

- Dock target: horizontal/vertical/global availability flags
- Dock host controls: layout id, docking enabled state, selector visibility, command bar counts
- Document/tool hosts: visible counts, active item, create/layout metadata
- Tab strips: selected index, item count, orientation and create affordances
- Tab items: selected/active and dockable capabilities
- Tool chrome: active/pinned/floating/maximized/menu state
- Pinned hosts: alignment/display mode, expanded state and visible pinned count
- MDI window: active and `MdiState`
- Selector overlay: open/closed, mode, item count, selected entry
- Drag preview control: preview status and content visibility
- Host/pinned windows: tracked/tool-window/window-state metadata

## Automation events

Dock peers raise automation change events for key transitions:

- `DockSelectorOverlay`: expand/collapse (`ExpandCollapseStateProperty`), selection (`SelectionProperty`), selected value (`ValueProperty`), mode/name (`NameProperty`), and items updates (`ChildrenChanged`).
- `DocumentTabStripItem` and `ToolTabStripItem`: selection state changes (`SelectionItemPatternIdentifiers.IsSelectedProperty`).
- `DragPreviewControl`: status changes (`ValuePatternIdentifiers.ValueProperty`).

## Template requirements

- Keep `PART_ItemsList` (`ListBox`) in `DockSelectorOverlay` templates to preserve full `ISelectionProvider` and `IScrollProvider` behavior for automation readers.

## Actions

Primary actions are automation-invokable where applicable:

- Document/tool/MDI host invoke activates the current (or first visible) dockable.
- Tab peers invoke activation and selection of the represented dockable.
- Pinned tab invoke activates the represented pinned tool.
- Tool chrome invoke activates the active tool; expand/collapse opens and closes the tool flyout.
- Pinned dock expand/collapse toggles `IToolDock.IsExpanded`.
- MDI window invoke activates the represented document window.
- Selector overlay expand/collapse toggles overlay visibility.
- Host and pinned windows invoke to focus/activate their surface.

## Keyboard-only selector navigation

Selector overlay keyboard navigation is validated in headless tests:

- Open document selector: `DockSettings.DocumentSelectorKeyGesture` (default `Ctrl+Tab`)
- Open tool selector: `DockSettings.ToolSelectorKeyGesture` (default `Ctrl+Alt+Tab`)
- Navigate: `Tab`, `Shift+Tab`, arrow keys
- Commit: `Enter` or release selector modifier keys
- Cancel: `Esc`

See [Selector overlay](dock-selector-overlay.md) for usage and runtime customization.

## Testing automation peers

You can inspect peers programmatically:

```csharp
Control control = new DocumentTabStripItem { DataContext = document };
AutomationPeer peer = ControlAutomationPeer.CreatePeerForElement(control);

AutomationControlType role = peer.GetAutomationControlType();
string name = peer.GetName();
IInvokeProvider? invoke = peer.GetProvider<IInvokeProvider>();
invoke?.Invoke();
```

Headless validation in this repository includes:

- `AutomationPeersTests` for role/provider/state contracts.
- `AutomationReaderCompatibilityTests` for peer-tree traversal and automation-reader style pattern usage.
- `DockSelectorKeyboardNavigationTests` for keyboard-only selector workflows.
- `AutomationPeerLeakTests` (Release) to validate event/subscription cleanup and prevent peer-related leaks.

For full control property reference, see [Avalonia controls reference](dock-controls-reference.md).
