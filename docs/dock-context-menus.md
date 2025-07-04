# Context menus and flyouts

Dock defines several built in context menus and flyouts that are attached to its controls. The menu text is stored in the same resource dictionaries as `MenuFlyout` or `ContextMenu` definitions. This document lists the available menus and describes how to localize or replace them.

## List of built in menus

| File | Resource key | Purpose |
| ---- | ------------ | ------- |
| `ToolChromeControl.axaml` | `ToolChromeControlContextMenu` | Menu for tool chrome grip button. |
| `ToolPinItemControl.axaml` | `ToolPinItemControlContextMenu` | Menu for pinned tool tabs. |
| `DocumentTabStripItem.axaml` | `DocumentTabStripItemContextMenu` | Menu for document tab items. |
| `ToolTabStripItem.axaml` | `ToolTabStripItemContextMenu` | Menu for tool tab items. |

Each dictionary also declares `x:String` resources used for menu item headers. For example `ToolTabStripItem.axaml` exposes keys such as `ToolTabStripItemFloatString`, `ToolTabStripItemDockString` and others.

When a dock's `CanCloseLastDockable` property is set to `false` the built-in menus automatically disable commands like **Close** or **Float** if executing them would remove the final item from that dock.

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

## Replacing entire menus

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

The current design relies on static resources. Replacing or localizing items is straightforward by overriding resource keys, but adding items dynamically requires providing a completely new menu. There are no hooks to inject menu items at runtime.

If extensibility is important for your application, consider wrapping the default menus in your own `ContextMenu` definitions so you can append items in XAML. Alternatively you could fork the resource dictionaries and modify them to expose extension points via data templates or bindings.

