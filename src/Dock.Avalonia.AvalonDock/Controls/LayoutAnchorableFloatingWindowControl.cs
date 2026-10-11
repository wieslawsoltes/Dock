using System;
using Avalonia;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>A native Dock host presenting an AvalonDock floating tool group.</summary>
public class LayoutAnchorableFloatingWindowControl : LayoutFloatingWindowControl
{
    /// <summary>Defines the item container selected in the only visible tool pane.</summary>
    public static readonly StyledProperty<LayoutItem?> SingleContentLayoutItemProperty =
        AvaloniaProperty.Register<LayoutAnchorableFloatingWindowControl, LayoutItem?>(nameof(SingleContentLayoutItem));

    private readonly LayoutAnchorableFloatingWindow _model;

    internal LayoutAnchorableFloatingWindowControl(LayoutAnchorableFloatingWindow model, bool isContentImmutable = false)
        : base(model, isContentImmutable) => _model = model;

    /// <inheritdoc/>
    public override ILayoutElement Model => _model;
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutAnchorableFloatingWindowControl);
    /// <summary>Gets or sets the container selected in a single visible tool pane.</summary>
    public LayoutItem? SingleContentLayoutItem
    {
        get => GetValue(SingleContentLayoutItemProperty);
        set => SetValue(SingleContentLayoutItemProperty, value);
    }
    /// <summary>Called when the selected single-pane container changes.</summary>
    protected virtual void OnSingleContentLayoutItemChanged(AvaloniaPropertyChangedEventArgs e) { }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SingleContentLayoutItemProperty) OnSingleContentLayoutItemChanged(change);
    }
}
