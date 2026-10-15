using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvalonDock.Infrastructure;

namespace AvalonDock.Controls;

/// <summary>Arranges document headers at their natural widths and suppresses overflowing headers.</summary>
public class DocumentPaneTabPanel : Panel
{
    private readonly LayoutTabOverflow _overflow;
    private int _layoutVersion;

    /// <summary>Creates a panel whose tab order runs from left to right.</summary>
    public DocumentPaneTabPanel()
    {
        FlowDirection = FlowDirection.LeftToRight;
        _overflow = new(this);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        _overflow.Refresh();
        double width = 0;
        double height = 0;
        int version = _layoutVersion;
        for (int i = 0; i < Children.Count; i++)
        {
            Control child = Children[i];
            child.Measure(Size.Infinity);
            if (version != _layoutVersion) { InvalidateMeasure(); break; }
            width += child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
        }
        return new Size(Math.Min(width, availableSize.Width), height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        _overflow.Refresh();
        if (_overflow.BringSelectedTabForward()) return finalSize;
        double x = 0;
        bool overflow = false;
        int version = _layoutVersion;
        for (int i = 0; i < Children.Count; i++)
        {
            Control child = Children[i];
            if (!child.IsVisible)
            {
                _overflow.SetHidden(child, false);
                if (version != _layoutVersion) { InvalidateMeasure(); break; }
                continue;
            }
            overflow |= x + child.DesiredSize.Width > finalSize.Width;
            _overflow.SetHidden(child, overflow);
            if (version != _layoutVersion) { InvalidateMeasure(); break; }
            // Hidden headers remain measured, like WPF Hidden rather than Collapsed.
            // Keeping a normal slot also permits immediate restoration on expansion.
            child.Arrange(new Rect(x, 0, child.DesiredSize.Width, finalSize.Height));
            if (version != _layoutVersion) { InvalidateMeasure(); break; }
            if (!overflow) x += child.Bounds.Width + child.Margin.Left + child.Margin.Right;
        }
        return finalSize;
    }

    /// <summary>Preserves normal pointer-exit routing for derived document tab panels.</summary>
    protected virtual void OnMouseLeave(PointerEventArgs e) => base.OnPointerExited(e);

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e) => OnMouseLeave(e);

    /// <inheritdoc/>
    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _layoutVersion++;
        base.ChildrenChanged(sender, e);
        _overflow?.ChildrenChanged();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _layoutVersion++;
        _overflow.Clear();
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _overflow.Refresh();
        InvalidateMeasure();
    }
}
