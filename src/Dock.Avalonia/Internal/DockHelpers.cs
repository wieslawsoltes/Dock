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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
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
}
