using System;
using Avalonia;
using Avalonia.Controls;
using Orientation = Avalonia.Layout.Orientation;
using Avalonia.Media;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Core;
using Dock.Settings;

namespace AvalonDock.Controls;

/// <summary>Base grid exposing the panes and native splitters of a public split container.</summary>
/// <typeparam name="T">The public child type accepted by the container.</typeparam>
public abstract class LayoutGridControl<T> : Grid, ILayoutControl, ILayoutGridView, IDockableControl, IProportionalSplitterHost where T : class, ILayoutPanelElement
{
    static LayoutGridControl() => BackgroundProperty.OverrideDefaultValue<LayoutGridControl<T>>(Brushes.Transparent);

    private readonly LayoutPositionableGroup<T> _model;
    private readonly LayoutGridControlBinding _binding;
    private readonly LayoutGridChildren _children;
    private readonly LayoutGridDefinitions _definitions;
    private DockableControlTracker? _tracker;
    private LayoutSizingViews? _views;
    private Orientation _reportedOrientation;
    private event EventHandler? SplitterOrientationChanged;
    private bool _fixPending = true;
    private bool _definitionsPending = true;
    private bool _fixing;

    internal LayoutGridControl(LayoutPositionableGroup<T> model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
        DataContext = model.DockModel;
        DockProperties.SetIsDropArea(this, true);
        FlowDirection = FlowDirection.LeftToRight;
        _reportedOrientation = Orientation;
        _children = new(this, model);
        _definitions = new(this, (ILayoutOrientableGroup)model);
        _binding = new(model, RequestLayout, RequestDefinitions);
    }

    /// <summary>Gets the public container represented by this grid.</summary>
    public ILayoutElement Model => _model;
    /// <summary>Gets the current split orientation of the public model.</summary>
    public Orientation Orientation => ((ILayoutOrientableGroup)_model).Orientation;

    Panel IProportionalSplitterHost.Panel => this;
    event EventHandler? IProportionalSplitterHost.OrientationChanged
    {
        add => SplitterOrientationChanged += value;
        remove => SplitterOrientationChanged -= value;
    }
    void IProportionalSplitterHost.Resize(Control target, Control neighbor, double dragDelta, double minimumSize) =>
        LayoutSplitterResize.TryResize(target, neighbor, Orientation, dragDelta, minimumSize);

    /// <summary>Subscribes to the live model when this grid is initialized.</summary>
    protected virtual void OnInitialized(EventArgs e)
    {
        base.OnInitialized();
        _binding.Attach();
    }

    /// <inheritdoc/>
    protected override void OnInitialized() => OnInitialized(EventArgs.Empty);

    /// <summary>Invokes the container's length normalization policy without recursive entry.</summary>
    protected void FixChildrenDockLengths()
    {
        if (_fixing) return;
        _fixing = true;
        try { OnFixChildrenDockLengths(); }
        finally { _fixing = false; }
    }

    /// <summary>Normalizes child lengths before rebuilding a split layout.</summary>
    protected abstract void OnFixChildrenDockLengths();

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (_fixPending && _model.Root?.Manager is not null)
        {
            _fixPending = false;
            FixChildrenDockLengths();
            _children.Refresh();
            _definitionsPending = true;
        }
        if (_definitionsPending)
        {
            _definitionsPending = false;
            _definitions.Refresh();
        }
        return base.MeasureOverride(availableSize);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        Size result = base.ArrangeOverride(finalSize);
        _definitions.SynchronizeProportions(result);
        return result;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LayoutSplitterSizing.WidthProperty || change.Property == LayoutSplitterSizing.HeightProperty)
            RequestDefinitions();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _binding.Attach();
        _tracker?.Dispose();
        _tracker = new(this, TrackingMode.Visible);
        _views = _model.Root?.Manager?.SizingViews;
        _views?.Add(this);
        RequestLayout();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _binding.Detach();
        _tracker?.Dispose();
        _tracker = null;
        _views?.Remove(this);
        _views = null;
        base.OnDetachedFromVisualTree(e);
    }

    void ILayoutGridView.ReleaseChildren()
    {
        _children.Clear();
        RequestLayout();
    }

    private void RequestLayout()
    {
        _fixPending = true;
        if (_reportedOrientation != Orientation)
        {
            _reportedOrientation = Orientation;
            SplitterOrientationChanged?.Invoke(this, EventArgs.Empty);
        }
        InvalidateMeasure();
    }

    private void RequestDefinitions()
    {
        _definitionsPending = true;
        InvalidateMeasure();
    }
}
