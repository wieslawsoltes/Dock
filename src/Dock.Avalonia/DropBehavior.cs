// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Dock.Avalonia.Controls;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Drop behavior.
    /// </summary>
    public sealed class DropBehavior : Behavior<Control>
    {
        private Control _adorner;

        /// <summary>
        /// Define <see cref="Context"/> property.
        /// </summary>
        public static readonly AvaloniaProperty ContextProperty =
            AvaloniaProperty.Register<DropBehavior, object>(nameof(Context));

        /// <summary>
        /// Define <see cref="Handler"/> property.
        /// </summary>
        public static readonly AvaloniaProperty HandlerProperty =
            AvaloniaProperty.Register<DropBehavior, IDropHandler>(nameof(Handler));

        /// <summary>
        /// Define <see cref="IsTunneled"/> property.
        /// </summary>
        public static readonly AvaloniaProperty IsTunneledProperty =
            AvaloniaProperty.Register<DropBehavior, bool>(nameof(IsTunneled), false);

        /// <summary>
        /// Gets or sets drag behavior context.
        /// </summary>
        public object Context
        {
            get => (object)GetValue(ContextProperty);
            set => SetValue(ContextProperty, value);
        }

        /// <summary>
        /// Gets or sets drop handler.
        /// </summary>
        public IDropHandler Handler
        {
            get => (IDropHandler)GetValue(HandlerProperty);
            set => SetValue(HandlerProperty, value);
        }

        /// <summary>
        /// Gets or sets tunneled event flag.
        /// </summary>
        public bool IsTunneled
        {
            get => (bool)GetValue(IsTunneledProperty);
            set => SetValue(IsTunneledProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            DragDrop.SetAllowDrop(AssociatedObject, true);

            var routes = RoutingStrategies.Direct | RoutingStrategies.Bubble;
            if (IsTunneled)
            {
                routes |= RoutingStrategies.Tunnel;
            }

            AssociatedObject.AddHandler(DragDrop.DragEnterEvent, DragEnter, routes);
            AssociatedObject.AddHandler(DragDrop.DragLeaveEvent, DragLeave, routes);
            AssociatedObject.AddHandler(DragDrop.DragOverEvent, DragOver, routes);
            AssociatedObject.AddHandler(DragDrop.DropEvent, Drop, routes);
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            DragDrop.SetAllowDrop(AssociatedObject, false);
            AssociatedObject.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
            AssociatedObject.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
            AssociatedObject.RemoveHandler(DragDrop.DragOverEvent, DragOver);
            AssociatedObject.RemoveHandler(DragDrop.DropEvent, Drop);
        }

        private void AddAdorner(IVisual visual)
        {
            var layer = AdornerLayer.GetAdornerLayer(visual);

            if (layer != null)
            {
                if (_adorner?.Parent is Panel panel)
                {
                    layer.Children.Remove(_adorner);
                    _adorner = null;
                }

                _adorner = new DockTarget
                {
                    [AdornerLayer.AdornedElementProperty] = visual,
                };

                ((ISetLogicalParent)_adorner).SetParent(visual as ILogical);

                layer.Children.Add(_adorner);
            }
        }

        private void RemoveAdorner(IVisual visual)
        {
            var layer = AdornerLayer.GetAdornerLayer(visual);

            if (layer != null)
            {
                if (_adorner?.Parent is Panel panel)
                {
                    layer.Children.Remove(_adorner);
                    ((ISetLogicalParent)_adorner).SetParent(null);
                    _adorner = null;
                }
            }
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            object sourceContext = e.Data.Get(DragDataFormats.Context);
            object targetContext = Context;
            bool isView = sourceContext is IView view;

            if (Handler?.Validate(sourceContext, targetContext, sender, DockOperation.Fill, e) == false)
            {
                if (!isView)
                {
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
            else
            {
                if (isView && sender is DockPanel panel)
                {
                    if (sender is IVisual visual)
                    {
                        AddAdorner(visual);
                    }
                }

                e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                e.Handled = true;
            }
        }

        private void DragLeave(object sender, RoutedEventArgs e)
        {
            RemoveAdorner(sender as IVisual);

            Handler?.Cancel(sender, e);
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            DockOperation operation = DockOperation.Fill;
            object sourceContext = e.Data.Get(DragDataFormats.Context);
            object targetContext = Context;
            bool isView = sourceContext is IView view;

            if (_adorner is DockTarget target)
            {
                var position = DropHelper.GetPosition(sender, e);

                operation = target.GetDockOperation(e);
            }

            if (Handler?.Validate(sourceContext, targetContext, sender, operation, e) == false)
            {
                if (!isView)
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

        private void Drop(object sender, DragEventArgs e)
        {
            DockOperation operation = DockOperation.Fill;
            object sourceContext = e.Data.Get(DragDataFormats.Context);
            object targetContext = Context;
            bool isView = sourceContext is IView view;

            if (_adorner is DockTarget target)
            {
                operation = target.GetDockOperation(e);
            }

            if (isView && sender is DockPanel panel)
            {
                RemoveAdorner(sender as IVisual);
            }

            if (Handler?.Execute(sourceContext, targetContext, sender, operation, e) == false)
            {
                if (!isView)
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
    }
}
