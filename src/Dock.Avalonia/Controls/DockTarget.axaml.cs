/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
    private Control? _topSelector;
    private Control? _bottomSelector;
    private Control? _leftSelector;
    private Control? _rightSelector;
    private Control? _centerSelector;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _topIndicator = e.NameScope.Find<Panel>("PART_TopIndicator");
        _bottomIndicator = e.NameScope.Find<Panel>("PART_BottomIndicator");
        _leftIndicator = e.NameScope.Find<Panel>("PART_LeftIndicator");
        _rightIndicator = e.NameScope.Find<Panel>("PART_RightIndicator");
        _centerIndicator = e.NameScope.Find<Panel>("PART_CenterIndicator");

        _topSelector = e.NameScope.Find<Control>("PART_TopSelector");
        _bottomSelector = e.NameScope.Find<Control>("PART_BottomSelector");
        _leftSelector = e.NameScope.Find<Control>("PART_LeftSelector");
        _rightSelector = e.NameScope.Find<Control>("PART_RightSelector");
        _centerSelector = e.NameScope.Find<Control>("PART_CenterSelector");
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
