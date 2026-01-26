# Control Recycling Guide

Dock's control recycling system reuses visual elements created for dockables. When a dockable reappears in the layout the cached control is returned instead of instantiating a new one. This helps retain control state and reduces visual churn when documents or tools are moved around.

## Adding the package

Include the `Dock.Controls.Recycling` NuGet package alongside your existing Dock references. It provides the `ControlRecycling` and `ControlRecyclingDataTemplate` types used to enable the feature.

## Enabling recycling in XAML

Define a `ControlRecycling` resource and assign it to `DockControl` using the `ControlRecyclingDataTemplate.ControlRecycling` attached property. The attached property is inheritable, so setting it on the `DockControl` makes it available to the internal content presenters that opt into recycling.

```xaml
<Application.Resources>
  <ControlRecycling x:Key="ControlRecyclingKey" />
</Application.Resources>

<Application.Styles>
  <Style Selector="DockControl">
    <Setter Property="(ControlRecyclingDataTemplate.ControlRecycling)"
            Value="{StaticResource ControlRecyclingKey}" />
  </Style>
</Application.Styles>
```

With this in place the controls generated for each dockable are kept in an internal cache. When the same dockable data is presented again the cached view control is reused.

## Using IDs for caching

`ControlRecycling` can store controls by arbitrary keys. Dockables implement `IControlRecyclingIdProvider` which exposes `GetControlRecyclingId` returning their `Id`. Setting `TryToUseIdAsKey="True"` uses this identifier for the cache key.

```xaml
<ControlRecycling x:Key="ControlRecyclingKey" TryToUseIdAsKey="True" />
```

This allows the same dockable to be recognized even if the instance itself changes, for example after deserializing a layout. Ensure `Id` values are stable and unique within a layout.

## Example

The sample applications ship with recycling enabled. A minimal MVVM setup looks like this:

```csharp
public class DocumentViewModel : Dock.Model.Mvvm.Controls.DockableBase
{
    public DocumentViewModel(string id, string title)
    {
        Id = id;
        Title = title;
    }
}
```

```xaml
<DockControl Layout="{Binding Layout}"
             InitializeLayout="True"
             InitializeFactory="True" />
```

As documents are opened and closed the same view control is reused for a given `DocumentViewModel`. If needed the cache can be cleared manually:

```csharp
controlRecycling.Clear();
```

### XAML-only layouts

Control recycling also applies when the layout is declared purely in XAML. Dockables defined in markup can specify an `Id` which the recycling cache can use (via `TryToUseIdAsKey`) to match and reuse visuals even after a reload. This lets tools and documents retain their internal state even when no view models or bindings are present.

```xaml
<DockControl x:Name="Dock" InitializeLayout="True" InitializeFactory="True">
  <RootDock>
    <DocumentDock>
      <Document Id="Doc1" Title="Welcome">
        <TextBox Text="Edit me" />
      </Document>
    </DocumentDock>
    <ToolDock>
      <Tool Id="Tool1" Title="Settings">
        <Slider Minimum="0" Maximum="100" Value="40" />
      </Tool>
    </ToolDock>
  </RootDock>
</DockControl>
```

With recycling enabled the `Document` and `Tool` above keep their content each time they are reopened or toggled. Even without bindings the `TextBox` and `Slider` values persist. Control recycling works for both MVVM and ReactiveUI samples as well as the XAML-only approach. Inspect the `App.axaml` files under the `samples` directory for complete working examples.

### Custom templates

Recycling is enabled by default in the Fluent and Simple theme templates because they include `ControlRecyclingDataTemplate` in the document and tool content presenters. If you build a custom control theme, include `ControlRecyclingDataTemplate` in your content presenter template so the cache can participate.

For an overview of all guides see the [documentation index](README.md).
