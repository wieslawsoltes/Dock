// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Linq;
using Avalonia.Automation.Peers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Dock.Avalonia.Automation.Peers;
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
[PseudoClasses(":preview")]
public abstract class DockTargetBase : TemplatedControl, IDockTarget
{
    /// <summary>Defines the projected docking rectangle's x property.</summary>
    public static readonly DirectProperty<DockTargetBase, double> PreviewXProperty =
        AvaloniaProperty.RegisterDirect<DockTargetBase, double>(nameof(PreviewX), target => target.PreviewX);

    private double _previewX;
    /// <summary>Gets the projected docking rectangle's x.</summary>
    public double PreviewX => _previewX;

    /// <summary>Defines the projected docking rectangle's y property.</summary>
    public static readonly DirectProperty<DockTargetBase, double> PreviewYProperty =
        AvaloniaProperty.RegisterDirect<DockTargetBase, double>(nameof(PreviewY), target => target.PreviewY);

    private double _previewY;
    /// <summary>Gets the projected docking rectangle's y.</summary>
    public double PreviewY => _previewY;

    /// <summary>Defines the projected docking rectangle's width property.</summary>
    public static readonly DirectProperty<DockTargetBase, double> PreviewWidthProperty =
        AvaloniaProperty.RegisterDirect<DockTargetBase, double>(nameof(PreviewWidth), target => target.PreviewWidth);

    private double _previewWidth;
    /// <summary>Gets the projected docking rectangle's width.</summary>
    public double PreviewWidth => _previewWidth;

    /// <summary>Defines the projected docking rectangle's height property.</summary>
    public static readonly DirectProperty<DockTargetBase, double> PreviewHeightProperty =
        AvaloniaProperty.RegisterDirect<DockTargetBase, double>(nameof(PreviewHeight), target => target.PreviewHeight);

    private double _previewHeight;
    /// <summary>Gets the projected docking rectangle's height.</summary>
    public double PreviewHeight => _previewHeight;

    /// <summary>Updates the projected indicator bounds in this adorner's coordinate space.</summary>
    /// <param name="bounds">The projected rectangle, or null to restore legacy indicators.</param>
    public void SetPreviewBounds(Rect? bounds)
    {
        var rect = bounds ?? default;
        SetAndRaise(PreviewXProperty, ref _previewX, rect.X);
        SetAndRaise(PreviewYProperty, ref _previewY, rect.Y);
        SetAndRaise(PreviewWidthProperty, ref _previewWidth, rect.Width);
        SetAndRaise(PreviewHeightProperty, ref _previewHeight, rect.Height);
        PseudoClasses.Set(":preview", bounds.HasValue);
        // A collapsed source may move the projected pane beyond the old target.
        AdornerLayer.SetIsClipEnabled(this, !bounds.HasValue);
    }

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

    /// <summary>
    /// Defines the <see cref="IsGlobalDockAvailable"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGlobalDockAvailableProperty =
        AvaloniaProperty.Register<DockTargetBase, bool>(nameof(IsGlobalDockAvailable));

    /// <summary>
    /// Defines the <see cref="IsGlobalDockActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGlobalDockActiveProperty =
        AvaloniaProperty.Register<DockTargetBase, bool>(nameof(IsGlobalDockActive));

    /// <summary>
    /// Initializes a new instance while wiring pseudo-classes to the target visibility and availability observables.
    /// </summary>
    public DockTargetBase()
    {
        PseudoClasses.Set(":horizontal", this.GetObservable(ShowHorizontalTargetsProperty));
        PseudoClasses.Set(":vertical", this.GetObservable(ShowVerticalTargetsProperty));
        PseudoClasses.Set(":global-available", this.GetObservable(IsGlobalDockAvailableProperty));
        PseudoClasses.Set(":global-active", this.GetObservable(IsGlobalDockActiveProperty));
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new DockTargetAutomationPeer(this);
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
    /// Gets or sets a value indicating whether any global docking options are currently available.
    /// </summary>
    public bool IsGlobalDockAvailable
    {
        get => GetValue(IsGlobalDockAvailableProperty);
        set => SetValue(IsGlobalDockAvailableProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether a global docking operation is active.
    /// </summary>
    public bool IsGlobalDockActive
    {
        get => GetValue(IsGlobalDockActiveProperty);
        set => SetValue(IsGlobalDockActiveProperty, value);
    }

    /// <summary>
    /// A dictionary that maps dock operations to their corresponding indicator controls.
    /// </summary>
    protected Dictionary<DockOperation, Control> IndicatorOperations { get; set; } = new();

    /// <summary>
    /// A dictionary that maps dock operations to their corresponding selector controls.
    /// </summary>
    protected Dictionary<DockOperation, Control> SelectorsOperations { get; set; } = new();

    private bool IsOperationEnabled(DockOperation operation)
    {
        return operation switch
        {
            DockOperation.Left or DockOperation.Right => ShowHorizontalTargets,
            DockOperation.Top or DockOperation.Bottom => ShowVerticalTargets,
            _ => true
        };
    }

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
        SelectorsOperations.TryGetValue(operation, out var selector);

        foreach (var kvp in IndicatorOperations)
        {
            if (indicator != kvp.Value)
            {
                kvp.Value.Opacity = 0;
            }
        }

        if (!IsOperationEnabled(operation))
        {
            if (indicator is not null)
            {
                indicator.Opacity = 0;
            }

            if (selector is not null)
            {
                selector.Opacity = 0;
            }

            return DefaultDockOperation;
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

            if (!IsOperationEnabled(operation))
            {
                if (selector is not null)
                {
                    selector.Opacity = 0;
                }

                if (kvp.Value is not null)
                {
                    kvp.Value.Opacity = 0;
                }

                continue;
            }

            if (InvalidateIndicator(selector, kvp.Value, point, relativeTo, operation, dragAction,
                    validate, visible))
            {
                result = operation;
            }
        }

        foreach (var kvp in IndicatorOperations)
        {
            if (kvp.Key != result)
            {
                kvp.Value.Opacity = 0;
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
        SetPreviewBounds(null);
        foreach (var control in IndicatorOperations.Values.Concat(SelectorsOperations.Values))
        {
            control.Opacity = 0;
        }

        ShowHorizontalTargets = true;
        ShowVerticalTargets = true;
        IsGlobalDockAvailable = false;
        IsGlobalDockActive = false;
    }
}
