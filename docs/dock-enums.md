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

## DocumentTabLayout

Specifies where the tab strip of a document dock is placed.

| Value | Description |
| ----- | ----------- |
| `Top` | Tabs are shown above the content. |
| `Left` | Tabs appear on the left side. |
| `Right` | Tabs appear on the right side. |

## GripMode

Determines how the grip element of a splitter behaves.

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

Represents the allowed drag and drop actions when interacting with external data sources.

| Value | Description |
| ----- | ----------- |
| `None` | No action permitted. |
| `Copy` | Data will be copied. |
| `Move` | Data will be moved. |
| `Link` | A link to the data will be created. |

For a high level overview of Dock terminology see the [glossary](dock-glossary.md).
