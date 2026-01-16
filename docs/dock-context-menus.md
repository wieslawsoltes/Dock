# Context menus and flyouts

Dock defines several built-in context menus and flyouts that are attached to its controls. The menu definitions live in the theme control dictionaries under `src/Dock.Avalonia.Themes.Fluent/Controls`, and the menu text is stored in `src/Dock.Avalonia.Themes.Fluent/Controls/ControlStrings.axaml`. The Simple theme links to the same control resources. This document lists the available menus and describes how to localize or replace them.

## List of built-in menus

| File | Resource key | Purpose | Control Property |
| ---- | ------------ | ------- | ---------------- |
| `ToolChromeControl.axaml` | `ToolChromeControlContextMenu` | Menu for tool chrome grip button. | `ToolFlyout` |
| `ToolPinItemControl.axaml` | `ToolPinItemControlContextMenu` | Menu for pinned tool tabs. | `PinContextMenu` |
| `DocumentTabStripItem.axaml` | `DocumentTabStripItemContextMenu` | Menu for document tab items. | `DocumentContextMenu` |
| `ToolTabStripItem.axaml` | `ToolTabStripItemContextMenu` | Menu for tool tab items. | `TabContextMenu` |
| `MdiDocumentWindow.axaml` | `MdiDocumentWindowContextMenu` | Menu for MDI document windows. | `DocumentContextMenu` |

The string resources used for menu item headers are defined in `Controls/ControlStrings.axaml`. For example it exposes keys such as `ToolTabStripItemFloatString`, `ToolTabStripItemDockString` and others.

When a dock's `CanCloseLastDockable` property is set to `false` and only one dockable is visible, the built-in menus hide actions such as **Close** or **Float** using `CanRemoveDockableConverter`.

## Customizing menus and themes per control

Most Dock controls are created by templates, so set these properties via styles or control themes unless you have a direct instance reference. Each control exposes properties that allow you to customize the context menus, flyouts, and button themes for individual instances:

### ToolChromeControl
```csharp
// Set a custom flyout for a specific tool chrome control
myToolChromeControl.ToolFlyout = new MenuFlyout
{
    Items = 
    {
        new MenuItem { Header = "Custom Action", Command = myCommand }
    }
};

// Set custom themes for individual buttons
myToolChromeControl.CloseButtonTheme = new ControlTheme(typeof(Button))
{
    Setters = 
    {
        new Setter(Button.BackgroundProperty, Brushes.Red),
        new Setter(Button.ForegroundProperty, Brushes.White)
    }
};

myToolChromeControl.MaximizeButtonTheme = new ControlTheme(typeof(Button))
{
    Setters = 
    {
        new Setter(Button.BackgroundProperty, Brushes.Blue),
        new Setter(Button.ForegroundProperty, Brushes.White)
    }
};

myToolChromeControl.PinButtonTheme = new ControlTheme(typeof(Button))
{
    Setters =
    {
        new Setter(Button.BackgroundProperty, Brushes.DarkSlateGray),
        new Setter(Button.ForegroundProperty, Brushes.White)
    }
};

myToolChromeControl.MenuButtonTheme = new ControlTheme(typeof(Button))
{
    Setters =
    {
        new Setter(Button.BackgroundProperty, Brushes.DimGray),
        new Setter(Button.ForegroundProperty, Brushes.White)
    }
};
```

### ToolTabStripItem
```csharp
// Set a custom context menu for a specific tool tab
myToolTabStripItem.TabContextMenu = new ContextMenu
{
    Items = 
    {
        new MenuItem { Header = "Custom Action", Command = myCommand }
    }
};
```

### DocumentTabStripItem
```csharp
// Set a custom context menu for a specific document tab
myDocumentTabStripItem.DocumentContextMenu = new ContextMenu
{
    Items = 
    {
        new MenuItem { Header = "Custom Action", Command = myCommand }
    }
};
```

### MdiDocumentWindow
```csharp
// Set a custom context menu for a specific MDI document window
myMdiDocumentWindow.DocumentContextMenu = new ContextMenu
{
    Items =
    {
        new MenuItem { Header = "Custom Action", Command = myCommand }
    }
};

// Optional: theme the close button
myMdiDocumentWindow.CloseButtonTheme = new ControlTheme(typeof(Button))
{
    Setters =
    {
        new Setter(Button.BackgroundProperty, Brushes.Orange),
        new Setter(Button.ForegroundProperty, Brushes.White)
    }
};
```

### ToolPinItemControl
```csharp
// Set a custom context menu for a specific pin item
myToolPinItemControl.PinContextMenu = new ContextMenu
{
    Items = 
    {
        new MenuItem { Header = "Custom Action", Command = myCommand }
    }
};
```

### DocumentTabStrip
```csharp
// Set a custom theme for the create document button
myDocumentTabStrip.CreateButtonTheme = new ControlTheme(typeof(Button))
{
    Setters = 
    {
        new Setter(Button.BackgroundProperty, Brushes.Green),
        new Setter(Button.ForegroundProperty, Brushes.White),
        new Setter(Button.WidthProperty, 30.0),
        new Setter(Button.HeightProperty, 30.0)
    }
};
```

### DocumentControl
```csharp
// Set a custom theme for the document close button
myDocumentControl.CloseButtonTheme = new ControlTheme(typeof(Button))
{
    Setters = 
    {
        new Setter(Button.BackgroundProperty, Brushes.Orange),
        new Setter(Button.ForegroundProperty, Brushes.White),
        new Setter(Button.WidthProperty, 20.0),
        new Setter(Button.HeightProperty, 20.0)
    }
};
```

## Localizing strings

To translate the menu headers, add a resource dictionary to your application with the same string keys. Avalonia will resolve dynamic resources from the application scope first, so your localized values override the defaults:

```xaml
<Application.Resources>
    <ResourceDictionary>
        <x:String x:Key="ToolTabStripItemCloseString">Schließen</x:String>
        <x:String x:Key="DocumentTabStripItemCloseAllTabsString">Alle Tabs schließen</x:String>
    </ResourceDictionary>
</Application.Resources>
```

If you need to translate all menus, copy the string resources from the source dictionaries and provide localized versions for each key.

## Replacing entire menus globally

Because the controls refer to their menus using `DynamicResource`, you can supply completely new `ContextMenu` or `MenuFlyout` instances. Define a resource with the same key in your application resources:

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ContextMenu x:Key="ToolPinItemControlContextMenu">
            <MenuItem Header="Custom action" Command="{Binding MyCommand}"/>
        </ContextMenu>
    </ResourceDictionary>
</Application.Resources>
```

This approach allows you to customize the menu structure or attach your own commands without modifying Dock's source.

## Extensibility analysis

The current design provides multiple levels of customization:

1. **Per-control customization**: Use the control properties (`ToolFlyout`, `TabContextMenu`, `DocumentContextMenu`, `PinContextMenu`, `CreateButtonTheme`, `CloseButtonTheme`, etc.) to customize menus and themes for individual control instances.

2. **Global customization**: Override resource keys to replace menus for all instances of a control type.

3. **String localization**: Override string resources to translate menu headers.

4. **Template customization**: The controls use template binding for their menus, making them more efficient and allowing for easier customization.

The property-based approach provides the most flexibility for runtime customization, while the resource-based approach remains available for global changes. This design allows you to:

- Customize menus and button themes for specific controls while keeping the default for others
- Add or remove menu items dynamically
- Bind menu items to view model properties
- Create context-sensitive menus based on the control's state
- Customize button appearances (colors, sizes, hover effects) for individual controls
- Create themed buttons that match your application's design system

If you need to add items to the default menus rather than replacing them entirely, you can:

1. Create a custom menu that includes the default items plus your additions
2. Use the control properties to set this custom menu on specific instances
3. Or override the resource globally if you want the same additions everywhere
