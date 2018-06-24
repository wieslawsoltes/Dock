// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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
        /// Define IsEnabled attached property.
        /// </summary>
        public static readonly AvaloniaProperty IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("IsEnabled", typeof(DropBehavior), true, true, BindingMode.TwoWay);

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

        /// <summary>
        /// Gets a value indicating whether the given control has drop operation enabled.
        /// </summary>
        /// <param name="control">The control object.</param>
        /// <returns>True if drag operation is enabled.</returns>
        public static bool GetIsEnabled(Control control)
        {
            return (bool)control.GetValue(IsEnabledProperty);
        }

        /// <summary>
        /// Sets IsEnabled attached property.
        /// </summary>
        /// <param name="control">The control object.</param>
        /// <param name="value">The drop operation flag.</param>
        public static void SetIsEnabled(Control control, bool value)
        {
            control.SetValue(IsEnabledProperty, value);
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

        private void DragEnter(object sender, DragEventArgs e)
        {
            if (GetIsEnabled(AssociatedObject))
            {
                object sourceContext = e.Data.Get(DragDataFormats.Context);
                object targetContext = Context;

                Enter(sender, e, sourceContext, targetContext);
            }
        }

        private void DragLeave(object sender, RoutedEventArgs e)
        {
            if (GetIsEnabled(AssociatedObject))
            {
                Leave(sender, e);
            }
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            if (GetIsEnabled(AssociatedObject))
            {
                object sourceContext = e.Data.Get(DragDataFormats.Context);
                object targetContext = Context;
                Over(sender, e, sourceContext, targetContext);
            }
        }

        private void Drop(object sender, DragEventArgs e)
        {
            if (GetIsEnabled(AssociatedObject))
            {
                object sourceContext = e.Data.Get(DragDataFormats.Context);
                object targetContext = Context;
                Execute(sender, e, sourceContext, targetContext);
            }
        }

        private class AdornerHelper
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

        private AdornerHelper _adornerHelper = new AdornerHelper();

        private void Enter(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            DockOperation operation = DockOperation.Fill;
            bool isView = sourceContext is IView view;

            if (Handler?.Validate(sender, e, sourceContext, targetContext, operation) == false)
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
                        _adornerHelper.AddAdorner(visual);
                    }
                }

                e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                e.Handled = true;
            }
        }

        private void Over(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            bool isView = sourceContext is IView view;
            DockOperation operation = DockOperation.Fill;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(e);
            }

            if (Handler?.Validate(sender, e, sourceContext, targetContext, operation) == false)
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

        private void Execute(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            DockOperation operation = DockOperation.Fill;
            bool isView = sourceContext is IView view;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(e);
            }

            if (isView && sender is DockPanel panel)
            {
                _adornerHelper.RemoveAdorner(sender as IVisual);
            }

            if (Handler?.Execute(sender, e, targetContext, sourceContext, operation) == false)
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

        private void Leave(object sender, RoutedEventArgs e)
        {
            _adornerHelper.RemoveAdorner(sender as IVisual);

            Handler?.Cancel(sender, e);
        }
    }
}
