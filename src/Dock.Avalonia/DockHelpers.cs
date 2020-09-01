using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Model;
using Dock.Model.Controls;

namespace Dock.Avalonia
{
    /// <summary>
    /// Dock helpers.
    /// </summary>
    internal static class DockHelpers
    {
        public static bool IsHitTestVisible(IVisual visual)
        {
            var element = visual as IInputElement;
            return element != null &&
                   element.IsVisible &&
                   element.IsHitTestVisible &&
                   element.IsEffectivelyEnabled &&
                   element.IsAttachedToVisualTree;
        }

        public static IEnumerable<IVisual>? GetVisualsAt(IVisual visual, Point p, Func<IVisual, bool> predicate)
        {
            var root = visual.GetVisualRoot();
            if (root != null)
            {
                var rootPoint = visual.TranslatePoint(p, root);
                if (rootPoint.HasValue)
                {
                    return root.Renderer?.HitTest(rootPoint.Value, visual, predicate);
                }
            }
            return Enumerable.Empty<IVisual>();
        }

        public static IControl? GetControl(IInputElement input, Point point, AvaloniaProperty<bool> property)
        {
            IEnumerable<IInputElement>? inputElements = null;
            try
            {
                inputElements = GetVisualsAt(input, point, IsHitTestVisible)?.Cast<IInputElement>();
                // TODO: GetVisualsAt can throw.
                //inputElements = input.GetVisualsAt(point, IsHitTestVisible)?.Cast<IInputElement>();
                // TODO: GetInputElementsAt can throw.
                // inputElements = input.GetInputElementsAt(point);
            }
            catch (Exception ex)
            {
                Print(ex);
                void Print(Exception exception)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        Print(ex.InnerException);
                    }
                }
            }
            if (inputElements == null)
            {
                return null;
            }
            var controls = inputElements.OfType<IControl>().ToList();
            if (controls != null)
            {
                foreach (var control in controls)
                {
                    if (control.GetValue(property) == true)
                    {
                        return control;
                    }
                }
            }
            return null;
        }

        public static DockPoint ToDockPoint(Point point)
        {
            return new DockPoint(point.X, point.Y);
        }

        public static void ShowWindows(IDockable dockable)
        {
            if (dockable.Owner is IDock dock && dock.Factory is IFactory factory)
            {
                if (factory.FindRoot(dock, (x) => true) is IRootDock root && root.ActiveDockable is IRootDock avtiveRootDockable)
                {
                    avtiveRootDockable.ShowWindows();
                }
            }
        }
    }
}
