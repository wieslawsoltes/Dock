// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using Dock.Model;

namespace Dock.Avalonia.Controls
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
    /// Pointer event type.
    /// </summary>
    internal enum EventType
    {
        /// <summary>
        /// Pointer pressed.
        /// </summary>
        Pressed,
        /// <summary>
        /// Pointer released.
        /// </summary>
        Released,
        /// <summary>
        /// Pointer moved.
        /// </summary>
        Moved,
        /// <summary>
        /// Pointer enter.
        /// </summary>
        Enter,
        /// <summary>
        /// Pointer leave.
        /// </summary>
        Leave,
        /// <summary>
        /// Lost capture.
        /// </summary>
        CaptureLost,
        /// <summary>
        /// Wheel changed.
        /// </summary>
        WheelChanged
    }

    /// <summary>
    /// Docking control shared state.
    /// </summary>
    internal class DockState
    {
        private IDockManager _dockManager = new DockManager();
        private AdornerHelper _adornerHelper = new AdornerHelper();
        private IControl _dragControl = null;
        private IControl _dropControl = null;
        private Point _dragStartPoint = default;
        private bool _pointerPressed = false;
        private bool _doDragDrop = false;
        private Point _targetPoint = default;
        private IVisual _targetDockControl = null;

        /// <summary>
        /// Minimum horizontal drag distance to initiate drag operation.
        /// </summary>
        public static double MinimumHorizontalDragDistance = 4;

        /// <summary>
        /// Minimum vertical drag distance to initiate drag operation.
        /// </summary>
        public static double MinimumVerticalDragDistance = 4;

        private DockPoint ToDockPoint(Point point)
        {
            return new DockPoint(point.X, point.Y);
        }

        private void ShowWindows(IDockable dockable)
        {
            if (dockable.Owner is IDock dock && dock.Factory is IFactory factory)
            {
                if (factory.FindRoot(dock) is IDock root && root.CurrentDockable is IDock currentRootDockable)
                {
                    currentRootDockable.ShowWindows();
                }
            }
        }

        private void Enter(Point point, DragAction dragAction, IVisual relativeTo)
        {
            Validate(point, DockOperation.Fill, dragAction, relativeTo);

            if (_dropControl is DockPanel)
            {
                _adornerHelper.AddAdorner(_dropControl);
            }
        }

        private void Over(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var operation = DockOperation.Fill;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(point, relativeTo, dragAction, Validate);
            }

            Validate(point, operation, dragAction, relativeTo);
        }

        private void Drop(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var operation = DockOperation.Window;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(point, relativeTo, dragAction, Validate);
            }

            if (_dropControl is DockPanel)
            {
                _adornerHelper.RemoveAdorner(_dropControl);
            }

            Execute(point, operation, dragAction, relativeTo);
        }

        private void Leave()
        {
            if (_dropControl is DockPanel)
            {
                _adornerHelper.RemoveAdorner(_dropControl);
            }
        }

        private bool Validate(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_dragControl == null || _dropControl == null)
            {
                return false;
            }

            if (_dragControl.DataContext is IDockable sourceDockable && _dropControl.DataContext is IDockable targetDockable)
            {
                _dockManager.Position = ToDockPoint(point);
                _dockManager.ScreenPosition = ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
                return _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
            }

            return false;
        }

        private bool Execute(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_dragControl == null || _dropControl == null)
            {
                return false;
            }

            if (_dragControl.DataContext is IDockable sourceDockable && _dropControl.DataContext is IDockable targetDockable)
            {
                _dockManager.Position = ToDockPoint(point);
                _dockManager.ScreenPosition = ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
                return _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: true);
            }

            return false;
        }

        private IControl GetDragControl(IInputElement input, Point point)
        {
            IControl dragControl = null;

            var controls = input.GetInputElementsAt(point)?.OfType<IControl>().ToList();
            if (controls?.Count > 0)
            {
                foreach (var control in controls)
                {
                    if (control.GetValue(DockControl.IsDragAreaProperty) == true)
                    {
                        dragControl = control;
                        break;
                    }
                }
            }

            return dragControl;
        }

        private IControl GetDropControl(IInputElement input, Point point)
        {
            IControl dropControl = null;

            var controls = input.GetInputElementsAt(point)?.OfType<IControl>().ToList();
            if (controls?.Count > 0)
            {
                foreach (var control in controls)
                {
                    if (control.GetValue(DockControl.IsDropAreaProperty) == true)
                    {
                        dropControl = control;
                        break;
                    }
                }
            }

            return dropControl;
        }

        /// <summary>
        /// Process pointer event.
        /// </summary>
        /// <param name="point">The pointer position.</param>
        /// <param name="delta">The mouse wheel delta.</param>
        /// <param name="eventType">The pointer event type.</param>
        /// <param name="dragAction">The input drag action.</param>
        /// <param name="activeDockControl">The active dock control.</param>
        /// <param name="dockControls">The dock controls.</param>
        public void Process(Point point, Vector delta, EventType eventType, DragAction dragAction, IVisual activeDockControl, IList<IVisual> dockControls)
        {
            if (!(activeDockControl is IInputElement inputActiveDockControl))
            {
                return;
            }

            switch (eventType)
            {
                case EventType.Pressed:
                    {
                        var dragControl = GetDragControl(inputActiveDockControl, point);
                        if (dragControl != null)
                        {
                            Debug.WriteLine($"Drag : {point} : {eventType} : {dragControl.Name} : {dragControl.GetType().Name} : {dragControl.DataContext?.GetType().Name}");
                            _dragControl = dragControl;
                            _dropControl = null;
                            _dragStartPoint = point;
                            _pointerPressed = true;
                            _doDragDrop = false;
                            _targetPoint = default;
                            _targetDockControl = null;
                            break;
                        }
                    }
                    break;
                case EventType.Released:
                    {
                        if (_doDragDrop == true)
                        {
                            if (_dropControl != null && _targetDockControl != null)
                            {
                                Drop(_targetPoint, dragAction, _targetDockControl);
                            }
                            else
                            {
                                // TODO: Create window.
                            }
                        }
                        Leave();
                        _dragControl = null;
                        _dropControl = null;
                        _dragStartPoint = default;
                        _pointerPressed = false;
                        _doDragDrop = false;
                        _targetPoint = default;
                        _targetDockControl = null;
                    }
                    break;
                case EventType.Moved:
                    {
                        if (_pointerPressed == false)
                        {
                            break;
                        }

                        if (_doDragDrop == false)
                        {
                            Vector diff = _dragStartPoint - point;
                            bool haveMinimumDragDistance = (Math.Abs(diff.X) > MinimumHorizontalDragDistance || Math.Abs(diff.Y) > MinimumVerticalDragDistance);
                            if (haveMinimumDragDistance == true)
                            {
                                if (_dragControl.DataContext is IDockable targetDockable)
                                {
                                    ShowWindows(targetDockable);
                                }
                                _doDragDrop = true;
                            }
                        }

                        if (_doDragDrop == true)
                        {
                            Point targetPoint = default;
                            IVisual targetDockControl = null;
                            IControl dropControl = null;

                            foreach (var dockControl in dockControls)
                            {
                                if (dockControl is IInputElement inputDockControl && inputDockControl != inputActiveDockControl)
                                {
                                    var screenPoint = inputActiveDockControl.PointToScreen(point);
                                    var dockControlPoint = dockControl.PointToClient(screenPoint);
                                    Debug.WriteLine($"{point} -> {dockControlPoint}");
                                    if (dockControlPoint == null)
                                    {
                                        continue;
                                    }
                                    dropControl = GetDropControl(inputDockControl, dockControlPoint);
                                    if (dropControl != null)
                                    {
                                        targetPoint = dockControlPoint;
                                        targetDockControl = inputDockControl;
                                        break;
                                    }
                                }
                            }

                            if (dropControl == null)
                            {
                                dropControl = GetDropControl(inputActiveDockControl, point);
                                if (dropControl != null)
                                {
                                    targetPoint = point;
                                    targetDockControl = inputActiveDockControl;
                                }
                            }

                            if (dropControl != null && targetDockControl != null)
                            {
                                Debug.WriteLine($"Drop : {targetPoint} : {eventType} : {dropControl.Name} : {dropControl.GetType().Name} : {dropControl.DataContext?.GetType().Name}");
                                if (_dropControl == dropControl)
                                {
                                    Over(targetPoint, dragAction, targetDockControl);
                                }
                                else
                                {
                                    if (_dropControl != null)
                                    {
                                        Leave();
                                        _dropControl = null;
                                    }

                                    _dropControl = dropControl;
                                    _targetPoint = targetPoint;
                                    _targetDockControl = targetDockControl;

                                    Enter(targetPoint, dragAction, targetDockControl);
                                }
                            }
                            else
                            {
                                Leave();
                                _dropControl = null;
                                _targetPoint = default;
                                _targetDockControl = null;
                            }
                        }
                    }
                    break;
                case EventType.Enter:
                    {
                    }
                    break;
                case EventType.Leave:
                    {
                    }
                    break;
                case EventType.CaptureLost:
                    {
                        Leave();
                        _dragControl = null;
                        _dropControl = null;
                        _dragStartPoint = default;
                        _pointerPressed = false;
                        _doDragDrop = false;
                        _targetPoint = default;
                        _targetDockControl = null;
                    }
                    break;
                case EventType.WheelChanged:
                    {
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Interaction logic for <see cref="DockControl"/> xaml.
    /// </summary>
    public class DockControl : TemplatedControl
    {
        private static List<IVisual> s_dockControls = new List<IVisual>();
        private DockState _dockState = new DockState();

        /// <summary>
        /// Defines the IsDragArea attached property.
        /// </summary>
        public static readonly AttachedProperty<bool> IsDragAreaProperty =
            AvaloniaProperty.RegisterAttached<DockControl, IControl, bool>("IsDragArea", false, false, BindingMode.TwoWay);

        /// <summary>
        /// Defines the IsDropArea attached property.
        /// </summary>
        public static readonly AttachedProperty<bool> IsDropAreaProperty =
            AvaloniaProperty.RegisterAttached<DockControl, IControl, bool>("IsDropArea", false, false, BindingMode.TwoWay);

        /// <summary>
        /// Define IsDragEnabled attached property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsDragEnabledProperty =
            AvaloniaProperty.RegisterAttached<DockControl, IControl, bool>("IsDragEnabled", true, true, BindingMode.TwoWay);

        /// <summary>
        /// Define IsDropEnabled attached property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsDropEnabledProperty =
            AvaloniaProperty.RegisterAttached<DockControl, IControl, bool>("IsDropEnabled", true, true, BindingMode.TwoWay);

        /// <summary>
        /// Gets the value of the IsDragArea attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The IsDragArea attached property.</returns>
        public static bool GetIsDragArea(IControl control)
        {
            return control.GetValue(IsDragAreaProperty);
        }

        /// <summary>
        /// Sets the value of the IsDragArea attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the IsDragArea property.</param>
        public static void SetIsDragArea(IControl control, bool value)
        {
            control.SetValue(IsDragAreaProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsDropArea attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The IsDropArea attached property.</returns>
        public static bool GetIsDropArea(IControl control)
        {
            return control.GetValue(IsDropAreaProperty);
        }

        /// <summary>
        /// Sets the value of the IsDropArea attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the IsDropArea property.</param>
        public static void SetIsDropArea(IControl control, bool value)
        {
            control.SetValue(IsDropAreaProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsDragEnabled attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The IsDragEnabled attached property.</returns>
        public static bool GetIsDragEnabled(IControl control)
        {
            return control.GetValue(IsDragEnabledProperty);
        }

        /// <summary>
        /// Sets the value of the IsDragEnabled attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the IsDragEnabled property.</param>
        public static void SetIsDragEnabled(IControl control, bool value)
        {
            control.SetValue(IsDragEnabledProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsDropEnabled attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The IsDropEnabled attached property.</returns>
        public static bool GetIsDropEnabled(IControl control)
        {
            return control.GetValue(IsDropEnabledProperty);
        }

        /// <summary>
        /// Sets the value of the IsDropEnabled attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the IsDropEnabled property.</param>
        public static void SetIsDropEnabled(IControl control, bool value)
        {
            control.SetValue(IsDropEnabledProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Layout"/> property.
        /// </summary>
        public static readonly StyledProperty<IDock> LayoutProperty =
            AvaloniaProperty.Register<DockControl, IDock>(nameof(Layout));

        /// <summary>
        /// Gets or sets the dock layout.
        /// </summary>
        /// <value>The layout.</value>
        [Content]
        public IDock Layout
        {
            get { return GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        /// <summary>
        /// Initialize the new instance of the <see cref="DockControl"/>.
        /// </summary>
        public DockControl()
        {
            AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerEnterEvent, Enter, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerLeaveEvent, Leave, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerWheelChangedEvent, WheelChanged, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            s_dockControls.Add(this);
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            s_dockControls.Remove(this);
        }

        private DragAction ToDragAction(PointerEventArgs e)
        {
            if (e.InputModifiers.HasFlag(InputModifiers.Alt))
            {
                return DragAction.Link;
            }
            else if (e.InputModifiers.HasFlag(InputModifiers.Shift))
            {
                return DragAction.Move;
            }
            else if (e.InputModifiers.HasFlag(InputModifiers.Control))
            {
                return DragAction.Copy;
            }
            else
            {
                return DragAction.Move;
            }
        }

        private void Pressed(object sender, PointerPressedEventArgs e)
        {
            _dockState.Process(e.GetPosition(this), new Vector(), EventType.Pressed, ToDragAction(e), this, s_dockControls);
        }

        private void Released(object sender, PointerReleasedEventArgs e)
        {
            _dockState.Process(e.GetPosition(this), new Vector(), EventType.Released, ToDragAction(e), this, s_dockControls);
        }

        private void Moved(object sender, PointerEventArgs e)
        {
            _dockState.Process(e.GetPosition(this), new Vector(), EventType.Moved, ToDragAction(e), this, s_dockControls);
        }

        private void Enter(object sender, PointerEventArgs e)
        {
            _dockState.Process(e.GetPosition(this), new Vector(), EventType.Enter, ToDragAction(e), this, s_dockControls);
        }

        private void Leave(object sender, PointerEventArgs e)
        {
            _dockState.Process(e.GetPosition(this), new Vector(), EventType.Leave, ToDragAction(e), this, s_dockControls);
        }

        private void CaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            _dockState.Process(new Point(), new Vector(), EventType.CaptureLost, DragAction.None, this, s_dockControls);
        }

        private void WheelChanged(object sender, PointerWheelEventArgs e)
        {
            _dockState.Process(e.GetPosition(this), e.Delta, EventType.WheelChanged, ToDragAction(e), this, s_dockControls);
        }
    }
}
