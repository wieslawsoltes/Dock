# Styling and Theming

This guide demonstrates how to customize the appearance of Dock controls using Avalonia styles. It shows how to apply built-in themes, override style resources and tweak control templates.

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

`ToolChromeControl` defines an extra brush for its icons:

```xaml
<SolidColorBrush x:Key="DockToolChromeIconBrush" Color="#474747" />
```

Include replacements for these in your accent dictionary to change icon or button colors.

Several icons are also exposed as `StreamGeometry` resources so you can swap them with your own glyphs:

```xaml
<StreamGeometry x:Key="DockIconCloseGeometry" />
<StreamGeometry x:Key="DockToolIconCloseGeometry" />
<StreamGeometry x:Key="DockIconAddDocumentGeometry" />
```

Update these geometries to customize the plus, close or other tool icons used throughout Dock.

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

```text
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

## Tab and window templates

Dock exposes template properties so you can control tab headers, icons, and modified indicators:

| Control | Properties | Notes |
| --- | --- | --- |
| `DocumentControl` | `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate`, `CloseTemplate` | Used for document tabs in tabbed mode. |
| `ToolControl` | `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate` | Used for tool tabs. |
| `MdiDocumentControl` | `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate`, `CloseTemplate` | Forwarded to `MdiDocumentWindow` in MDI mode. |
| `MdiDocumentWindow` | `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate`, `CloseTemplate` | Used in MDI window chrome. |

Example overriding the modified indicator:

```xaml
<Style Selector="DocumentControl">
  <Setter Property="ModifiedTemplate">
    <DataTemplate>
      <TextBlock Text="*" Foreground="{DynamicResource DockThemeAccentBrush}" />
    </DataTemplate>
  </Setter>
</Style>
```

## Template parts and pseudo classes

Dock controls mark important elements in their templates with `PART_` names. When
copying these templates keep the parts intact so features such as window dragging
continue to work. For example `ToolChromeControl` defines `PART_Grip`,
`PART_CloseButton` and `PART_MaximizeRestoreButton`, while `HostWindow` exposes
`PART_TitleBar`.

Controls also toggle pseudo classes to reflect their current state. These can be
targeted in selectors to customize the appearance:

- `DocumentControl`, `DocumentTabStrip`, and `DocumentTabStripItem` use `:active`.
- `DocumentTabStrip` and `ToolTabStrip` apply `:create` when new items can be
  added.
- `ToolChromeControl` sets `:active`, `:pinned`, `:floating` and `:maximized`.
- `HostWindow` toggles `:toolwindow`, `:dragging`, `:toolchromecontrolswindow` and
  `:documentchromecontrolswindow`.
- `ProportionalStackPanelSplitter` uses `:horizontal` or `:vertical` depending on
  orientation, and `:preview` while preview resizing is active.

These pseudo classes are driven by properties such as `IsActive` and
`CanCreateItem` on the respective controls.

`DockControl` also exposes `IsDraggingDock`, which is toggled during drag
operations. Theme styles can bind to it to disable hit testing or adjust
appearance while dragging.

Refer to the source code for the complete list of parts and classes.

## Drag preview control

`DragPreviewControl` renders the floating preview during a drag. It exposes:

- `ContentTemplate` to render the dragged dockable.
- `Status` set to `Dock`, `Float`, or `None`, which the default theme uses to show status icons and strings.

You can swap the content template in a theme:

```xaml
<Style Selector="DragPreviewControl">
  <Setter Property="ContentTemplate">
    <DataTemplate>
      <TextBlock Text="{Binding Title}" FontWeight="SemiBold" />
    </DataTemplate>
  </Setter>
</Style>
```

## Creating a custom theme

While overriding individual resources works for small tweaks, you can also define an entirely custom theme. Dock themes are ordinary `Styles` files that merge resource dictionaries. Create a new `.axaml` file and merge the Dock control styles along with your own accent resources.

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
                <!-- include other Dock controls as needed -->
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Styles.Resources>
</Styles>
```

`MyDockAccent.axaml` defines brushes used by the templates:

```xaml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="DockThemeBackgroundBrush" Color="#202020" />
    <SolidColorBrush x:Key="DockThemeAccentBrush" Color="#675EDC" />
</ResourceDictionary>
```

Reference your theme instead of `DockFluentTheme` in `App.axaml`:

```xaml
<Application.Styles>
    <FluentTheme Mode="Dark" />
    <local:MyDockTheme />
</Application.Styles>
```

See [Dock properties](dock-properties.md) for details on setting attached properties when creating custom templates.

For a complete walkthrough see [Custom themes](dock-custom-theme.md).
