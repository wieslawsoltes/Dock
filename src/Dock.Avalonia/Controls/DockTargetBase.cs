// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Dock.Avalonia.Contract;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Base class for dock targets that provide drop indicators and selectors for docking operations.
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
public abstract class DockTargetBase : TemplatedControl, IDockTarget
{
    private static readonly string[] s_indicators =
    [
        "PART_TopIndicator",
        "PART_BottomIndicator",
        "PART_LeftIndicator",
        "PART_RightIndicator",
        "PART_CenterIndicator"
    ];

    private static readonly string[] s_selectors =
    [
        "PART_TopSelector",
        "PART_BottomSelector",
        "PART_LeftSelector",
        "PART_RightSelector",
        "PART_CenterSelector"
    ];

    /// <summary>
    /// Gets or sets whether only drop indicators should be shown.
    /// </summary>
    public static readonly StyledProperty<bool> ShowIndicatorsOnlyProperty =
        AvaloniaProperty.Register<DockTargetBase, bool>(nameof(ShowIndicatorsOnly));
    
    /// <summary>
    /// Defines the <see cref="ShowHorizontalTargets"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowHorizontalTargetsProperty = AvaloniaProperty.Register<DockTargetBase, bool>(
        nameof(ShowHorizontalTargets), defaultValue: true);


    /// <summary>
    /// Defines the <see cref="ShowVerticalTargets"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowVerticalTargetsProperty = AvaloniaProperty.Register<DockTargetBase, bool>(
        nameof(ShowVerticalTargets), defaultValue: true);

    public DockTargetBase()
    {
        PseudoClasses.Set(":horizontal", this.GetObservable(ShowHorizontalTargetsProperty));
        PseudoClasses.Set(":vertical", this.GetObservable(ShowVerticalTargetsProperty));
    }

    /// <summary>
    /// Gets or sets whether only drop indicators should be shown.
    /// </summary>
    public bool ShowIndicatorsOnly
    {
        get => GetValue(ShowIndicatorsOnlyProperty);
        set => SetValue(ShowIndicatorsOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether horizontal docking targets should be displayed.
    /// </summary>
    public bool ShowHorizontalTargets
    {
        get => GetValue(ShowHorizontalTargetsProperty);
        set => SetValue(ShowHorizontalTargetsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether vertical docking targets should be displayed.
    /// </summary>s
    public bool ShowVerticalTargets
    {
        get => GetValue(ShowVerticalTargetsProperty);
        set => SetValue(ShowVerticalTargetsProperty, value);
    }

    /// <summary>
    /// A dictionary that maps dock operations to their corresponding indicator controls.
    /// </summary>
    protected Dictionary<DockOperation, Control> IndicatorOperations { get; set; } = new();

    /// <summary>
    /// A dictionary that maps dock operations to their corresponding selector controls.
    /// </summary>
    protected Dictionary<DockOperation, Control> SelectorsOperations { get; set; } = new();

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        foreach (var indicator in s_indicators)
        {
            AddIndicator(indicator, e.NameScope);
        }

        foreach (var selector in s_selectors)
        {
            AddSelector(selector, e.NameScope);
        }
    }

    /// <summary>
    /// Adds an indicator to the dock target based on the provided name and name scope.
    /// </summary>
    /// <param name="name">Template part name of the indicator.</param>
    /// <param name="nameScope">Scope to look up the template part.</param>
    protected void AddIndicator(string name, INameScope nameScope)
    {
        var indicator = nameScope.Find<Control>(name);
        if (indicator == null)
        {
            return;
        }

        var operation = DockProperties.GetIndicatorDockOperation(indicator);

        IndicatorOperations[operation] = indicator;
    }

    /// <summary>
    /// Adds a selector to the dock target based on the provided name and name scope.
    /// </summary>
    /// <param name="name">Template part name of the selector.</param>
    /// <param name="nameScope">Scope to look up the template part.</param>
    protected void AddSelector(string name, INameScope nameScope)
    {
        var selector = nameScope.Find<Control>(name);
        if (selector == null)
        {
            return;
        }

        var operation = DockProperties.GetIndicatorDockOperation(selector);

        SelectorsOperations[operation] = selector;
    }

    /// <summary>
    /// Gets the default dock operation for this dock target.
    /// </summary>
    protected virtual DockOperation DefaultDockOperation => DockOperation.Window;

    /// <summary>
    /// Gets the dock operation based on the provided point, drop control, relative visual, drag action,
    /// </summary>
    /// <param name="point">The current pointer position.</param>
    /// <param name="dropControl">Control that initiated the drop.</param>
    /// <param name="relativeTo">Visual relative to which the position is calculated.</param>
    /// <param name="dragAction">Current drag action type.</param>
    /// <param name="validate">Callback validating the operation.</param>
    /// <param name="visible">Callback checking indicator visibility.</param>
    /// <returns>The resulting dock operation.</returns>
    public DockOperation GetDockOperation(
        Point point,
        Control dropControl,
        Visual relativeTo,
        DragAction dragAction,
        DockOperationHandler validate,
        DockOperationHandler? visible = null)
    {
        return ShowIndicatorsOnly 
            ? GetDockOperationIndicatorsOnly(point, dropControl, relativeTo, dragAction, visible) 
            : GetDockOperationFromSelectors(point, relativeTo, dragAction, validate, visible);
    }

    private DockOperation GetDockOperationIndicatorsOnly(
        Point point, 
        Control dropControl, 
        Visual relativeTo,
        DragAction dragAction, 
        DockOperationHandler? visible)
    {
        var operation = DockProperties.GetIndicatorDockOperation(dropControl);

        IndicatorOperations.TryGetValue(operation, out var indicator);

        foreach (var kvp in IndicatorOperations)
        {
            if (indicator != kvp.Value)
            {
                kvp.Value.Opacity = 0;
            }
        }

        return InvalidateIndicatorOnly(dropControl, indicator, point, relativeTo, operation, dragAction, visible)
            ? operation
            : DefaultDockOperation;
    }

    private DockOperation GetDockOperationFromSelectors(
        Point point, 
        Visual relativeTo, 
        DragAction dragAction,
        DockOperationHandler validate, 
        DockOperationHandler? visible)
    {
        var result = DefaultDockOperation;

        foreach (var kvp in IndicatorOperations)
        {
            var operation = kvp.Key;
            SelectorsOperations.TryGetValue(operation, out var selector);

            if (InvalidateIndicator(selector, kvp.Value, point, relativeTo, operation, dragAction,
                    validate, visible))
            {
                result = operation;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if the provided control is a dock target selector.
    /// </summary>
    /// <param name="selector">Control to check.</param>
    /// <returns>True if the control is a selector.</returns>
    protected bool IsDockTargetSelector(Control selector)
    {
        foreach (var kvp in SelectorsOperations)
        {
            if (kvp.Value == selector)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Invalidates the indicator based on the provided parameters.
    /// </summary>
    /// <param name="selector">Selector used to hit test the pointer.</param>
    /// <param name="indicator">Visual indicator to update.</param>
    /// <param name="point">Pointer position relative to <paramref name="relativeTo"/>.</param>
    /// <param name="relativeTo">Visual used for coordinate translation.</param>
    /// <param name="operation">Dock operation represented by the selector.</param>
    /// <param name="dragAction">Current drag action type.</param>
    /// <param name="validate">Callback validating the operation.</param>
    /// <param name="visible">Optional callback determining indicator visibility.</param>
    /// <returns>True if the indicator should be shown as active.</returns>
    private bool InvalidateIndicator(
        Control? selector,
        Control? indicator,
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
            // Check if the input element is the selector itself.
            if (selector.InputHitTest(selectorPoint.Value) is { } inputElement)
            {
                if (Equals(inputElement, selector))
                {
                    if (validate(point, operation, dragAction, relativeTo))
                    {
                        indicator.Opacity = 0.5;
                        return true;
                    }
                }
            }
        }

        indicator.Opacity = 0;
        return false;
    }

    /// <summary>
    /// Invalidates the indicator when only drop indicators are shown.
    /// </summary>
    /// <param name="selector">Selector used to translate the pointer.</param>
    /// <param name="indicator">Visual indicator to update.</param>
    /// <param name="point">Pointer position relative to <paramref name="relativeTo"/>.</param>
    /// <param name="relativeTo">Visual used for coordinate translation.</param>
    /// <param name="operation">Dock operation represented by the selector.</param>
    /// <param name="dragAction">Current drag action type.</param>
    /// <param name="visible">Optional callback determining indicator visibility.</param>
    /// <returns>True if the indicator should be shown as active.</returns>
    private bool InvalidateIndicatorOnly(
        Control? selector,
        Control? indicator,
        Point point,
        Visual relativeTo,
        DockOperation operation,
        DragAction dragAction,
        DockOperationHandler? visible)
    {
        if (selector is null || indicator is null)
        {
            return false;
        }

        if (visible is not null && !visible(point, operation, dragAction, relativeTo))
        {
            indicator.Opacity = 0;
            return false;
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
            indicator.Opacity = 0.5;
            return true;
        }

        indicator.Opacity = 0;
        return false;
    }

    void IDockTarget.Reset()
    {
        foreach (var control in IndicatorOperations.Values.Concat(SelectorsOperations.Values))
        {
            control.Opacity = 0;
        }

        ShowHorizontalTargets = true;
        ShowVerticalTargets = true;
    }
}
