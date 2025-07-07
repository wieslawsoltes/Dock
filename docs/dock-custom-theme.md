# Custom Dock Themes

This guide walks through creating a theme file from scratch. Use it when the built‑in Fluent and Simple themes do not match your application branding.

## 1. Create an accent dictionary

Define brushes and colors in a new `.axaml` file:

```xaml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="DockThemeBackgroundBrush" Color="#202020" />
    <SolidColorBrush x:Key="DockThemeAccentBrush" Color="#675EDC" />
    <SolidColorBrush x:Key="DockThemeForegroundBrush" Color="#EEEEEE" />
    <SolidColorBrush x:Key="DockToolChromeIconBrush" Color="#474747" />
    <SolidColorBrush x:Key="DockToolChromeGripBrush" Color="#474747" />
</ResourceDictionary>
```

## 2. Merge Dock control styles

Create another `.axaml` file that merges the Dock controls together with your accent dictionary:

```xaml
<Styles xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="MyApp.MyDockTheme">
    <Styles.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceInclude Source="avares://Dock.Controls.ProportionalStackPanel/ProportionalStackPanelSplitter.axaml" />
                <ResourceInclude Source="avares://MyApp/Styles/MyDockAccent.axaml" />
                <ResourceInclude Source="/Controls/DockControl.axaml" />
                <ResourceInclude Source="/Controls/ToolControl.axaml" />
                <!-- include additional Dock controls as desired -->
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Styles.Resources>
</Styles>
```

## 3. Apply the theme

Reference the theme from `App.axaml`:

```xaml
<Application.Styles>
    <FluentTheme Mode="Dark" />
    <local:MyDockTheme />
</Application.Styles>
```

Your Dock layout now uses the brushes defined in `MyDockAccent.axaml`. You can further customise control templates by copying them from the Dock source and adjusting the XAML. When editing templates remember to set the [DockProperties](dock-properties.md) so that drag and drop continues to work.
