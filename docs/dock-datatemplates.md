# DataTemplates and Custom Dock Types

This guide explains how DockControl uses DataTemplates to render different dock elements and how to create custom dock types with their own visual representation.

## Overview

The Dock library uses Avalonia's DataTemplate system to render different types of dock elements. Each dock type (documents, tools, containers, splitters) has a corresponding DataTemplate that defines how it should be visually represented.

### Key Components

- **DockControl**: The main control that hosts dock layouts and manages DataTemplates
- **DockDataTemplateHelper**: Centralized creation of built-in DataTemplates  
- **RequiresDataTemplateAttribute**: Marks interfaces that need DataTemplates
- **AutoCreateDataTemplates**: Property to control automatic DataTemplate creation

## How DataTemplates Work

DockControl automatically creates DataTemplates for all dock types when its template is applied. The process works as follows:

1. **Automatic Creation**: When `AutoCreateDataTemplates` is `true` (default), DockControl automatically adds DataTemplates for all built-in dock types
2. **Template Matching**: Avalonia matches dock objects to their corresponding DataTemplate based on type
3. **Visual Rendering**: The matched DataTemplate creates the appropriate control to render the dock element

### Controlling DataTemplate Creation

You can control DataTemplate behavior using the `AutoCreateDataTemplates` property:

```csharp
// Disable automatic creation to use custom templates
dockControl.AutoCreateDataTemplates = false;

// Or in XAML
<DockControl AutoCreateDataTemplates="False" />
```

## RequiresDataTemplateAttribute

The `RequiresDataTemplateAttribute` marks dock interfaces that need DataTemplates. This ensures all dock types have proper visual representation.

### Usage

When creating custom dock types, mark the interface with this attribute:

```csharp
[RequiresDataTemplate]
public interface IMyCustomDock : IDock
{
    // Interface members
}
```

This attribute serves two purposes:
- **Documentation**: Clearly indicates which interfaces need visual representation
- **Testing**: Unit tests automatically verify that all marked interfaces have DataTemplates

## Built-in DataTemplates

The library provides DataTemplates for all built-in dock types:

**Content Types:**
- `IDocumentContent` → `DocumentContentControl`
- `IToolContent` → `ToolContentControl`

**Container Types:**
- `IDocumentDock` → `DocumentDockControl`
- `IToolDock` → `ToolDockControl`
- `IProportionalDock` → `ProportionalDockControl`
- `IStackDock` → `StackDockControl`
- `IGridDock` → `GridDockControl`
- `IWrapDock` → `WrapDockControl`
- `IUniformGridDock` → `UniformGridDockControl`
- `IDockDock` → `DockDockControl`
- `IRootDock` → `RootDockControl`

**Splitter Types:**
- `IProportionalDockSplitter` → `ProportionalStackPanelSplitter`
- `IGridDockSplitter` → `GridSplitter`

All DataTemplates are created automatically by `DockDataTemplateHelper` using Avalonia's binding syntax.

## Creating Custom Dock Types

To create a custom dock type with its own visual representation:

### 1. Define the Interface

Create your custom dock interface and mark it with the attribute:

```csharp
[RequiresDataTemplate]
public interface ICustomTabDock : IDock
{
    string TabStyle { get; set; }
    bool ShowTabIcons { get; set; }
}
```

### 2. Implement the Interface

Create a concrete implementation using your preferred model (Avalonia, MVVM, ReactiveUI, etc.):

```csharp
public class CustomTabDock : Dock, ICustomTabDock
{
    public string TabStyle { get; set; } = "Default";
    public bool ShowTabIcons { get; set; } = true;
}
```

### 3. Create the Control

Design the visual control for your dock type:

```csharp
public class CustomTabDockControl : TemplatedControl
{
    // Implement your custom control logic
}
```

### 4. Add the DataTemplate

**Option A: In Code (Recommended)**

```csharp
// Add your custom template alongside the default ones
var customTemplate = new FuncDataTemplate<ICustomTabDock>(
    (data, scope) => new CustomTabDockControl { DataContext = data });
dockControl.DataTemplates.Add(customTemplate);

// Default templates are automatically added if AutoCreateDataTemplates is true (default)
// To disable default templates entirely, set AutoCreateDataTemplates = false
```

**Option B: In XAML**

```xml
<DockControl>
    <DockControl.DataTemplates>
        <!-- Your custom template is added alongside the default ones -->
        <DataTemplate DataType="{x:Type model:ICustomTabDock}">
            <controls:CustomTabDockControl />
        </DataTemplate>
    </DockControl.DataTemplates>
</DockControl>
```

## Best Practices

When working with custom dock types and DataTemplates:

1. **Always mark custom dock interfaces** with `[RequiresDataTemplate]`
2. **Use descriptive naming** following the pattern: `IMyDock`, `MyDock`, `MyDockControl`
3. **Add custom templates directly** - they work alongside the default templates automatically
4. **Test your custom types** to ensure DataTemplate coverage
5. **Use the bang operator** (`!`) for property bindings when creating templates programmatically
6. **Consider inheritance** - derive from existing dock types when possible
7. **Only disable `AutoCreateDataTemplates`** when you want to provide all templates manually

## See Also

- [Creating Custom Models](dock-custom-model.md) - Learn about implementing custom dock models
- [Styling Guide](dock-styling.md) - Customize the appearance of dock controls
- [Sample Applications](../samples/) - Examples of custom dock implementations