// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DockControl"/> xaml.
    /// </summary>
    public class DockControl : TemplatedControl
    {
        private static List<IVisual> s_dockControls = new List<IVisual>();
        private DockState _dockState = new DockState();

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
