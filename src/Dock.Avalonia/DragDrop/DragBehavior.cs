// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Dock.Avalonia
{
    /// <summary>
    /// Drag behavior.
    /// </summary>
    public sealed class DragBehavior : Behavior<Control>
    {
        /// <summary>
        /// Define <see cref="Context"/> property.
        /// </summary>
        public static readonly AvaloniaProperty ContextProperty =
            AvaloniaProperty.Register<DragBehavior, object>(nameof(Context));

        /// <summary>
        /// Define <see cref="Handler"/> property.
        /// </summary>
        public static readonly AvaloniaProperty HandlerProperty =
            AvaloniaProperty.Register<DragBehavior, IDragHandler>(nameof(Handler));

        /// <summary>
        /// Define IsEnabled attached property.
        /// </summary>
        public static readonly AvaloniaProperty IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("IsEnabled", typeof(DragBehavior), true, true, BindingMode.TwoWay);

        /// <summary>
        /// Gets or sets drag behavior context.
        /// </summary>
        public object Context
        {
            get => (object)GetValue(ContextProperty);
            set => SetValue(ContextProperty, value);
        }

        /// <summary>
        /// Gets or sets drag handler.
        /// </summary>
        public IDragHandler Handler
        {
            get => (IDragHandler)GetValue(HandlerProperty);
            set => SetValue(HandlerProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether the given control has drag operation enabled.
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
        /// <param name="value">The drag operation flag.</param>
        public static void SetIsEnabled(Control control, bool value)
        {
            control.SetValue(IsEnabledProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PointerPressed += DoDrag;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PointerPressed -= DoDrag;
        }

        private async void DoDrag(object sender, PointerPressedEventArgs e)
        {
            if (GetIsEnabled(AssociatedObject))
            {
                Handler?.BeforeDragDrop(sender, e, Context);

                var data = new DataObject();
                data.Set(DragDataFormats.Context, Context);

                var effect = DragDropEffects.None;
                if (e.InputModifiers.HasFlag(InputModifiers.Alt))
                {
                    effect |= DragDropEffects.Link;
                }
                else if (e.InputModifiers.HasFlag(InputModifiers.Shift))
                {
                    effect |= DragDropEffects.Move;
                }
                else if (e.InputModifiers.HasFlag(InputModifiers.Control))
                {
                    effect |= DragDropEffects.Copy;
                }
                else
                {
                    effect |= DragDropEffects.Move;
                }

                var result = await DragDrop.DoDragDrop(data, effect);
                Handler?.AfterDragDrop(sender, e, Context);
            }
        }
    }
}
