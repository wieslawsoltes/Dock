# Control Recycling Guide

Dock's control recycling system reuses visual elements created for dockables. When a dockable reappears in the layout the cached control is returned instead of instantiating a new one. This helps retain control state and reduces visual churn when documents or tools are moved around.

## Adding the package

Include the `Dock.Controls.Recycling` NuGet package alongside your existing Dock references. It provides the `ControlRecycling` and `ControlRecyclingDataTemplate` types used to enable the feature.

## Enabling recycling in XAML

Define a `ControlRecycling` resource and assign it to `DockControl` using the `ControlRecyclingDataTemplate.ControlRecycling` attached property.

```xml
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

With this in place the controls generated for each dockable are kept in an internal cache. When the same view model is presented again the cached instance is reused.

## Using IDs for caching

`ControlRecycling` can store controls by arbitrary keys. Dockables implement `IControlRecyclingIdProvider` which exposes `GetControlRecyclingId` returning their `Id`. Setting `TryToUseIdAsKey="True"` uses this identifier for the cache key.

```xml
<ControlRecycling x:Key="ControlRecyclingKey" TryToUseIdAsKey="True" />
```

This allows the same dockable to be recognised even if the instance itself changes, for example after deserializing a layout.

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

```xml
<DockControl Items="{Binding Documents}" />
```

As documents are opened and closed the same `DocumentControl` is reused for a given `DocumentViewModel`. If needed the cache can be cleared manually:

```csharp
controlRecycling.Clear();
```

Control recycling works for both MVVM and ReactiveUI samples as well as the XAML‑only approach. Inspect the `App.axaml` files under the `samples` directory for complete working examples.

For an overview of all guides see the [documentation index](README.md).
