using System;
using Avalonia;
using Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Let Dock measure, arrange, and resize its native children after translating
// public pane dimensions against this panel's actual available space.
internal sealed class LayoutSizingPanel : ProportionalStackPanel
{
    private readonly LayoutSizing _sizing = new();
    private LayoutSizingViews? _views;

    static LayoutSizingPanel() => AffectsMeasure<LayoutSizingPanel>(DataContextProperty,
        LayoutSplitterSizing.WidthProperty, LayoutSplitterSizing.HeightProperty);

    protected override void OnAssignProportions(Size size, double splitterThickness)
    {
        double available = base.GetProportionalDimension(size, splitterThickness);
        if (GetContainer() is not { } group || !_sizing.Apply(group, available, GetProportionalDimension(size, splitterThickness)))
            base.OnAssignProportions(size, splitterThickness);
    }

    // Fixed columns can exceed a constrained viewport, even when the splitter
    // gaps consume all available space. A positive denominator preserves them.
    protected override double GetProportionalDimension(Size size, double splitterThickness) =>
        GetContainer() is not null ? Math.Max(1, base.GetProportionalDimension(size, splitterThickness))
            : base.GetProportionalDimension(size, splitterThickness);

    protected override void ResizeProportions(Control target, Control neighbor, double dragDelta, double minimumSize)
    {
        if (!LayoutSplitterResize.TryResize(target, neighbor, Orientation, dragDelta, minimumSize))
            base.ResizeProportions(target, neighbor, dragDelta, minimumSize);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Register();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _views?.Remove(this);
        _views = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataContextProperty && TopLevel.GetTopLevel(this) is not null) Register();
    }

    private ILayoutContainer? GetContainer() =>
        DataContext is IProportionalDock { Factory: LayoutFactory factory } model ? factory.GetLayoutContainer(model) : null;

    private void Register()
    {
        _views?.Remove(this);
        _views = GetContainer()?.Root?.Manager?.SizingViews;
        _views?.Add(this);
    }
}
