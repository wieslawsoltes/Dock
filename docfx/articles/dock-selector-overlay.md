# Selector Overlay

Dock includes a document/tool selector overlay that lets users switch between dockables with the keyboard. The overlay is part of the `DockControl` template and is driven by `DockSettings` gestures.

## Default shortcuts

- Document selector: `Ctrl+Tab`
- Tool selector: `Ctrl+Alt+Tab`

These defaults come from `DockSettings.DocumentSelectorKeyGesture` and `DockSettings.ToolSelectorKeyGesture`.

## Enable or customize

```csharp
DockSettings.SelectorEnabled = true;
DockSettings.DocumentSelectorKeyGesture = new KeyGesture(Key.Tab, KeyModifiers.Control);
DockSettings.ToolSelectorKeyGesture = new KeyGesture(Key.Tab, KeyModifiers.Control | KeyModifiers.Alt);
```

When the selector is open:

- `Tab` / `Shift+Tab` cycles items
- Arrow keys move selection
- `Enter` activates the selected item
- `Esc` closes the overlay
- Releasing the modifier keys commits the current selection

## Control what appears

Dockable base classes implement `IDockSelectorInfo`. Use it to hide items or change labels:

```csharp
public class OutputTool : Tool
{
    public OutputTool()
    {
        ShowInSelector = false;
    }
}

public class DocumentViewModel : Document
{
    public DocumentViewModel()
    {
        SelectorTitle = "README.md";
    }
}
```

## Programmatic control

`DockControl` implements `IDockSelectorService` so you can open or close the overlay in code:

```csharp
if (dockControl is IDockSelectorService selector)
{
    selector.ShowSelector(DockSelectorMode.Documents);
    // ...
    selector.HideSelector();
}
```

## Overlay control properties

The overlay itself is a `DockSelectorOverlay` control in the `DockControl` template. It exposes:

- `IsOpen` - Toggles visibility and the `:open` pseudo class.
- `Items` - The `DockSelectorItem` list rendered by the overlay.
- `SelectedItem` - The currently highlighted selector item.
- `Mode` - `Documents` or `Tools`, shown in the header.
- `PART_ItemsList` - `ListBox` template part used for selector navigation and UI automation selection/scroll delegation. Keep this part in custom templates for full reader support.

These properties are primarily used by the template but can be styled or replaced in a custom theme.

## Automation behavior

`DockSelectorOverlayAutomationPeer` exposes:

- `AutomationControlType.List`
- `IExpandCollapseProvider` for open/close
- `ISelectionProvider` for current selector entry
- `IScrollProvider` delegated to the internal `ListBox`
- `IValueProvider` (read-only selected title)

Automation events raised by `DockSelectorOverlayAutomationPeer`:

- `ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty` when `IsOpen` changes.
- `SelectionPatternIdentifiers.SelectionProperty` and `ValuePatternIdentifiers.ValueProperty` when `SelectedItem` changes.
- `AutomationElementIdentifiers.NameProperty` when `Mode` changes (for mode-specific selector name).
- `ChildrenChanged` when `Items` is replaced.

Reader compatibility notes:

- `IValueProvider.SetValue` is intentionally unsupported (read-only contract).
- Selection and scroll providers are delegated to `PART_ItemsList`, so they stay compatible with default list automation behavior.

## Ordering and selection

The selector is ordered by the most recently activated dockables. Dock tracks activation order inside `DockControl`, so frequently used documents and tools appear first.

For related settings see [Dock settings](dock-settings.md).

For UI automation peer behavior and accessibility contract details, see [Accessibility and UI automation](dock-accessibility.md).
