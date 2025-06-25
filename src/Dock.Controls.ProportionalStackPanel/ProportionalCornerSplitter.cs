using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Avalonia.Controls;

/// <summary>
/// Represents a splitter placed at the intersection of two <see cref="ProportionalStackPanelSplitter"/> controls.
/// Dragging the corner splitter resizes both orientations at the same time.
/// </summary>
public class ProportionalCornerSplitter : Thumb
{
    /// <summary>
    /// Gets or sets the horizontal splitter that will be adjusted while dragging.
    /// </summary>
    public ProportionalStackPanelSplitter? HorizontalSplitter { get; set; }

    /// <summary>
    /// Gets or sets the vertical splitter that will be adjusted while dragging.
    /// </summary>
    public ProportionalStackPanelSplitter? VerticalSplitter { get; set; }

    private Point _startPoint;
    private bool _isMoving;

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _startPoint = e.GetPosition(this);
        _isMoving = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isMoving = false;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_isMoving)
        {
            return;
        }

        var point = e.GetPosition(this);
        var delta = point - _startPoint;
        _startPoint = point;

        HorizontalSplitter?.AdjustTargetProportion(delta.X);
        VerticalSplitter?.AdjustTargetProportion(delta.Y);
    }
}
