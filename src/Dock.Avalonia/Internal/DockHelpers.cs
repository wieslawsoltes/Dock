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

namespace Dock.Avalonia.Internal
{
    /// <summary>
    /// Dock helpers.
    /// </summary>
    internal static class DockHelpers
    {
        private static bool IsHitTestVisible(IVisual visual)
        {
            return visual is IInputElement
            {
                IsVisible: true, 
                IsHitTestVisible: true, 
                IsEffectivelyEnabled: true, 
                IsAttachedToVisualTree: true
            };
        }

        private static IEnumerable<IVisual>? GetVisualsAt(IVisual? visual, Point p, Func<IVisual, bool> predicate)
        {
            var root = visual.GetVisualRoot();
            if (root is { })
            {
                var rootPoint = visual.TranslatePoint(p, root);
                if (rootPoint.HasValue)
                {
                    return root.Renderer?.HitTest(rootPoint.Value, visual, predicate);
                }
            }
            return Enumerable.Empty<IVisual>();
        }

        public static IControl? GetControl(IInputElement? input, Point point, AvaloniaProperty<bool> property)
        {
            IEnumerable<IInputElement>? inputElements = null;
            try
            {
                inputElements = GetVisualsAt(input, point, IsHitTestVisible)?.Cast<IInputElement>();
            }
            catch (Exception ex)
            {
                Print(ex);
            }

            var controls = inputElements?.OfType<IControl>().ToList();
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
}
