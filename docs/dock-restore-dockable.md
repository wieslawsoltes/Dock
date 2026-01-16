# RestoreDockable Behavior and Splitter Management

This guide explains the `RestoreDockable` functionality in the Dock library, its behavior regarding splitter management, and recent optimizations to prevent unwanted splitter creation.

## Overview

The `RestoreDockable` method is responsible for moving hidden dockables back to their original visible locations within the docking layout. This functionality is essential for applications that support hiding and showing dock panels dynamically.

## Method Signatures

The `IFactory` interface provides two overloads for restoring dockables:

```csharp
/// <summary>
/// Restores a hidden dockable to its original dock.
/// </summary>
/// <param name="dockable">The dockable to restore.</param>
void RestoreDockable(IDockable dockable);

/// <summary>
/// Restores a hidden dockable to its original dock.
/// </summary>
/// <param name="id">The dockable id.</param>
/// <returns>The restored dockable or null.</returns>
IDockable? RestoreDockable(string id);
```

## Behavior

### Basic Restoration Process

1. **Find Root Dock**: Locates the root dock that contains the hidden dockable
2. **Validate Hidden State**: Checks if the dockable exists in the `HiddenDockables` collection
3. **Remove from Hidden**: Removes the dockable from `HiddenDockables`
4. **Fire Restored Event**: Triggers the `DockableRestored` event
5. **Restore to Original Owner**: If the dockable has an `OriginalOwner`, adds it back to that dock's `VisibleDockables`
6. **Update Ownership**: Sets the dockable's `Owner` to the original owner and clears `OriginalOwner`
7. **Fire Added Event**: Triggers the `DockableAdded` event

### String Overload Behavior

The string overload (`RestoreDockable(string id)`) searches all root docks returned by the factory (`Find(d => d is IRootDock)`) and looks for a hidden dockable with the specified ID. If found, it calls the main `RestoreDockable(IDockable)` method.

## Splitter Management

`RestoreDockable` does not create splitters automatically. The layout optimization system handles splitter creation and cleanup automatically as needed.

### Current Implementation
```csharp
if (dockable.OriginalOwner is IDock owner)
{
    AddVisibleDockable(owner, dockable);
    OnDockableAdded(dockable);
    dockable.Owner = owner;
    dockable.OriginalOwner = null;
}
```

### Design Principles

1. **Layout Consistency**: Splitter management is centralized in the layout optimization system
2. **Automatic Management**: Splitters are created and removed based on actual layout needs
3. **Simplified Logic**: RestoreDockable focuses solely on moving dockables between collections

### Splitter Integration

The layout system handles splitters through:
- `CleanupOrphanedSplitters()`: Removes splitters at beginnings/ends and consecutive splitters
- `RemoveDockable()`: Automatically cleans up splitters after dockable removal
- `SplitToDock()`: Creates splitters only when needed for new split operations

## Usage Examples

### Basic Restoration

```csharp
// Hide a dockable
factory.HideDockable(myDocument);

// Later, restore it
factory.RestoreDockable(myDocument);

// Or restore by ID
var restored = factory.RestoreDockable("MyDocumentId");
```

### Event Handling

```csharp
factory.DockableRestored += (sender, args) =>
{
    Console.WriteLine($"Restored: {args.Dockable.Title}");
};

factory.DockableAdded += (sender, args) =>
{
    Console.WriteLine($"Added back to dock: {args.Dockable.Title}");
};
```

### Error Handling

```csharp
// Restore by ID with error handling
var restored = factory.RestoreDockable("MyDocumentId");
if (restored == null)
{
    Console.WriteLine("Dockable not found or not hidden");
}
```

## Best Practices

### Do's
- ✅ Use the direct dockable overload when you have a reference to the dockable
- ✅ Use the string overload when working with serialized layouts or IDs
- ✅ Handle the case where string restoration returns null
- ✅ Let the layout optimization handle splitter management automatically

### Don'ts
- ❌ Don't manually create splitters before or after calling `RestoreDockable`
- ❌ Don't assume splitters will be automatically created during restoration
- ❌ Don't modify `OriginalOwner` manually - let the factory manage this property

## Error Conditions

| Condition | Behavior |
|-----------|----------|
| `dockable` is null | Not expected; the API requires a non-null dockable |
| Root dock not found | Method returns early, no action taken |
| `HiddenDockables` is null | Method returns early, no action taken |
| Dockable not in `HiddenDockables` | Method returns early, no action taken |
| `OriginalOwner` is null | Sets dockable's `Owner` to null |
| `OriginalOwner.VisibleDockables` is null | Creates new collection and adds dockable |

## Testing

The library includes comprehensive unit tests covering:

- Basic restoration scenarios
- Multiple dockables restoration
- Different dock types (ToolDock, DocumentDock, ProportionalDock)
- Edge cases and error conditions
- Event firing verification
- **Splitter behavior verification** (ensures correct splitter management)

## Related Documentation

- [Dock events](dock-events.md)
- [Dock state guide](dock-state.md)
- [DockManager guide](dock-manager-guide.md)

## Implementation Notes

When working with `RestoreDockable`:

1. Splitter management is handled automatically by the layout system
2. Focus on the dockable restoration logic rather than splitter concerns
3. The layout optimization ensures consistent splitter behavior across all operations
4. Splitters are serialized and restored through the standard layout persistence mechanism
