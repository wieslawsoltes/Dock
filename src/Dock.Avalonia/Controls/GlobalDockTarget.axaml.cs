﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Model.Core;
using Dock.Avalonia.Contract;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="GlobalDockTarget"/> xaml.
/// </summary>
[TemplatePart("PART_TopIndicator", typeof(Panel))]
[TemplatePart("PART_BottomIndicator", typeof(Panel))]
[TemplatePart("PART_LeftIndicator", typeof(Panel))]
[TemplatePart("PART_RightIndicator", typeof(Panel))]
[TemplatePart("PART_TopSelector", typeof(Control))]
[TemplatePart("PART_BottomSelector", typeof(Control))]
[TemplatePart("PART_LeftSelector", typeof(Control))]
[TemplatePart("PART_RightSelector", typeof(Control))]
public class GlobalDockTarget : TemplatedControl
{
    private Panel? _topIndicator;
    private Panel? _bottomIndicator;
    private Panel? _leftIndicator;
    private Panel? _rightIndicator;
    private Control? _topSelector;
    private Control? _bottomSelector;
    private Control? _leftSelector;
    private Control? _rightSelector;

    /// <summary>
    /// Gets or sets whether only drop selectors should be shown.
    /// </summary>
    public static readonly StyledProperty<bool> ShowSelectorsOnlyProperty =
        AvaloniaProperty.Register<GlobalDockTarget, bool>(nameof(ShowSelectorsOnly));

    /// <summary>
    /// Gets or sets whether only drop selectors should be shown.
    /// </summary>
    public bool ShowSelectorsOnly
    {
        get => GetValue(ShowSelectorsOnlyProperty);
        set => SetValue(ShowSelectorsOnlyProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _topIndicator = e.NameScope.Find<Panel>("PART_TopIndicator");
        _bottomIndicator = e.NameScope.Find<Panel>("PART_BottomIndicator");
        _leftIndicator = e.NameScope.Find<Panel>("PART_LeftIndicator");
        _rightIndicator = e.NameScope.Find<Panel>("PART_RightIndicator");

        _topSelector = e.NameScope.Find<Control>("PART_TopSelector");
        _bottomSelector = e.NameScope.Find<Control>("PART_BottomSelector");
        _leftSelector = e.NameScope.Find<Control>("PART_LeftSelector");
        _rightSelector = e.NameScope.Find<Control>("PART_RightSelector");
    }

    internal DockOperation GetDockOperation(Point point, Visual relativeTo, DragAction dragAction,
        DockOperationHandler validate,
        DockOperationHandler? visible = null)
    {
        var result = DockOperation.None;

        if (InvalidateIndicator(_leftSelector, _leftIndicator, point, relativeTo, DockOperation.Left, dragAction, validate, visible))
        {
            result = DockOperation.Left;
        }

        if (InvalidateIndicator(_rightSelector, _rightIndicator, point, relativeTo, DockOperation.Right, dragAction, validate, visible))
        {
            result = DockOperation.Right;
        }

        if (InvalidateIndicator(_topSelector, _topIndicator, point, relativeTo, DockOperation.Top, dragAction, validate, visible))
        {
            result = DockOperation.Top;
        }

        if (InvalidateIndicator(_bottomSelector, _bottomIndicator, point, relativeTo, DockOperation.Bottom, dragAction, validate, visible))
        {
            result = DockOperation.Bottom;
        }

        return result;
    }

    private bool InvalidateIndicator(Control? selector, Panel? indicator, Point point, Visual relativeTo,
        DockOperation operation, DragAction dragAction,
        DockOperationHandler validate,
        DockOperationHandler? visible)
    {
        if (selector is null || indicator is null)
        {
            return false;
        }

        if (visible is { } && !visible(point, operation, dragAction, relativeTo))
        {
            indicator.Opacity = 0;
            selector.Opacity = 0;
            return false;
        }

        selector.Opacity = 1;

        var selectorPoint = relativeTo.TranslatePoint(point, selector);
        if (selectorPoint is null)
        {
            var screenPoint = relativeTo.PointToScreen(point);
            var localPoint = this.PointToClient(screenPoint);
            selectorPoint = this.TranslatePoint(localPoint, selector);
        }

        if (selectorPoint is { })
        {
            if (selector.InputHitTest(selectorPoint.Value) is { } inputElement && Equals(inputElement, selector))
            {
                if (validate(point, operation, dragAction, relativeTo))
                {
                    if (ShowSelectorsOnly)
                    {
                        selector.Opacity = 0.5;
                    }
                    else
                    {
                        indicator.Opacity = 0.5;
                    }
                    return true;
                }
            }
        }

        if (!ShowSelectorsOnly)
        {
            indicator.Opacity = 0;
        }
        else
        {
            selector.Opacity = 1;
        }
        return false;
    }
}
