using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Base grid for a public split container rendered by Dock's proportional layout.</summary>
/// <typeparam name="T">The public child type accepted by the container.</typeparam>
public abstract class LayoutGridControl<T> : Grid, ILayoutControl, ILayoutGridView where T : class, ILayoutPanelElement
{
    private readonly LayoutPositionableGroup<T> _model;
    private readonly LayoutGridControlBinding _binding;
    private Control? _content;
    private bool _fixPending = true;
    private bool _fixing;

    internal LayoutGridControl(LayoutPositionableGroup<T> model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
        FlowDirection = FlowDirection.LeftToRight;
        _binding = new(model, RequestLayout);
    }

    /// <summary>Gets the public container represented by this grid.</summary>
    public ILayoutElement Model => _model;
    /// <summary>Gets the current split orientation of the public model.</summary>
    public Orientation Orientation => ((ILayoutOrientableGroup)_model).Orientation;

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
        }
        return base.MeasureOverride(availableSize);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _binding.Attach();
        RequestLayout();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _binding.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    void ILayoutGridView.SetContent(Control? content)
    {
        if (ReferenceEquals(_content, content)) return;
        _content = content;
        Children.Clear();
        if (content is not null) Children.Add(content);
    }

    private void RequestLayout()
    {
        _fixPending = true;
        InvalidateMeasure();
    }
}
