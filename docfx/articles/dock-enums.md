# Dock Enumerations

Several Dock APIs rely on small enumeration types. This page lists the most common ones so you know what values to expect when configuring docks or handling events.

## Alignment

Used by tools and docks to describe where they are placed relative to the main layout.

| Value | Description |
| ----- | ----------- |
| `Unset` | Alignment has not been specified. |
| `Left` | Positioned on the left side. |
| `Bottom` | Positioned at the bottom. |
| `Right` | Positioned on the right side. |
| `Top` | Positioned at the top. |

## DockMode

Indicates the default docking location for a dock or dockable.

| Value | Description |
| ----- | ----------- |
| `Center` | Fills the remaining space. |
| `Left` | Docks to the left. |
| `Bottom` | Docks to the bottom. |
| `Right` | Docks to the right. |
| `Top` | Docks to the top. |

## DockOperation

Specifies the operation being performed when a dockable is moved or split.

| Value | Description |
| ----- | ----------- |
| `None` | No docking operation. |
| `Fill` | Replace the target area. |
| `Left` | Insert to the left. |
| `Bottom` | Insert below. |
| `Right` | Insert to the right. |
| `Top` | Insert above. |
| `Window` | Open in a floating window. |

## Orientation

Controls whether a proportional dock arranges its children horizontally or vertically.

| Value | Description |
| ----- | ----------- |
| `Horizontal` | Children are stacked from left to right. |
| `Vertical` | Children are stacked from top to bottom. |

## GridResizeDirection

Specifies whether a grid splitter moves rows or columns.

| Value | Description |
| ----- | ----------- |
| `Columns` | Resize grid columns. |
| `Rows` | Resize grid rows. |

## DocumentTabLayout

Controls where document tabs are placed around a document dock. Setting this affects both orientation and docking position of the tab strip.

| Value | Description |
| ----- | ----------- |
| `Top` | Tabs are shown above the document content. |
| `Left` | Tabs are arranged vertically to the left. |
| `Right` | Tabs are arranged vertically to the right. |

## DocumentCloseButtonShowMode

Controls when close buttons are displayed on document tabs.

| Value | Description |
| ----- | ----------- |
| `Always` | Always show close buttons. |
| `Active` | Show close buttons only for the active document. |
| `Never` | Never show close buttons. |

## DocumentLayoutMode

Controls whether a document dock renders documents as tabs or classic MDI windows.

| Value | Description |
| ----- | ----------- |
| `Tabbed` | Documents are shown as tabs (default). |
| `Mdi` | Documents are shown as internal MDI windows. |

## MdiWindowState

Indicates the current state of an MDI document window.

| Value | Description |
| ----- | ----------- |
| `Normal` | Window is in its normal size and position. |
| `Minimized` | Window is minimized. |
| `Maximized` | Window fills the document area. |

## DockWindowState

Indicates the current state of a floating dock window (`IDockWindow.WindowState`).

| Value | Description |
| ----- | ----------- |
| `Normal` | Window is in its normal size and position. |
| `Minimized` | Window is minimized. |
| `Maximized` | Window is maximized. |
| `FullScreen` | Window uses full-screen presentation when supported. |

## GripMode

Determines how the grip element in tool chrome behaves (used by tool docks).

| Value | Description |
| ----- | ----------- |
| `Visible` | Grip is always shown. |
| `AutoHide` | Grip hides until the pointer is over it. |
| `Hidden` | Grip is not displayed. |

## TrackingMode

Indicates which bounds `TrackingAdapter` is currently storing.

| Value | Description |
| ----- | ----------- |
| `Visible` | Tracks the visible bounds. |
| `Pinned` | Tracks the pinned bounds. |
| `Tab` | Tracks the tab bounds. |

## DragAction

Represents the drag and drop action Dock should perform when docking. This is a flags enum, but Dock uses single values (`Move`, `Link`, `Copy`) for internal docking gestures.

| Value | Description |
| ----- | ----------- |
| `None` | No action permitted. |
| `Copy` | Data will be copied. |
| `Move` | Data will be moved. |
| `Link` | A link to the data will be created. |

## SplitViewDisplayMode

Controls how split view panes are displayed.

| Value | Description |
| ----- | ----------- |
| `Inline` | Pane is displayed next to content and does not auto-collapse. |
| `CompactInline` | Pane is displayed next to content with a compact closed state. |
| `Overlay` | Pane overlays the content and auto-collapses. |
| `CompactOverlay` | Overlay mode with a compact closed state. |

## SplitViewPanePlacement

Controls where the split view pane is placed.

| Value | Description |
| ----- | ----------- |
| `Left` | Pane is shown to the left of the content. |
| `Right` | Pane is shown to the right of the content. |
| `Top` | Pane is shown above the content. |
| `Bottom` | Pane is shown below the content. |

For a high level overview of Dock terminology see the [glossary](dock-glossary.md).
