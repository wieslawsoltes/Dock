// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

/// <summary>
/// Dock helpers.
/// </summary>
internal static class DockHelpers
{
    public static DockFloatingWindowHostMode ResolveFloatingWindowHostMode(Visual? visual)
    {
        if (visual is null)
        {
            return DockSettings.ResolveFloatingWindowHostMode();
        }

        var dockControl = visual as DockControl ?? visual.FindAncestorOfType<DockControl>();
        var root = dockControl?.Layout as IRootDock;
        return DockSettings.ResolveFloatingWindowHostMode(root);
    }

    public static DockFloatingWindowHostMode ResolveFloatingWindowHostMode(IDockable? dockable)
    {
        if (dockable is IRootDock rootDock)
        {
            return DockSettings.ResolveFloatingWindowHostMode(rootDock);
        }

        if (dockable is { Owner: not null, Owner: IRootDock ownerRoot })
        {
            return DockSettings.ResolveFloatingWindowHostMode(ownerRoot);
        }

        if (dockable?.Owner is { Factory: { } factory })
        {
            var root = factory.FindRoot(dockable, _ => true);
            return DockSettings.ResolveFloatingWindowHostMode(root);
        }

        return DockSettings.ResolveFloatingWindowHostMode();
    }

    public static bool IsManagedWindowHostingEnabled(Visual? visual)
    {
        return ResolveFloatingWindowHostMode(visual) == DockFloatingWindowHostMode.Managed;
    }

    public static bool IsManagedWindowHostingEnabled(IDockable? dockable)
    {
        return ResolveFloatingWindowHostMode(dockable) == DockFloatingWindowHostMode.Managed;
    }

    public static Point GetScreenPoint(Visual visual, Point point)
    {
        // var scaling = (visual.GetVisualRoot() as TopLevel)?.RenderScaling ?? 1.0;
        var scaling = (visual.GetVisualRoot() as TopLevel)?.Screens?.ScreenFromVisual(visual)?.Scaling ?? 1.0;
        var screenPoint = visual.PointToScreen(point).ToPoint(scaling);
        return screenPoint;
    }

    private static bool IsHitTestVisible(Visual visual)
    {
        var element = visual as IInputElement;
        return element != null &&
               visual.IsVisible &&
               element.IsHitTestVisible &&
               element.IsEffectivelyEnabled &&
               visual.GetVisualRoot() != null;
    }

    [Conditional("DEBUG")]
    private static void LogDropSearch(string message)
    {
        DockLogger.LogDebug("DropSearch", message);
    }

    public static void LogDropAreas(Visual? root, string context)
    {
        // Avoid walking the visual tree unless diagnostics are explicitly enabled
        if (!DockSettings.EnableDiagnosticsLogging)
        {
            return;
        }

        if (root is null)
        {
            LogDropSearch($"[{context}] Root visual is null.");
            return;
        }

        var dropAreas = new List<Control>();

        foreach (var visual in root.GetVisualDescendants())
        {
            if (visual is Control control && control.IsSet(DockProperties.IsDropAreaProperty))
            {
                dropAreas.Add(control);
            }
        }

        LogDropSearch($"[{context}] Found {dropAreas.Count} drop area controls under {root.GetType().Name}.");
        foreach (var control in dropAreas)
        {
            var value = control.GetValue(DockProperties.IsDropAreaProperty);
            LogDropSearch($"  DropArea {control.GetType().Name}: Value={value}, Bounds={control.Bounds}, IsVisible={control.IsVisible}, IsHitTestVisible={control.IsHitTestVisible}, IsEffectivelyEnabled={control.IsEffectivelyEnabled}");
        }
    }

    public static Control? GetControl(Visual? input, Point point, StyledProperty<bool> property)
    {
        List<Visual>? rawElements = null;
        List<Visual>? inputElements = null;
        try
        {
            rawElements = input?.GetVisualsAt(point).ToList();
            inputElements = input?.GetVisualsAt(point, IsHitTestVisible)?.ToList();
        }
        catch (Exception ex)
        {
            Print(ex);
        }

        if (rawElements is not null)
        {
            LogDropSearch($"Raw hits ({rawElements.Count}) at point {point} on {(input?.GetType().Name ?? "null")}: {string.Join(", ", rawElements.Select(v => v.GetType().Name))}");
        }

        List<Control>? controlsToInspect = null;
        if (inputElements is { } visibleElements)
        {
            controlsToInspect = visibleElements.OfType<Control>().ToList();
        }

        var inputRoot = input?.GetVisualRoot();

        if ((controlsToInspect is null || controlsToInspect.Count == 0) && rawElements is { Count: > 0 })
        {
            LogDropSearch("Hit-test-visible search returned none; falling back to raw visual hits.");
            controlsToInspect = rawElements.OfType<Control>().ToList();
        }

        if (controlsToInspect is null || controlsToInspect.Count == 0)
        {
            LogDropSearch($"No visuals hit for point {point} on {(input?.GetType().Name ?? "null")}.");
            return null;
        }

        LogDropSearch($"Hit {controlsToInspect.Count} visuals at point {point} on {(input?.GetType().Name ?? "null")}.");

        foreach (var control in controlsToInspect)
        {
            if (inputRoot is { } && control.GetVisualRoot() != inputRoot)
            {
                LogDropSearch($"  Candidate {control.GetType().Name}: skipped due to different visual root ({control.GetVisualRoot()?.GetType().Name}).");
                continue;
            }

            var isSet = control.IsSet(property);
            var value = isSet && control.GetValue(property);
            LogDropSearch($"  Candidate {control.GetType().Name}: IsSet={isSet}, Value={value}");
            if (value)
            {
                LogDropSearch($"Selected drop control {control.GetType().Name}.");
                return control;
            }
        }

        LogDropSearch("No control satisfied drop area requirement.");
        return null;
    }

    private static void Print(Exception ex)
    {
        // Exception logging should always emit to help diagnose hit-test/runtime failures
        DockLogger.Log("Exception", ex.Message);
        DockLogger.Log("Exception", ex.StackTrace ?? string.Empty);
        if (ex.InnerException is { })
        {
            Print(ex.InnerException);
        }
    }

    public static DockPoint ToDockPoint(Point point)
    {
        return new(point.X, point.Y);
    }

    public static void ShowWindows(IDockable dockable)
    {
        if (dockable.Owner is IDock {Factory: { } factory} dock)
        {
            if (factory.FindRoot(dock, _ => true) is {ActiveDockable: IRootDock activeRootDockable})
            {
                if (activeRootDockable.ShowWindows.CanExecute(null))
                {
                    activeRootDockable.ShowWindows.Execute(null);
                }
            }
        }
    }

    public static IProportionalDock? FindProportionalDock(IDock dock)
    {
        if (dock.Factory is { } factory)
        {
            return factory.FindDockable(dock, d => d is IProportionalDock) as IProportionalDock;
        }

        return null;
    }
    
    private static int IndexOf(Window[] windowsArray, Window? windowToFind)
    {
        if (windowToFind == null)
        {
            return -1;
        }

        for (var i = 0; i < windowsArray.Length; i++)
        {
            if (ReferenceEquals(windowsArray[i], windowToFind))
            {
                return i;
            }
        }

        return -1;
    }

    private static int GetManagedWindowZOrder(DockControl dockControl)
    {
        if (dockControl.FindAncestorOfType<MdiDocumentWindow>() is { DataContext: IMdiDocument document })
        {
            return document.MdiZIndex;
        }

        return int.MinValue;
    }

    public static IEnumerable<DockControl> GetZOrderedDockControls(IList<IDockControl> dockControls)
    {
        var windows = dockControls
            .OfType<DockControl>()
            .Select(dock => dock.GetVisualRoot() as Window)
            .OfType<Window>()
            .Distinct()
            .ToArray();

        Window.SortWindowsByZOrder(windows);

        return dockControls
            .OfType<DockControl>()
            .Select(dock => (dock, windowOrder: IndexOf(windows, dock.GetVisualRoot() as Window), managedOrder: GetManagedWindowZOrder(dock)))
            .OrderByDescending(x => x.windowOrder)
            .ThenByDescending(x => x.managedOrder)
            .Select(pair => pair.dock);
    }
}
