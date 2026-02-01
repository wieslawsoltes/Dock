# Inline Pinned Dock Previews

## Summary
Pinned dock previews can now be shown in one of two display modes:

- `Overlay`: the preview floats above the main content (existing behavior).
- `Inline`: the preview consumes layout space and pushes main content aside.

You can set a root-level default and override the mode per dockable. Inline previews are resizable via the splitter, and their size is persisted via `PinnedBounds`.

## Key API Additions
- `PinnedDockDisplayMode` enum (`Overlay`, `Inline`).
- `IRootDock.PinnedDockDisplayMode` (root-level default).
- `IDockable.PinnedDockDisplayModeOverride` (optional per-dockable override).
- `IDockable.PinnedBounds` (stored pinned preview bounds).

## Usage

### Set inline mode at the root
```csharp
var root = new RootDock
{
    PinnedDockDisplayMode = PinnedDockDisplayMode.Inline
};
```

### Override per dockable
```csharp
var tool = new Tool
{
    Title = "Explorer",
    PinnedDockDisplayModeOverride = PinnedDockDisplayMode.Inline
};
```

### Reset to root default
```csharp
tool.PinnedDockDisplayModeOverride = null;
```

### Inspect or set pinned preview size
```csharp
// Read stored bounds
var bounds = tool.PinnedBounds;

// Persist a custom size
tool.PinnedBounds = new DockRect(0, 0, 280, 600);
```

### Fluent theme (root template)
The Fluent theme uses `PinnedDockHostPanel` to arrange the main content and pinned previews.

```xml
<controls:PinnedDockHostPanel
  PinnedDockDisplayMode="{Binding PinnedDockDisplayMode}"
  PinnedDockAlignment="{Binding PinnedDock.Alignment, FallbackValue={x:Static core:Alignment.Unset}}">
  <ContentControl Content="{Binding ActiveDockable}" Name="PART_MainContent" />
  <PinnedDockControl />
</controls:PinnedDockHostPanel>
```

## Behavior Details
- Inline layout is active when the effective display mode is `Inline` and alignment is not `Unset`.
- The splitter can resize the inline preview, and the control stores the new size in `PinnedBounds`.
- Overlay mode continues to use the pinned window/managed overlay logic when enabled.
- `PinnedBounds` is stored only when valid size data exists (non-NaN, non-infinite, positive).

## Serialization
`PinnedBounds` is included in Avalonia JSON serialization. Example payload:

```json
{
  "PinnedBounds": {
    "X": 0,
    "Y": 0,
    "Width": 280,
    "Height": 600
  }
}
```

## Notes
- Default root mode is `Overlay` to preserve existing behavior.
- Per-dockable overrides take priority over the root setting.
- Inline size persistence applies to both inline and overlay pinned previews; overlay uses the same bounds data.

