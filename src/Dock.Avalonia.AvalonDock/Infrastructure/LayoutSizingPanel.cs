using System;
using Avalonia;
using Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Let Dock measure, arrange, and resize its native children after translating
// public pane dimensions against this panel's actual available space.
internal sealed class LayoutSizingPanel : ProportionalStackPanel
{
    private readonly LayoutSizing _sizing = new();
    private readonly LayoutIntrinsicSizing _intrinsic = new();
    private LayoutSizingViews? _views;

    static LayoutSizingPanel() => AffectsMeasure<LayoutSizingPanel>(DataContextProperty,
        LayoutSplitterSizing.WidthProperty, LayoutSplitterSizing.HeightProperty);

    protected override Size GetMeasureConstraint(Size constraint, double splitterThickness)
    {
        if (GetContainer() is null) return base.GetMeasureConstraint(constraint, splitterThickness);
        double natural = _intrinsic.Measure(Children, constraint, Orientation) + splitterThickness;
        bool horizontal = Orientation == global::Avalonia.Layout.Orientation.Horizontal;
        if (!double.IsPositiveInfinity(horizontal ? constraint.Width : constraint.Height)) return constraint;
        return horizontal ? constraint.WithWidth(natural) : constraint.WithHeight(natural);
    }

    protected override Size GetChildMeasureConstraint(Control child, Size constraint)
    {
        if (child.DataContext is IDockable { Factory: LayoutFactory factory } native
            && factory.GetLayoutContainer(native) is ILayoutPositionableGroup model && _intrinsic.IsIntrinsic(model))
            return Orientation == global::Avalonia.Layout.Orientation.Horizontal
                ? constraint.WithWidth(double.PositiveInfinity) : constraint.WithHeight(double.PositiveInfinity);
        return constraint;
    }

    protected override void OnAssignProportions(Size size, double splitterThickness)
    {
        double available = base.GetProportionalDimension(size, splitterThickness);
        if (GetContainer() is not { } group || !_sizing.Apply(group, available, GetProportionalDimension(size, splitterThickness), _intrinsic))
        {
            base.OnAssignProportions(size, splitterThickness);
            return;
        }
        // Theme replacement can reset a generated presenter's attached value
        // while its model keeps the same proportion and raises no notification.
        // Assign the layout inputs directly, preserving any application binding.
        for (int i = 0; i < Children.Count; i++)
            if (Children[i].DataContext is IDockable model and not ISplitter)
                Children[i].SetCurrentValue(ProportionProperty, model.Proportion);
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
        _intrinsic.Clear();
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
