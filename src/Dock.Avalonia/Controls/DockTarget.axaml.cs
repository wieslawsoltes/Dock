// Copyright (c) Wiesław Šoltés. All rights reserved.
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
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DockTarget"/> xaml.
/// </summary>
[TemplatePart("PART_TopIndicator", typeof(Panel))]
[TemplatePart("PART_BottomIndicator", typeof(Panel))]
[TemplatePart("PART_LeftIndicator", typeof(Panel))]
[TemplatePart("PART_RightIndicator", typeof(Panel))]
[TemplatePart("PART_CenterIndicator", typeof(Panel))]
[TemplatePart("PART_TopSelector", typeof(Control))]
[TemplatePart("PART_BottomSelector", typeof(Control))]
[TemplatePart("PART_LeftSelector", typeof(Control))]
[TemplatePart("PART_RightSelector", typeof(Control))]
[TemplatePart("PART_CenterSelector", typeof(Control))]
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

    /// <summary>
    /// Gets or sets whether only drop indicators should be shown.
    /// </summary>
    public static readonly StyledProperty<bool> ShowIndicatorsOnlyProperty =
        AvaloniaProperty.Register<DockTarget, bool>(nameof(ShowIndicatorsOnly));

    /// <summary>
    /// Gets or sets whether only drop indicators should be shown.
    /// </summary>
    public bool ShowIndicatorsOnly
    {
        get => GetValue(ShowIndicatorsOnlyProperty);
        set => SetValue(ShowIndicatorsOnlyProperty, value);
    }

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

    internal DockOperation GetDockOperation(
        Point point, 
        Control dropControl, 
        Visual relativeTo,
        DragAction dragAction,
        DockOperationHandler validate,
        DockOperationHandler? visible = null)
    {
        if (ShowIndicatorsOnly)
        {
            var operation = DockProperties.GetIndicatorDockOperation(dropControl);
            var indicator = operation switch
            {
                DockOperation.Left => _leftIndicator,
                DockOperation.Right => _rightIndicator,
                DockOperation.Top => _topIndicator,
                DockOperation.Bottom => _bottomIndicator,
                DockOperation.Fill => _centerIndicator,
                _ => null
            };

            if (_leftIndicator is not null && indicator != _leftIndicator)
            {
                _leftIndicator.Opacity = 0;
            }

            if (_rightIndicator is not null && indicator != _rightIndicator)
            {
                _rightIndicator.Opacity = 0;
            }

            if (_topIndicator is not null && indicator != _topIndicator)
            {
                _topIndicator.Opacity = 0;
            }

            if (_bottomIndicator is not null && indicator != _bottomIndicator)
            {
                _bottomIndicator.Opacity = 0;
            }

            if (_centerIndicator is not null && indicator != _centerIndicator)
            {
                _centerIndicator.Opacity = 0;
            }

            return InvalidateIndicator(dropControl, indicator, point, relativeTo, operation, dragAction, validate, visible)
                ? operation
                : DockOperation.Window;
        }

        var result = DockOperation.Window;

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

        if (InvalidateIndicator(_centerSelector, _centerIndicator, point, relativeTo, DockOperation.Fill, dragAction, validate, visible))
        {
            result = DockOperation.Fill;
        }

        return result;
    }

    private bool IsDockTargetSelector(Control selector)
    {
        return selector == _topSelector ||
               selector == _bottomSelector ||
               selector == _leftSelector ||
               selector == _rightSelector ||
               selector == _centerSelector;
    }

    private bool InvalidateIndicator(
        Control? selector, 
        Panel? indicator, 
        Point point, 
        Visual relativeTo,
        DockOperation operation, 
        DragAction dragAction,
        DockOperationHandler validate,
        DockOperationHandler? visible)
    {
        if (selector is null || indicator is null)
        {
            return false;
        }

        var isDockTargetSelector = IsDockTargetSelector(selector);
 
        if (visible is not null && !visible(point, operation, dragAction, relativeTo))
        {
            indicator.Opacity = 0;

            if (isDockTargetSelector)
            {
                selector.Opacity = 0;
            }

            return false;
        }

        if (isDockTargetSelector)
        {
            selector.Opacity = 1;
        }

        var selectorPoint = relativeTo.TranslatePoint(point, selector);
        if (selectorPoint is null)
        {
            var screenPoint = relativeTo.PointToScreen(point);
            var localPoint = this.PointToClient(screenPoint);
            selectorPoint = this.TranslatePoint(localPoint, selector);
        }

        if (selectorPoint is not null)
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
