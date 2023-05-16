using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Represents a control that lets the user change the size of elements in a <see cref="ProportionalStackPanel"/>.
/// </summary>
[PseudoClasses(":horizontal", ":vertical")]
public class ProportionalStackPanelSplitter : Thumb
{
    /// <summary>
    /// Defines the Proportion attached property.
    /// </summary>
    public static readonly AttachedProperty<double> ProportionProperty =
        AvaloniaProperty.RegisterAttached<ProportionalStackPanelSplitter, Control, double>("Proportion", double.NaN, false, BindingMode.TwoWay);

    /// <summary>
    /// Gets the value of the Proportion attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The Proportion attached property.</returns>
    public static double GetProportion(AvaloniaObject control)
    {
        return control.GetValue(ProportionProperty);
    }

    /// <summary>
    /// Sets the value of the Proportion attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the Proportion property.</param>
    public static void SetProportion(AvaloniaObject control, double value)
    {
        control.SetValue(ProportionProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Thickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<ProportionalStackPanelSplitter, double>(nameof(Thickness), 4.0);

    /// <summary>
    /// Defines the MinimumProportionSize attached property.
    /// </summary>
    public static readonly AttachedProperty<double> MinimumProportionSizeProperty =
        AvaloniaProperty.RegisterAttached<ProportionalStackPanelSplitter, Control, double>("MinimumProportionSize", 75, true);

    /// <summary>
    /// Gets the value of the MinimumProportion attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The minimum size a proportion can be resized to.</returns>
    public static double GetMinimumProportionSize(AvaloniaObject control)
    {
        return control.GetValue(MinimumProportionSizeProperty);
    }

    /// <summary>
    /// Sets the value of the MinimumProportionSize attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The minimum size a proportion can be resized to.</param>
    public static void SetMinimumProportionSize(AvaloniaObject control, double value)
    {
        control.SetValue(MinimumProportionSizeProperty, value);
    }
        
    /// <summary>
    /// Gets or sets the thickness (height or width, depending on orientation).
    /// </summary>
    /// <value>The thickness.</value>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    private Point _startPoint;
    private bool _isMoving;

    internal static bool IsSplitter(Control? control)
    {
        if (control is ContentPresenter contentPresenter)
        {
            if (contentPresenter.Child is null)
            {
                contentPresenter.UpdateChild();
            }

            return contentPresenter.Child is ProportionalStackPanelSplitter;
        }

        return control is ProportionalStackPanelSplitter;
    }

    internal static void SetControlProportion(Control? control, double proportion)
    {
        if (control is ContentPresenter contentPresenter)
        {
            if (contentPresenter.Child is null)
            {
                contentPresenter.UpdateChild();
            }

            if (contentPresenter.Child is not null)
            {
                SetProportion(contentPresenter.Child, proportion);
            }
        }
        else
        {
            if (control is not null)
            {
                SetProportion(control, proportion);
            }
        }
    }

    internal static double GetControlProportion(Control? control)
    {
        if (control is ContentPresenter contentPresenter)
        {
            if (contentPresenter.Child is null)
            {
                contentPresenter.UpdateChild();
            }

            if (contentPresenter.Child is not null)
            {
                return GetProportion(contentPresenter.Child);
            }

            return double.NaN;
        }

        if (control is not null)
        {
            return GetProportion(control);
        }

        return double.NaN;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (GetPanel() is { } panel)
        {
            var point = e.GetPosition(panel);
            _startPoint = point;
            _isMoving = true;
        }
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

        if (_isMoving)
        {  
            if (GetPanel() is { } panel)
            {
                var point = e.GetPosition(panel);
                var delta = point - _startPoint;
                _startPoint = point;
                SetTargetProportion(panel.Orientation == Orientation.Vertical ? delta.Y : delta.X);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        _isMoving = false;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (GetPanel() is { } panel)
        {
            if (panel.Orientation == Orientation.Vertical)
            {
                return new Size(0, Thickness);
            }
            else
            {
                return new Size(Thickness, 0);
            }
        }

        return new Size();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var panel = GetPanel();
        if (panel is null)
        {
            return;
        }

        UpdateHeightOrWidth();
    }

    private Control? FindNextChild(ProportionalStackPanel panel)
    {
        var children = panel.Children;
        int nextIndex;

        if (Parent is ContentPresenter parentContentPresenter)
        {
            nextIndex = children.IndexOf(parentContentPresenter) + 1;
        }
        else
        {
            nextIndex = children.IndexOf(this) + 1;
        }

        var child = children[nextIndex];
        if (child is ContentPresenter contentPresenter)
        {
            if (contentPresenter.Child is null)
            {
                contentPresenter.UpdateChild();
            }

            return contentPresenter.Child;
        }

        return child;
    }

    private void SetTargetProportion(double dragDelta)
    {
        var target = GetTargetElement();
        var panel = GetPanel();
        if (target is null || panel is null)
        {
            return;
        }

        var child = FindNextChild(panel);

        var targetElementProportion = GetControlProportion(target);
        var neighbourProportion = child is not null ? GetControlProportion(child) : double.NaN;

        var dProportion = dragDelta / (panel.Orientation == Orientation.Vertical ? panel.Bounds.Height : panel.Bounds.Width);

        if (targetElementProportion + dProportion < 0)
        {
            dProportion = -targetElementProportion;
        }

        if (neighbourProportion - dProportion < 0)
        {
            dProportion = +neighbourProportion;
        }

        targetElementProportion += dProportion;
        neighbourProportion -= dProportion;

        var minProportion = GetValue(MinimumProportionSizeProperty) / (panel.Orientation == Orientation.Vertical ? panel.Bounds.Height : panel.Bounds.Width);

        if (targetElementProportion < minProportion)
        {
            dProportion = targetElementProportion - minProportion;
            neighbourProportion += dProportion;
            targetElementProportion -= dProportion;

        }
        else if (neighbourProportion < minProportion)
        {
            dProportion = neighbourProportion - minProportion;
            neighbourProportion -= dProportion;
            targetElementProportion += dProportion;
        }

        SetProportion(target, targetElementProportion);

        if (child is not null)
        {
            SetProportion(child, neighbourProportion);
        }

        panel.InvalidateMeasure();
        panel.InvalidateArrange();
    }

    private void UpdateHeightOrWidth()
    {
        if (GetPanel() is { } panel)
        {
            if (panel.Orientation == Orientation.Vertical)
            {
                Height = Thickness;
                Width = double.NaN;
                Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                PseudoClasses.Add(":vertical");
            }
            else
            {
                Width = Thickness;
                Height = double.NaN;
                Cursor = new Cursor(StandardCursorType.SizeWestEast);
                PseudoClasses.Add(":horizontal");
            }
        }
    }

    private ProportionalStackPanel? GetPanel()
    {
        if (Parent is ContentPresenter presenter)
        {
            if (presenter.GetVisualParent() is ProportionalStackPanel panel)
            {
                return panel;
            }
        }
        else if (this.GetVisualParent() is ProportionalStackPanel psp)
        {
            return psp;
        }

        return null;
    }

    private Control? GetTargetElement()
    {
        if (Parent is ContentPresenter presenter)
        {
            if (presenter.GetVisualParent() is not Panel panel)
            {
                return null;
            }

            var parent = Parent as Control;
            var index = parent is null ? -1 :panel.Children.IndexOf(parent);
            if (index > 0 && panel.Children.Count > 0)
            {
                if (panel.Children[index - 1] is ContentPresenter contentPresenter)
                {
                    return contentPresenter.Child;
                }
                else
                {
                    return null;
                }
            }
        }
        else
        {
            var panel = GetPanel();
            if (panel is not null)
            {
                var index = panel.Children.IndexOf(this);
                if (index > 0 && panel.Children.Count > 0)
                {
                    return panel.Children[index - 1];
                }
            }
        }

        return null;
    }
}
