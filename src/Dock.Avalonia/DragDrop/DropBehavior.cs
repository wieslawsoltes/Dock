// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

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
            AssociatedObject.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AssociatedObject.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            AssociatedObject.AddHandler(DragDrop.DragOverEvent, DragOver);
            AssociatedObject.AddHandler(DragDrop.DropEvent, Drop);
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
                Handler?.Enter(sender, e, sourceContext, targetContext);
            }
        }

        private void DragLeave(object sender, RoutedEventArgs e)
        {
            if (GetIsEnabled(AssociatedObject))
            {
                Handler?.Leave(sender, e);
            }
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            if (GetIsEnabled(AssociatedObject))
            {
                object sourceContext = e.Data.Get(DragDataFormats.Context);
                object targetContext = Context;
                Handler?.Over(sender, e, sourceContext, targetContext);
            }
        }

        private void Drop(object sender, DragEventArgs e)
        {
            if (GetIsEnabled(AssociatedObject))
            {
                object sourceContext = e.Data.Get(DragDataFormats.Context);
                object targetContext = Context;
                Handler?.Drop(sender, e, sourceContext, targetContext);
            }
        }
    }
}
