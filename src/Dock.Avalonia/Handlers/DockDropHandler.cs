// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model;

namespace Dock.Avalonia
{
    internal class AdornerHelper
    {
        public Control Adorner;

        public void AddAdorner(IVisual visual)
        {
            var layer = AdornerLayer.GetAdornerLayer(visual);
            if (layer != null)
            {
                if (Adorner?.Parent is Panel panel)
                {
                    layer.Children.Remove(Adorner);
                    Adorner = null;
                }

                Adorner = new DockTarget
                {
                    [AdornerLayer.AdornedElementProperty] = visual,
                };

                ((ISetLogicalParent)Adorner).SetParent(visual as ILogical);

                layer.Children.Add(Adorner);
            }
        }

        public void RemoveAdorner(IVisual visual)
        {
            var layer = AdornerLayer.GetAdornerLayer(visual);
            if (layer != null)
            {
                if (Adorner?.Parent is Panel panel)
                {
                    layer.Children.Remove(Adorner);
                    ((ISetLogicalParent)Adorner).SetParent(null);
                    Adorner = null;
                }
            }
        }
    }

    /// <summary>
    /// Dock drop handler.
    /// </summary>
    public class DockDropHandler : IDropHandler
    {
        private IDockManager _dockManager = new DockManager();
        private AdornerHelper _adornerHelper = new AdornerHelper();
        private bool _executed = false;

        /// <summary>
        /// Gets or sets handler id.
        /// </summary>
        public int Id { get; set; }

        private DragAction ToDragAction(DragEventArgs e)
        {
            if (e.DragEffects == DragDropEffects.Copy)
            {
                return DragAction.Copy;
            }
            else if (e.DragEffects == DragDropEffects.Move)
            {
                return DragAction.Move;
            }
            else if (e.DragEffects == DragDropEffects.Link)
            {
                return DragAction.Link;
            }
            return DragAction.None;
        }

        private DockPoint ToDockPoint(Point point)
        {
            return new DockPoint(point.X, point.Y);
        }

        /// <inheritdoc/>
        public void Enter(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            var operation = DockOperation.Fill;
            bool isDockable = sourceContext is IDockable dockable;

            if (Validate(sender, e, sourceContext, targetContext, operation) == false)
            {
                if (!isDockable)
                {
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
            else
            {
                if (isDockable && sender is DockPanel panel)
                {
                    if (sender is IVisual visual)
                    {
                        _adornerHelper.AddAdorner(visual);
                    }
                }

                e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        public void Over(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            bool isDockable = sourceContext is IDockable dockable;
            var operation = DockOperation.Fill;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(e);
            }

            if (Validate(sender, e, sourceContext, targetContext, operation) == false)
            {
                if (!isDockable)
                {
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
            else
            {
                e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        public void Drop(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            var operation = DockOperation.Fill;
            bool isDockable = sourceContext is IDockable dockable;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(e);
            }

            if (isDockable && sender is DockPanel panel)
            {
                _adornerHelper.RemoveAdorner(sender as IVisual);
            }

            if (Execute(sender, e, sourceContext, targetContext, operation) == false)
            {
                if (!isDockable)
                {
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
            else
            {
                e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        public void Leave(object sender, RoutedEventArgs e)
        {
            _adornerHelper.RemoveAdorner(sender as IVisual);
            Cancel(sender, e);
        }

        /// <inheritdoc/>
        public bool Validate(object sender, DragEventArgs e, object sourceContext, object targetContext, object state)
        {
            if (state is DockOperation operation && sourceContext is IDockable sourceDockable && targetContext is IDockable targetDockable)
            {
                _dockManager.Position = ToDockPoint(DropHelper.GetPosition(sender, e));
                _dockManager.ScreenPosition = ToDockPoint(DropHelper.GetPositionScreen(sender, e));
                return _dockManager.ValidateDockable(sourceDockable, targetDockable, ToDragAction(e), operation, false);
            }
            return false;
        }

        /// <inheritdoc/>
        public bool Execute(object sender, DragEventArgs e, object sourceContext, object targetContext, object state)
        {
            if (_executed == false && state is DockOperation operation && sourceContext is IDockable sourceDockable && targetContext is IDockable targetDockable)
            {
                _dockManager.Position = ToDockPoint(DropHelper.GetPosition(sender, e));
                _dockManager.ScreenPosition = ToDockPoint(DropHelper.GetPositionScreen(sender, e));
                bool bResult = _dockManager.ValidateDockable(sourceDockable, targetDockable, ToDragAction(e), operation, true);
                if (bResult == true)
                {
                    _executed = true;
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <inheritdoc/>
        public void Cancel(object sender, RoutedEventArgs e)
        {
            _executed = false;
        }
    }
}
