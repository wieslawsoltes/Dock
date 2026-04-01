# Deferred Content Presentation

Dock can defer expensive content materialization so presenter-heavy layouts do not pay for template creation, logical attachment, and style activation inside the first measure burst.

The feature lives in the `Dock.Controls.DeferredContentControl` package and exposes:

- `DeferredContentControl` for templates that can use a `ContentControl`.
- `DeferredContentPresenter` for templates that must keep a `ContentPresenter` contract.
- `DeferredContentPresentationTimeline` for shared scoped queues.
- `DeferredContentScheduling` for inherited `Timeline`, `Delay`, and `Order` attached properties.

## Add the package

```bash
dotnet add package Dock.Controls.DeferredContentControl
```

## Default behavior

If you do nothing beyond replacing an eager content host with `DeferredContentControl` or `DeferredContentPresenter`, the package keeps the current default behavior:

- a shared default queue,
- next-pass presentation,
- FIFO ordering for equal items,
- count-based batching through `DeferredContentPresentationSettings`,
- a short opacity reveal to smooth later deferred content swaps without hiding first paint.

That keeps existing Dock theme behavior unchanged.

## Use DeferredContentControl

Use `DeferredContentControl` in theme templates that do not require a `ContentPresenter`-typed part:

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

The host keeps the latest `Content` and `ContentTemplate`, then forwards them to its inner presenter when the deferred queue grants that target a turn.

## Use DeferredContentPresenter

Some templates must keep a `ContentPresenter` contract. In that case, use `DeferredContentPresenter` directly:

```xaml
<dcc:DeferredContentPresenter x:Name="PART_ContentPresenter"
                              Content="{TemplateBinding Content}"
                              ContentTemplate="{TemplateBinding ContentTemplate}"
                              Margin="{TemplateBinding Padding}"
                              HorizontalContentAlignment="{TemplateBinding HorizontalContentAlignment}"
                              VerticalContentAlignment="{TemplateBinding VerticalContentAlignment}" />
```

This is the right choice for host windows, chrome templates, or any control contract that names the part as a `ContentPresenter`.

## Configure the default timeline

`DeferredContentPresentationSettings` now configures the shared default timeline:

```csharp
DeferredContentPresentationSettings.BudgetMode = DeferredContentPresentationBudgetMode.ItemCount;
DeferredContentPresentationSettings.MaxPresentationsPerPass = 3;
DeferredContentPresentationSettings.InitialDelay = TimeSpan.Zero;
DeferredContentPresentationSettings.FollowUpDelay = TimeSpan.FromMilliseconds(16);
DeferredContentPresentationSettings.RevealDuration = TimeSpan.FromMilliseconds(90);
```

For a realization-time budget:

```csharp
DeferredContentPresentationSettings.BudgetMode = DeferredContentPresentationBudgetMode.RealizationTime;
DeferredContentPresentationSettings.MaxRealizationTimePerPass = TimeSpan.FromMilliseconds(10);
DeferredContentPresentationSettings.FollowUpDelay = TimeSpan.FromMilliseconds(33);
DeferredContentPresentationSettings.RevealDuration = TimeSpan.FromMilliseconds(90);
```

Properties:

- `BudgetMode`
- `MaxPresentationsPerPass`
- `MaxRealizationTimePerPass`
- `InitialDelay`
- `FollowUpDelay`
- `RevealDuration`
- `DefaultTimeline`

Set `RevealDuration` to `TimeSpan.Zero` to disable the reveal transition.

## Scope a timeline to a subtree

Create a `DeferredContentPresentationTimeline` resource and attach it to a container with `DeferredContentScheduling.Timeline`.

```xaml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:dcc="clr-namespace:Dock.Controls.DeferredContentControl;assembly=Dock.Controls.DeferredContentControl">
  <Window.Resources>
    <dcc:DeferredContentPresentationTimeline x:Key="OrderedTimeline"
                                             MaxPresentationsPerPass="1"
                                             FollowUpDelay="0:0:0.10" />
  </Window.Resources>

  <StackPanel dcc:DeferredContentScheduling.Timeline="{StaticResource OrderedTimeline}">
    <dcc:DeferredContentControl Content="{Binding PrimaryDocument}" />
    <dcc:DeferredContentControl Content="{Binding SecondaryDocument}" />
  </StackPanel>
</Window>
```

Every deferred host in that subtree shares the same scoped queue. A different subtree can attach a different timeline and realize independently.

## Set per-host delay and order

`DeferredContentScheduling.Delay` and `DeferredContentScheduling.Order` are inheritable attached properties. You can set them on a scope root or on individual hosts.

```xaml
<StackPanel dcc:DeferredContentScheduling.Timeline="{StaticResource OrderedTimeline}">
  <dcc:DeferredContentControl Content="{Binding First}"
                              dcc:DeferredContentScheduling.Order="-10"
                              dcc:DeferredContentScheduling.Delay="0:0:0.00" />

  <dcc:DeferredContentControl Content="{Binding Second}"
                              dcc:DeferredContentScheduling.Order="5"
                              dcc:DeferredContentScheduling.Delay="0:0:0.04" />

  <dcc:DeferredContentControl Content="{Binding Third}"
                              dcc:DeferredContentScheduling.Order="20"
                              dcc:DeferredContentScheduling.Delay="0:0:0.12" />
</StackPanel>
```

Rules:

- lower `Order` values realize first,
- equal `Order` values preserve FIFO behavior,
- `Delay` adds per-host time on top of the timeline's `InitialDelay`,
- scoped order and delay work in both count-based and time-based budgets.

If you configure a non-zero `InitialDelay` or large per-host `Delay` values, a host can legitimately remain blank until its scheduled turn. Use smaller delays, or stage visible placeholder UI outside the deferred host, when first-paint completeness matters.

## How scopes and budgets interact

Every `DeferredContentPresentationTimeline` owns a separate queue and scheduler. That means you can mix different policies in one app:

- one subtree can use `ItemCount` batching,
- another subtree can use `RealizationTime` batching,
- each subtree can use different `InitialDelay` and `FollowUpDelay`,
- per-host `Delay` and `Order` still apply inside each scope.

This makes deferred loading composable in the same way Dock scopes control recycling.

## Smooth the deferred reveal

Deferred loading can still cause a visible pop when content arrives on a later dispatcher turn. The package smooths later deferred content swaps by fading the presenter in over `RevealDuration`.

The first realization from a blank host stays immediate so startup and structural dock hosts are not hidden behind an extra opacity handoff.

That improves the perceived refresh, but it does not change measurement correctness. If a host is auto-sized and its true size is unknown until the content is materialized, layout can still change when the real content arrives. In those cases, use one or more of:

- explicit width or height on the deferred host,
- placeholder chrome outside the deferred host,
- smaller timeline delays so the content lands closer to the initial layout burst.

## Opt out for specific content

If a content object must stay synchronous, implement `IDeferredContentPresentation` and return `false`.

```csharp
public sealed class ManagedDockWindowDocument : IDeferredContentPresentation
{
    bool IDeferredContentPresentation.DeferContentPresentation => false;
}
```

Dock uses this for managed floating-window content that should not be delayed.

## Built-in Dock theme behavior

The Fluent and Simple Dock themes already use deferred presentation for the heavy content hosts:

- document content,
- tool content,
- document and tool active presenters,
- MDI document content,
- split-view content,
- pinned content,
- root and dock-level active hosts,
- host-window and chrome presenter paths.

Cached document tab content stays eager by design because that path intentionally prebuilds hidden tabs.

## Sample

The repository includes a focused sample that demonstrates:

- the shared default timeline,
- scoped timelines,
- inherited `Delay`,
- inherited `Order`,
- `DeferredContentControl`,
- `DeferredContentPresenter` through a presenter-contract templated host,
- count-based and time-based budget modes.

See [Deferred content sample](dock-deferred-content-sample.md).

Run it with:

```bash
dotnet run --project samples/DockDeferredContentSample/DockDeferredContentSample.csproj
```

## Related guides

- [Deferred content sample](dock-deferred-content-sample.md)
- [Control recycling](dock-control-recycling.md)
- [Custom Dock themes](dock-custom-theme.md)
- [Styling and theming](dock-styling.md)
