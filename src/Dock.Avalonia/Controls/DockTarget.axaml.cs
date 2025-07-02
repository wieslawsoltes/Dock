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
    private Panel? _globalTopIndicator;
    private Panel? _globalBottomIndicator;
    private Panel? _globalLeftIndicator;
    private Panel? _globalRightIndicator;
    private Control? _topSelector;
    private Control? _bottomSelector;
    private Control? _leftSelector;
    private Control? _rightSelector;
    private Control? _centerSelector;
    private Control? _globalTopSelector;
    private Control? _globalBottomSelector;
    private Control? _globalLeftSelector;
    private Control? _globalRightSelector;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _topIndicator = e.NameScope.Find<Panel>("PART_TopIndicator");
        _bottomIndicator = e.NameScope.Find<Panel>("PART_BottomIndicator");
        _leftIndicator = e.NameScope.Find<Panel>("PART_LeftIndicator");
        _rightIndicator = e.NameScope.Find<Panel>("PART_RightIndicator");
        _centerIndicator = e.NameScope.Find<Panel>("PART_CenterIndicator");
        _globalTopIndicator = e.NameScope.Find<Panel>("PART_GlobalTopIndicator");
        _globalBottomIndicator = e.NameScope.Find<Panel>("PART_GlobalBottomIndicator");
        _globalLeftIndicator = e.NameScope.Find<Panel>("PART_GlobalLeftIndicator");
        _globalRightIndicator = e.NameScope.Find<Panel>("PART_GlobalRightIndicator");

        _topSelector = e.NameScope.Find<Control>("PART_TopSelector");
        _bottomSelector = e.NameScope.Find<Control>("PART_BottomSelector");
        _leftSelector = e.NameScope.Find<Control>("PART_LeftSelector");
        _rightSelector = e.NameScope.Find<Control>("PART_RightSelector");
        _centerSelector = e.NameScope.Find<Control>("PART_CenterSelector");
        _globalTopSelector = e.NameScope.Find<Control>("PART_GlobalTopSelector");
        _globalBottomSelector = e.NameScope.Find<Control>("PART_GlobalBottomSelector");
        _globalLeftSelector = e.NameScope.Find<Control>("PART_GlobalLeftSelector");
        _globalRightSelector = e.NameScope.Find<Control>("PART_GlobalRightSelector");
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

        if (InvalidateIndicator(_globalLeftSelector, _globalLeftIndicator, point, relativeTo, DockOperation.GlobalLeft, dragAction, validate))
        {
            result = DockOperation.GlobalLeft;
        }

        if (InvalidateIndicator(_globalRightSelector, _globalRightIndicator, point, relativeTo, DockOperation.GlobalRight, dragAction, validate))
        {
            result = DockOperation.GlobalRight;
        }

        if (InvalidateIndicator(_globalTopSelector, _globalTopIndicator, point, relativeTo, DockOperation.GlobalTop, dragAction, validate))
        {
            result = DockOperation.GlobalTop;
        }

        if (InvalidateIndicator(_globalBottomSelector, _globalBottomIndicator, point, relativeTo, DockOperation.GlobalBottom, dragAction, validate))
        {
            result = DockOperation.GlobalBottom;
        }

        if (InvalidateIndicator(_centerSelector, _centerIndicator, point, relativeTo, DockOperation.Fill, dragAction, validate))
        {
            result = DockOperation.Fill;
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
