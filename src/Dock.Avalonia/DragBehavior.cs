// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Dock.Avalonia
{
    /// <summary>
    /// Drag behavior.
    /// </summary>
    public sealed class DragBehavior : Behavior<Control>
    {
        private Point _dragStartPoint;
        private bool _pointerPressed;
        private bool _doDragDrop;

        /// <summary>
        /// Minimum horizontal drag distance to initiate drag operation.
        /// </summary>
        public static double MinimumHorizontalDragDistance = 4;

        /// <summary>
        /// Minimum vertical drag distance to initiate drag operation.
        /// </summary>
        public static double MinimumVerticalDragDistance = 4;

        /// <summary>
        /// Define <see cref="Context"/> property.
        /// </summary>
        public static readonly AvaloniaProperty ContextProperty =
            AvaloniaProperty.Register<DragBehavior, object>(nameof(Context));

        /// <summary>
        /// Define <see cref="IsTunneled"/> property.
        /// </summary>
        public static readonly AvaloniaProperty IsTunneledProperty =
            AvaloniaProperty.Register<DragBehavior, bool>(nameof(IsTunneled), false);

        /// <summary>
        /// Gets or sets drag behavior context.
        /// </summary>
        public object Context
        {
            get => (object)GetValue(ContextProperty);
            set => SetValue(ContextProperty, value);
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

            var routes = RoutingStrategies.Direct | RoutingStrategies.Bubble;
            if (IsTunneled)
            {
                routes |= RoutingStrategies.Tunnel;
            }

            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, PointerPressed, routes);
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, routes);
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent, PointerMoved, routes);
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, PointerPressed);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
        }

        private void PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.MouseButton == MouseButton.Left)
            {
                _dragStartPoint = e.GetPosition(AssociatedObject);
                _pointerPressed = true;
                _doDragDrop = false;
            }
        }

        private void PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.MouseButton == MouseButton.Left)
            {
                _pointerPressed = false;
                _doDragDrop = false;
            }
        }

        private async void PointerMoved(object sender, PointerEventArgs e)
        {
            Point point = e.GetPosition(AssociatedObject);
            Vector diff = _dragStartPoint - point;
            bool min = (Math.Abs(diff.X) > MinimumHorizontalDragDistance || Math.Abs(diff.Y) > MinimumVerticalDragDistance);
            if (_pointerPressed == true && _doDragDrop == false && min == true)
            {
                _doDragDrop = true;

                var data = new DataObject();

                data.Set(DragDataFormats.Context, Context);

                var effect = DragDropEffects.None;

                if (e.InputModifiers.HasFlag(InputModifiers.Alt))
                    effect |= DragDropEffects.Link;
                else if (e.InputModifiers.HasFlag(InputModifiers.Shift))
                    effect |= DragDropEffects.Move;
                else if (e.InputModifiers.HasFlag(InputModifiers.Control))
                    effect |= DragDropEffects.Copy;
                else
                    effect |= DragDropEffects.Move;

                var result = await DragDrop.DoDragDrop(data, effect);

                _pointerPressed = false;
                _doDragDrop = false;
            }
        }
    }
}
