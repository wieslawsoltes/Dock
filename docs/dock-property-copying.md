# Property Copying API

This guide covers the property copying API introduced to provide fine-grained control over how properties are transferred between dockables and docks during various dock operations.

## Overview

The property copying API consists of five new virtual methods in the `IFactory` interface that allow you to customize how properties are copied during dock operations:

- `CopyDockableProperties` - Copies properties between dockables during move operations
- `CopyDockProperties` - Copies properties between docks during split operations  
- `CopyPropertiesForSplitDock` - Copies properties when creating new docks during splits
- `CopyPropertiesForFloatingWindow` - Copies properties when creating floating windows
- `CopyDimensionProperties` - Copies dimension properties for window operations

## API Reference

### CopyDockableProperties

```csharp
void CopyDockableProperties(IDockable source, IDockable target, DockOperation operation)
```

Copies properties from a source dockable to a target dockable during move operations.

**Parameters:**
- `source` - The source dockable
- `target` - The target dockable  
- `operation` - The dock operation being performed

**Default Implementation:** No properties are copied by default. Override to customize behavior.

### CopyDockProperties

```csharp
void CopyDockProperties(IDock source, IDock target, DockOperation operation)
```

Copies properties from a source dock to a target dock during split operations.

**Parameters:**
- `source` - The source dock
- `target` - The target dock
- `operation` - The dock operation being performed

**Default Implementation:** 
- For `IDocumentDock`: Copies `Id`, `CanCreateDocument`, `EnableWindowDrag`, and `DocumentTemplate`
- For `IToolDock`: Copies `Id`, `Alignment`, `GripMode`, `IsExpanded`, and `AutoHide`

### CopyPropertiesForSplitDock

```csharp
void CopyPropertiesForSplitDock(IDock source, IDock newDock, DockOperation operation, bool isNestedLayout = false)
```

Copies properties when creating a new dock during split operations.

**Parameters:**
- `source` - The source dock being split
- `newDock` - The newly created dock
- `operation` - The split operation being performed
- `isNestedLayout` - True for nested layout splits (proportions halved), false for root splits (source becomes NaN)

**Default Implementation:** Handles proportion copying based on the layout type:
- **Nested layouts**: Both source and new dock get half the original proportion
- **Root splits**: Source proportion becomes NaN, new dock gets the original proportion

### CopyPropertiesForFloatingWindow

```csharp
void CopyPropertiesForFloatingWindow(IDockable source, IDockWindow window, IDock targetDock)
```

Copies properties when creating a floating window from a dockable.

**Parameters:**
- `source` - The source dockable
- `window` - The new dock window
- `targetDock` - The dock that will contain the dockable in the window

**Default Implementation:** Uses `CopyDockProperties` for document dock consistency.

### CopyDimensionProperties

```csharp
void CopyDimensionProperties(IDockable source, object target, double x, double y, double width, double height)
```

Copies dimension properties (width, height, position) for window operations.

**Parameters:**
- `source` - The source dockable or dock
- `target` - The target window or dock
- `x` - The target X position
- `y` - The target Y position
- `width` - The target width
- `height` - The target height

**Default Implementation:** Sets window position and size properties if target is `IDockWindow`.

## Usage Examples

### Basic Property Copying

```csharp
public class CustomFactory : Factory
{
    public override void CopyDockableProperties(IDockable source, IDockable target, DockOperation operation)
    {
        // Copy custom properties
        if (source is ICustomDockable customSource && target is ICustomDockable customTarget)
        {
            customTarget.CustomProperty = customSource.CustomProperty;
            customTarget.Theme = customSource.Theme;
        }
        
        // Call base implementation
        base.CopyDockableProperties(source, target, operation);
    }
}
```

### Advanced Dock Property Copying

```csharp
public class AdvancedFactory : Factory
{
    public override void CopyDockProperties(IDock source, IDock target, DockOperation operation)
    {
        // Call base implementation first
        base.CopyDockProperties(source, target, operation);
        
        // Add custom logic based on operation type
        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Right:
                // Copy horizontal-specific properties
                if (source is ICustomDock customSource && target is ICustomDock customTarget)
                {
                    customTarget.HorizontalBehavior = customSource.HorizontalBehavior;
                }
                break;
                
            case DockOperation.Top:
            case DockOperation.Bottom:
                // Copy vertical-specific properties
                if (source is ICustomDock customSource && target is ICustomDock customTarget)
                {
                    customTarget.VerticalBehavior = customSource.VerticalBehavior;
                }
                break;
        }
    }
}
```

### Custom Split Dock Behavior

```csharp
public class ProportionAwareFactory : Factory
{
    public override void CopyPropertiesForSplitDock(IDock source, IDock newDock, DockOperation operation, bool isNestedLayout = false)
    {
        // Call base implementation for proportion handling
        base.CopyPropertiesForSplitDock(source, newDock, operation, isNestedLayout);
        
        // Copy additional properties
        if (source is ICustomDock customSource && newDock is ICustomDock customNewDock)
        {
            customNewDock.SplitBehavior = customSource.SplitBehavior;
            customNewDock.MinimumSize = customSource.MinimumSize;
            
            // Adjust properties based on split type
            if (isNestedLayout)
            {
                // For nested layouts, inherit parent settings
                customNewDock.InheritParentSettings = true;
            }
            else
            {
                // For root splits, use independent settings
                customNewDock.InheritParentSettings = false;
            }
        }
    }
}
```

### Floating Window Customization

```csharp
public class WindowAwareFactory : Factory
{
    public override void CopyPropertiesForFloatingWindow(IDockable source, IDockWindow window, IDock targetDock)
    {
        // Call base implementation
        base.CopyPropertiesForFloatingWindow(source, window, targetDock);
        
        // Customize window properties based on source
        if (source is IDocument document)
        {
            window.Title = $"Document: {document.Title}";
            window.Icon = document.Icon;
        }
        else if (source is ITool tool)
        {
            window.Title = $"Tool: {tool.Title}";
            window.CanResize = tool.CanResize;
        }
        
        // Set window-specific properties
        if (window is ICustomWindow customWindow)
        {
            customWindow.FloatingBehavior = FloatingBehavior.Independent;
            customWindow.ShowInTaskbar = true;
        }
    }
}
```

### Dimension Property Handling

```csharp
public class DimensionAwareFactory : Factory
{
    public override void CopyDimensionProperties(IDockable source, object target, double x, double y, double width, double height)
    {
        // Call base implementation
        base.CopyDimensionProperties(source, target, x, y, width, height);
        
        // Add constraints and validation
        if (target is IDockWindow window)
        {
            // Ensure minimum window size
            window.Width = Math.Max(width, 300);
            window.Height = Math.Max(height, 200);
            
            // Constrain to screen bounds
            var screenBounds = GetScreenBounds();
            window.X = Math.Max(0, Math.Min(x, screenBounds.Width - window.Width));
            window.Y = Math.Max(0, Math.Min(y, screenBounds.Height - window.Height));
            
            // Set additional window properties
            if (source is IDocument)
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
            }
        }
    }
    
    private Rectangle GetScreenBounds()
    {
        // Implementation to get screen bounds
        return new Rectangle(0, 0, 1920, 1080);
    }
}
```

## Integration Points

The property copying methods are automatically called during these operations:

- **Split Operations**: `CopyPropertiesForSplitDock` is called when creating new docks during splits
- **Window Creation**: `CopyPropertiesForFloatingWindow` and `CopyDimensionProperties` are called when floating dockables
- **Document Splits**: `CopyDockProperties` is called in `NewHorizontalDocumentDock` and `NewVerticalDocumentDock`
- **Move Operations**: `CopyDockableProperties` can be called during dockable movement (implementation-dependent)

## Best Practices

1. **Always call base implementation** unless you specifically want to override the default behavior completely
2. **Check types before casting** to ensure safe property access
3. **Consider the operation type** when deciding which properties to copy
4. **Handle both nested and root layouts** appropriately in `CopyPropertiesForSplitDock`
5. **Validate dimension values** in `CopyDimensionProperties` to prevent invalid window sizes
6. **Use consistent naming** for similar properties across different dock types

## Migration from Previous Versions

If you were previously modifying dock properties in other factory methods, consider moving that logic to the appropriate property copying method:

- Move dock creation customization to `CopyDockProperties`
- Move window creation customization to `CopyPropertiesForFloatingWindow`
- Move dimension handling to `CopyDimensionProperties`

This provides better separation of concerns and more predictable behavior.

## Related Documentation

- [Dock Advanced Guide](dock-advanced.md) - Advanced factory customization
- [Dock API Reference](dock-reference.md) - Complete API documentation
- [Dock Operations](dock-enums.md) - DockOperation enumeration values
- [Factory Guide](dock-content-guide.md) - General factory usage patterns

For an overview of all guides see the [documentation index](README.md).