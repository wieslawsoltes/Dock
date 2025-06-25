# Styling and Theming

This guide demonstrates how to customize the appearance of Dock controls using Avalonia styles. It shows how to apply built‑in themes, override style resources and tweak control templates.

## Applying a theme

Dock ships with **Fluent** and **Simple** themes. Include one of them in `App.axaml` together with an Avalonia base theme:

```xaml
<Application.Styles>
  <FluentTheme Mode="Dark" />
  <DockFluentTheme />
</Application.Styles>
```

The `DockFluentTheme` merges all default styles for the docking controls. Use `DockSimpleTheme` if you prefer lighter accents.

## Overriding resources

You can override brushes and other resources to match your application colors:

```xaml
<Application.Resources>
  <Color x:Key="RegionColor">#FF1E1E1E</Color>
  <Color x:Key="SystemAccentColor">#FF007ACC</Color>
  <SolidColorBrush x:Key="DockThemeBackgroundBrush"
                  Color="{StaticResource RegionColor}" />
</Application.Resources>
```

Any `SolidColorBrush` referenced by the theme can be replaced this way. Controls automatically pick up the new values.

## Custom control styles

Specific Dock controls can also be styled by selector. The sample applications set custom headers like this:

```xaml
<Style Selector="DocumentControl">
  <Setter Property="HeaderTemplate">
    <DataTemplate DataType="core:IDockable">
      <StackPanel Orientation="Horizontal">
        <PathIcon Data="M5 1L13 9V15H3V1Z" Width="16" Height="16" />
        <TextBlock Text="{Binding Title}" Padding="4,0,0,0" />
      </StackPanel>
    </DataTemplate>
  </Setter>
</Style>
```

Create additional styles for `ToolControl`, `ToolChromeControl` or any other Dock control to adjust fonts, padding and colors.

```
-----------------------------------------------
| Tool |     Document      |      Tool       |
-----------------------------------------------
|                                         |
|     [ dark themed document content ]    |
|                                         |
-----------------------------------------------
```

The ASCII representation above shows a dark themed layout with custom colors applied.

For a deeper look at Dock internals see the [Deep Dive](dock-deep-dive.md) guide.
