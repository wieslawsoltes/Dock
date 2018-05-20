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
    public sealed class DragBehavior : Behavior<Control>
    {
        private Point _dragStartPoint;
        private bool _pointerPressed;
        private bool _doDragDrop;

        public static double MinimumHorizontalDragDistance = 4;
        public static double MinimumVerticalDragDistance = 4;

        public static readonly AvaloniaProperty IsTunneledProperty =
            AvaloniaProperty.Register<DragBehavior, bool>(nameof(IsTunneled), true);

        public bool IsTunneled
        {
            get => (bool)GetValue(IsTunneledProperty);
            set => SetValue(IsTunneledProperty, value);
        }

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

                data.Set(DragDataFormats.Source, e.Source);
                data.Set(DragDataFormats.Sender, sender);
                data.Set(DragDataFormats.Object, AssociatedObject);
                data.Set(DragDataFormats.ObjectData, AssociatedObject.DataContext);
                data.Set(DragDataFormats.Parent, AssociatedObject.Parent);
                data.Set(DragDataFormats.ParentData, AssociatedObject.Parent.DataContext);

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
            }
        }
    }
}
