# Localization and string resources

Dock controls use dynamic resources for menu text and other UI labels. All text properties are decorated with localization attributes so the values can be translated.
The default strings ship in `ControlStrings.axaml` under the `Dock.Avalonia` assembly.

## Overriding strings

Override any built in string by adding a resource dictionary with the same keys in your application:

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceInclude Source="avares://Dock.Avalonia/Controls/ControlStrings.axaml" />
        <x:String x:Key="ToolTabStripItemCloseString">Schließen</x:String>
        <x:String x:Key="DocumentTabStripItemCloseAllTabsString">Alle Tabs schließen</x:String>
    </ResourceDictionary>
</Application.Resources>
```

Avalonia resolves dynamic resources from the application scope first so your values override the defaults.

## See also

- [Context menus](dock-context-menus.md) – Customize or replace built in menus.
