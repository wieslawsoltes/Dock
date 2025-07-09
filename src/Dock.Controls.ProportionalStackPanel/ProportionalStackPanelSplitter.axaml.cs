// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using System.Windows.Input;
using Dock.Controls.ProportionalStackPanel.Internal;
using Avalonia.VisualTree;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Represents a control that lets the user change the size of elements in a <see cref="ProportionalStackPanel"/>.
/// </summary>
[PseudoClasses(":horizontal", ":vertical")]
public class ProportionalStackPanelSplitter : Thumb
{
    /// <summary>
    /// Defines the <see cref="Thickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<ProportionalStackPanelSplitter, double>(nameof(Thickness), 4.0);

    /// <summary>
    /// Defines the <see cref="IsResizingEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsResizingEnabledProperty =
        AvaloniaProperty.Register<ProportionalStackPanelSplitter, bool>(nameof(IsResizingEnabled), true);

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

    /// <summary>
    /// Gets or sets a value indicating whether resizing is enabled.
    /// </summary>
    public bool IsResizingEnabled
    {
        get => GetValue(IsResizingEnabledProperty);
        set => SetValue(IsResizingEnabledProperty, value);
    }

    private Point _startPoint;
    private bool _isMoving;

    /// <summary>
    /// Command that resets the proportions of adjacent elements.
    /// </summary>
    public ICommand ResetProportionCommand { get; }

    /// <summary>
    /// Command that sets the proportion of the element before the splitter.
    /// </summary>
    public ICommand SetProportionCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProportionalStackPanelSplitter"/> class.
    /// </summary>
    public ProportionalStackPanelSplitter()
    {
        ResetProportionCommand = Internal.Command.Create(ResetProportion);
        SetProportionCommand = Internal.Command.Create<double>(SetProportion);
    }


    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsResizingEnabledProperty)
        {
            UpdateVisualState();
        }
    }

    internal static bool IsSplitter(Control? control, out ProportionalStackPanelSplitter? proportionalStackPanelSplitter)
    {
        if (control is ContentPresenter contentPresenter)
        {
            if (contentPresenter.Child is null)
            {
                contentPresenter.UpdateChild();
            }

            if (contentPresenter.Child is ProportionalStackPanelSplitter childSplitter)
            {
                proportionalStackPanelSplitter = childSplitter;
                return true;
            }
        }

        if (control is ProportionalStackPanelSplitter splitter)
        {
            proportionalStackPanelSplitter = splitter;
            return true;
        }

        proportionalStackPanelSplitter = null;
        return false;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (IsResizingEnabled && GetPanel() is { } panel)
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

        if (_isMoving && IsResizingEnabled)
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

        if (ContextMenu is null)
        {
            if (Application.Current?.TryFindResource("ProportionalStackPanelSplitterContextMenu", out var resource) == true &&
                resource is ContextMenu menu)
            {
                ContextMenu = menu;
            }
        }

        var panel = GetPanel();
        if (panel is null)
        {
            return;
        }

        UpdateVisualState();
    }

    private Control? FindNextChild(ProportionalStackPanel panel)
    {
        return GetSiblingInDirection(panel, 1);
    }

    private void SetTargetProportion(double dragDelta)
    {
        var panel = GetPanel();
        if (panel == null)
        {
            return;
        }
        
        var target = GetTargetElement(panel);
        if (target is null)
        {
            return;
        }

        var child = FindNextChild(panel);

        var targetElementProportion = ProportionalStackPanel.GetProportion(target);
        var neighbourProportion = child is not null ? ProportionalStackPanel.GetProportion(child) : double.NaN;

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

        ProportionalStackPanel.SetProportion(target, targetElementProportion);

        if (child is not null)
        {
            ProportionalStackPanel.SetProportion(child, neighbourProportion);
        }
    }

    private void UpdateVisualState()
    {
        if (GetPanel() is { } panel)
        {
            if (panel.Orientation == Orientation.Vertical)
            {
                Height = Thickness;
                Width = double.NaN;
                Cursor = IsResizingEnabled
                    ? new Cursor(StandardCursorType.SizeNorthSouth)
                    : new Cursor(StandardCursorType.Arrow);
                PseudoClasses.Add(":vertical");
            }
            else
            {
                Width = Thickness;
                Height = double.NaN;
                Cursor = IsResizingEnabled
                    ? new Cursor(StandardCursorType.SizeWestEast)
                    : new Cursor(StandardCursorType.Arrow);
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

    private Control? GetSiblingInDirection(ProportionalStackPanel panel, int direction)
    {
        Debug.Assert(direction == -1 || direction == 1);
        
        var children = panel.Children;
        int siblingIndex;

        if (Parent is ContentPresenter parentContentPresenter)
        {
            siblingIndex = children.IndexOf(parentContentPresenter) + direction;
        }
        else
        {
            siblingIndex = children.IndexOf(this) + direction;
        }

        while (siblingIndex >= 0 && siblingIndex < children.Count && 
               (ProportionalStackPanel.GetIsCollapsed(children[siblingIndex]) || IsSplitter(children[siblingIndex], out _)))
        {
            siblingIndex += direction;
        }

        return siblingIndex >= 0 && siblingIndex < children.Count ? children[siblingIndex] : null;
    }
    
    private Control? GetTargetElement(ProportionalStackPanel panel)
    {
        return GetSiblingInDirection(panel, -1);
    }


    /// <summary>
    /// Resets the proportions of elements adjacent to the splitter.
    /// </summary>
    public void ResetProportion()
    {
        var panel = GetPanel();
        if (panel is null)
        {
            return;
        }

        var prev = GetTargetElement(panel);
        var next = FindNextChild(panel);

        if (prev is not null)
        {
            ProportionalStackPanel.SetProportion(prev, double.NaN);
        }

        if (next is not null)
        {
            ProportionalStackPanel.SetProportion(next, double.NaN);
        }
    }

    /// <summary>
    /// Sets the proportion of the element before the splitter.
    /// </summary>
    /// <param name="proportion">The new proportion value.</param>
    public void SetProportion(double proportion)
    {
        var panel = GetPanel();
        if (panel is null)
        {
            return;
        }

        var target = GetTargetElement(panel);
        var neighbour = FindNextChild(panel);
        if (target is null || neighbour is null)
        {
            return;
        }

        var delta = proportion - ProportionalStackPanel.GetProportion(target);
        var neighbourProportion = ProportionalStackPanel.GetProportion(neighbour);

        ProportionalStackPanel.SetProportion(target, proportion);
        ProportionalStackPanel.SetProportion(neighbour, neighbourProportion - delta);
    }


}
