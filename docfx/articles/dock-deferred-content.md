# Deferred Content Presentation

Dock can defer expensive content materialization to the next dispatcher render pass. This is useful when initial template resolution and later style resolution make document, tool, or window content noticeably heavy during first layout.

The feature lives in the `Dock.Controls.DeferredContentControl` package and exposes two hosts:

- `DeferredContentControl` for templates that can use a `ContentControl`.
- `DeferredContentPresenter` for templates that must keep a `ContentPresenter` contract.

## Adding the package

Include the package alongside your normal Dock references:

```bash
dotnet add package Dock.Controls.DeferredContentControl
```

## Using DeferredContentControl in custom themes

Use `DeferredContentControl` in places where a Dock theme would otherwise materialize heavy content immediately.

```xaml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:dcc="clr-namespace:Dock.Controls.DeferredContentControl;assembly=Dock.Controls.DeferredContentControl">
  <ControlTemplate x:Key="MyDocumentTemplate" TargetType="DocumentControl">
    <dcc:DeferredContentControl x:Name="PART_ContentPresenter"
                                Content="{Binding}"
                                HorizontalAlignment="Stretch"
                                VerticalAlignment="Stretch">
      <dcc:DeferredContentControl.ContentTemplate>
        <ControlRecyclingDataTemplate Parent="{Binding #PART_ContentPresenter}" />
      </dcc:DeferredContentControl.ContentTemplate>
    </dcc:DeferredContentControl>
  </ControlTemplate>
</ResourceDictionary>
```

This keeps the requested content and template, then applies them on the next dispatcher frame. Multiple content changes before the flush are batched into one materialization pass.

## Configuring the queue budget

The deferred queue is shared by all deferred hosts. You can configure it globally through `DeferredContentPresentationSettings`.

Count-based budget:

```csharp
DeferredContentPresentationSettings.BudgetMode = DeferredContentPresentationBudgetMode.ItemCount;
DeferredContentPresentationSettings.MaxPresentationsPerPass = 3;
```

Time-based budget:

```csharp
DeferredContentPresentationSettings.BudgetMode = DeferredContentPresentationBudgetMode.RealizationTime;
DeferredContentPresentationSettings.MaxRealizationTimePerPass = TimeSpan.FromMilliseconds(10);
```

Use item-count budgeting when you want predictable batching by host count. Use time-based budgeting when you want to cap the total realization time spent in one dispatcher pass.

## Using DeferredContentPresenter

Some Dock templates require the named part to remain a `ContentPresenter`. In those cases use `DeferredContentPresenter` instead of `DeferredContentControl`.

```xaml
<dcc:DeferredContentPresenter x:Name="PART_ContentPresenter"
                              Content="{TemplateBinding Content}"
                              ContentTemplate="{TemplateBinding ContentTemplate}"
                              Margin="{TemplateBinding Padding}"
                              HorizontalContentAlignment="{TemplateBinding HorizontalContentAlignment}"
                              VerticalContentAlignment="{TemplateBinding VerticalContentAlignment}" />
```

This is the right choice for hosts such as custom chrome windows or any template that relies on a `ContentPresenter`-typed part.

## Opting out for specific content

If a content object must stay synchronous, implement `IDeferredContentPresentation` and return `false`.

```csharp
public sealed class ManagedDockWindowDocument : IDeferredContentPresentation
{
    bool IDeferredContentPresentation.DeferContentPresentation => false;
}
```

Dock uses this for managed floating-window content that should not be delayed.

## Built-in theme behavior

The Fluent and Simple themes use deferred content presentation for the main heavy content hosts, including document, tool, MDI, split-view, pinned, root, and host-window content paths. Cached document tab content stays eager by design because that path intentionally prebuilds hidden tabs.

## Related guides

- [Control recycling](dock-control-recycling.md)
- [Custom Dock themes](dock-custom-theme.md)
- [Styling and theming](dock-styling.md)
