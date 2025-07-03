// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DockTarget"/> xaml.
/// </summary>
public class DockTarget : TemplatedControl
{
    private Panel? _topIndicator;
    private Panel? _bottomIndicator;
    private Panel? _leftIndicator;
    private Panel? _rightIndicator;
    private Panel? _centerIndicator;
    private Panel? _rootTopIndicator;
    private Panel? _rootBottomIndicator;
    private Panel? _rootLeftIndicator;
    private Panel? _rootRightIndicator;
    private Control? _topSelector;
    private Control? _bottomSelector;
    private Control? _leftSelector;
    private Control? _rightSelector;
    private Control? _centerSelector;
    private Control? _rootTopSelector;
    private Control? _rootBottomSelector;
    private Control? _rootLeftSelector;
    private Control? _rootRightSelector;
    private Grid? _indicatorGrid;

    public Rect RootBounds { get; set; }

    public Rect PlacementBounds { get; set; }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _topIndicator = e.NameScope.Find<Panel>("PART_TopIndicator");
        _bottomIndicator = e.NameScope.Find<Panel>("PART_BottomIndicator");
        _leftIndicator = e.NameScope.Find<Panel>("PART_LeftIndicator");
        _rightIndicator = e.NameScope.Find<Panel>("PART_RightIndicator");
        _centerIndicator = e.NameScope.Find<Panel>("PART_CenterIndicator");
        _rootTopIndicator = e.NameScope.Find<Panel>("PART_RootTopIndicator");
        _rootBottomIndicator = e.NameScope.Find<Panel>("PART_RootBottomIndicator");
        _rootLeftIndicator = e.NameScope.Find<Panel>("PART_RootLeftIndicator");
        _rootRightIndicator = e.NameScope.Find<Panel>("PART_RootRightIndicator");

        _topSelector = e.NameScope.Find<Control>("PART_TopSelector");
        _bottomSelector = e.NameScope.Find<Control>("PART_BottomSelector");
        _leftSelector = e.NameScope.Find<Control>("PART_LeftSelector");
        _rightSelector = e.NameScope.Find<Control>("PART_RightSelector");
        _centerSelector = e.NameScope.Find<Control>("PART_CenterSelector");
        _rootTopSelector = e.NameScope.Find<Control>("PART_RootTopSelector");
        _rootBottomSelector = e.NameScope.Find<Control>("PART_RootBottomSelector");
        _rootLeftSelector = e.NameScope.Find<Control>("PART_RootLeftSelector");
        _rootRightSelector = e.NameScope.Find<Control>("PART_RootRightSelector");
        _indicatorGrid = e.NameScope.Find<Grid>("PART_IndicatorGrid");
        UpdatePlacement();
    }

    internal void UpdatePlacement()
    {
        if (_indicatorGrid is { })
        {
            var left = PlacementBounds.X;
            var top = PlacementBounds.Y;
            var right = RootBounds.Width - PlacementBounds.Right;
            var bottom = RootBounds.Height - PlacementBounds.Bottom;
            _indicatorGrid.Margin = new Thickness(left, top, right, bottom);
        }
    }

    internal DockOperation GetDockOperation(Point point, Visual relativeTo, DragAction dragAction, Func<Point, DockOperation, DragAction, Visual, bool> validate)
    {
        var result = DockOperation.Window;

        if (InvalidateIndicator(_leftSelector, _leftIndicator, point, relativeTo, DockOperation.Left, dragAction, validate))
        {
            result = DockOperation.Left;
        }

        if (InvalidateIndicator(_rightSelector, _rightIndicator, point, relativeTo, DockOperation.Right, dragAction, validate))
        {
            result = DockOperation.Right;
        }

        if (InvalidateIndicator(_topSelector, _topIndicator, point, relativeTo, DockOperation.Top, dragAction, validate))
        {
            result = DockOperation.Top;
        }

        if (InvalidateIndicator(_bottomSelector, _bottomIndicator, point, relativeTo, DockOperation.Bottom, dragAction, validate))
        {
            result = DockOperation.Bottom;
        }

        if (InvalidateIndicator(_centerSelector, _centerIndicator, point, relativeTo, DockOperation.Fill, dragAction, validate))
        {
            result = DockOperation.Fill;
        }

        if (InvalidateIndicator(_rootLeftSelector, _rootLeftIndicator, point, relativeTo, DockOperation.RootLeft, dragAction, validate))
        {
            result = DockOperation.RootLeft;
        }

        if (InvalidateIndicator(_rootRightSelector, _rootRightIndicator, point, relativeTo, DockOperation.RootRight, dragAction, validate))
        {
            result = DockOperation.RootRight;
        }

        if (InvalidateIndicator(_rootTopSelector, _rootTopIndicator, point, relativeTo, DockOperation.RootTop, dragAction, validate))
        {
            result = DockOperation.RootTop;
        }

        if (InvalidateIndicator(_rootBottomSelector, _rootBottomIndicator, point, relativeTo, DockOperation.RootBottom, dragAction, validate))
        {
            result = DockOperation.RootBottom;
        }

        return result;
    }

    private bool InvalidateIndicator(Control? selector, Panel? indicator, Point point, Visual relativeTo, DockOperation operation, DragAction dragAction, Func<Point, DockOperation, DragAction, Visual, bool> validate)
    {
        if (selector is null || indicator is null)
        {
            return false;
        }

        var selectorPoint = relativeTo.TranslatePoint(point, selector);
        if (selectorPoint is { })
        {
            if (selector.InputHitTest(selectorPoint.Value) is { } inputElement && Equals(inputElement, selector))
            {
                if (validate(point, operation, dragAction, relativeTo))
                {
                    indicator.Opacity = 0.5;
                    return true;
                }
            }
        }

        indicator.Opacity = 0;
        return false;
    }
}
