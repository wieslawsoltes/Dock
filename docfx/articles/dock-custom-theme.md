# Custom Dock Themes

This guide walks through creating a theme file from scratch. Use it when the built-in Fluent and Simple themes do not match your application branding.

## 1. Create an accent dictionary

Define brushes and colors in a new `.axaml` file. Start from
`src/Dock.Avalonia.Themes.Fluent/Accents/Fluent.axaml` to see the full set of
resource keys used by the built-in templates:

```xaml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="DockThemeBackgroundBrush" Color="#202020" />
    <SolidColorBrush x:Key="DockThemeAccentBrush" Color="#675EDC" />
    <SolidColorBrush x:Key="DockThemeForegroundBrush" Color="#EEEEEE" />
    <SolidColorBrush x:Key="DockToolChromeIconBrush" Color="#474747" />
    <StreamGeometry x:Key="DockIconCloseGeometry" />
    <StreamGeometry x:Key="DockToolIconCloseGeometry" />
</ResourceDictionary>
```

`StreamGeometry` resources like `DockIconCloseGeometry` let you replace the built-in vector icons.

## 2. Merge Dock control styles

Create another `.axaml` file that merges the Dock controls together with your
accent dictionary. The Simple theme reuses the Fluent control templates, so you
can reference them directly via `avares://Dock.Avalonia.Themes.Fluent/Controls/...`
or copy them into your own assembly and include `/Controls/...` resources.

```xaml
<Styles xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="MyApp.MyDockTheme">
    <Styles.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceInclude Source="avares://Dock.Controls.ProportionalStackPanel/ProportionalStackPanelSplitter.axaml" />
                <ResourceInclude Source="avares://Dock.Avalonia.Themes.Fluent/Controls/ControlStrings.axaml" />
                <ResourceInclude Source="avares://MyApp/Styles/MyDockAccent.axaml" />
                <ResourceInclude Source="avares://Dock.Avalonia.Themes.Fluent/Controls/DockControl.axaml" />
                <ResourceInclude Source="avares://Dock.Avalonia.Themes.Fluent/Controls/ToolControl.axaml" />
                <!-- include additional Dock controls as desired -->
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Styles.Resources>
</Styles>
```

If your copied templates host expensive document or tool content, add the
`Dock.Controls.DeferredContentControl` package and replace eager content hosts
with `DeferredContentControl` or `DeferredContentPresenter`. See the
[Deferred content presentation](dock-deferred-content.md) guide for the theme
patterns used by the built-in Dock themes.

## 3. Apply the theme

Reference the theme from `App.axaml`:

```xaml
<Application.Styles>
    <FluentTheme Mode="Dark" />
    <local:MyDockTheme />
</Application.Styles>
```

Your Dock layout now uses the brushes defined in `MyDockAccent.axaml`. You can further customize control templates by copying them from the Dock source and adjusting the XAML. When editing templates remember to set the [DockProperties](dock-properties.md) so that drag and drop continues to work.

## Migrating custom docking indicators to projected bounds

Dock can project the resulting pane after a move, including source-pane collapse
and nested splits. Fluent and Simple include the new indicator; Browser inherits
it from Fluent. Applications that replace `DockTarget` or `GlobalDockTarget`
templates must update both overrides to display projected bounds. Existing
templates continue to load and use their legacy indicators.

Keep the existing named indicator and selector parts. Inside the template, wrap
the existing content and the following overlay in a `Panel`:

```xaml
<Canvas IsHitTestVisible="False">
  <Border x:Name="PART_PreviewIndicator" Opacity="0.5"
          Canvas.Left="{TemplateBinding PreviewX}"
          Canvas.Top="{TemplateBinding PreviewY}"
          Width="{TemplateBinding PreviewWidth}"
          Height="{TemplateBinding PreviewHeight}"
          Background="{DynamicResource DockTargetIndicatorBrush}" />
</Canvas>
```

Add these styles inside the `ControlTheme`:

```xaml
<Setter Property="ClipToBounds" Value="False" />
<Style Selector="^/template/ Border#PART_PreviewIndicator">
  <Setter Property="IsVisible" Value="False" />
</Style>
<Style Selector="^:preview /template/ Border#PART_PreviewIndicator">
  <Setter Property="IsVisible" Value="True" />
</Style>
<Style Selector="^:preview /template/ Panel#PART_LeftIndicator">
  <Setter Property="IsVisible" Value="False" />
</Style>
```

Repeat the last style for `PART_RightIndicator`, `PART_TopIndicator`,
`PART_BottomIndicator`, and the local target's `PART_CenterIndicator`. Keep the
selectors available. Do not clip the overlay or its parent: source collapse can
move the proposed rectangle outside the old hovered pane. The `:preview`
pseudo-class is removed when projection is unavailable, restoring legacy
indicators. Custom outlines can replace the background with their own border
brush, thickness and corner radius. Selectors styling the old indicator panels
must also style `PART_PreviewIndicator` to affect the new highlight.

### Size constraints and compatibility

Put pane `MinWidth`, `MaxWidth`, `MinHeight` and `MaxHeight` on the Dock models,
and retain the theme bindings to those properties. The projection measures model
constraints; it does not instantiate application widget templates. If a realized
tool or document pane overrides these constraints through styles or local values,
Dock conservatively retains legacy indicators. Changes to this eligibility
invalidate cached previews, including the check immediately before release.

This fallback preserves existing docking behavior; it does not guarantee that a
legacy indicator matches the final pane. To enable projected bounds, move those
constraints into the models. Arbitrary custom template geometry and per-position
styles are outside the projection contract. Derived factories also need the
explicit opt-in described in [Custom Dock Models](dock-custom-model.md#isolated-docking-preview-factories).
