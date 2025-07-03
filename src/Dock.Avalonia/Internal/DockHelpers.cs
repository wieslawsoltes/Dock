﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

/// <summary>
/// Dock helpers.
/// </summary>
internal static class DockHelpers
{
    private static bool IsHitTestVisible(Visual visual)
    {
        var element = visual as IInputElement;
        return element != null &&
               visual.IsVisible &&
               element.IsHitTestVisible &&
               element.IsEffectivelyEnabled &&
               visual.GetVisualRoot() != null;
    }

    public static Control? GetControl(Visual? input, Point point, StyledProperty<bool> property)
    {
        IEnumerable<Visual>? inputElements = null;
        try
        {
            inputElements = input?.GetVisualsAt(point, IsHitTestVisible);
        }
        catch (Exception ex)
        {
            Print(ex);
        }

        var controls = inputElements?.OfType<Control>().ToList();
        if (controls is { })
        {
            foreach (var control in controls)
            {
                if (control.GetValue(property))
                {
                    return control;
                }
            }
        }
        return null;
    }

    private static void Print(Exception ex)
    {
        Debug.WriteLine(ex.Message);
        Debug.WriteLine(ex.StackTrace);
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

    public static RootDockControl? FindRootDockControl(Visual? visual)
    {
        var current = visual;
        while (current is { })
        {
            if (current is RootDockControl rootDock)
            {
                return rootDock;
            }

            current = current.GetVisualParent();
        }

        return null;
    }
}
