using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DockControl"/> xaml.
    /// </summary>
    public class DockControl : TemplatedControl, IDockControl
    {
        internal static List<IDockControl> s_dockControls = new List<IDockControl>();

        private readonly DockManager _dockManager;

        private readonly DockControlState _dockControlState;

        /// <summary>
        /// Defines the <see cref="Layout"/> property.
        /// </summary>
        public static readonly StyledProperty<IDock> LayoutProperty =
            AvaloniaProperty.Register<DockControl, IDock>(nameof(Layout));

        /// <summary>
        /// Defines the <see cref="Factory"/> property.
        /// </summary>
        public static readonly StyledProperty<IFactory> FactoryProperty =
            AvaloniaProperty.Register<DockControl, IFactory>(nameof(Factory));

        /// <summary>
        /// Defines the <see cref="InitializeLayout"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> InitializeLayoutProperty =
            AvaloniaProperty.Register<DockControl, bool>(nameof(InitializeLayout), false);

        /// <summary>
        /// Defines the <see cref="InitializeFactory"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> InitializeFactoryProperty =
            AvaloniaProperty.Register<DockControl, bool>(nameof(InitializeFactory), false);

        /// <inheritdoc/>
        public IList<IDockControl>? DockControls => s_dockControls;

        /// <inheritdoc/>
        public IDockManager? DockManager => _dockManager;

        /// <inheritdoc/>
        public IDockControlState? DockControlState => _dockControlState;

        /// <inheritdoc/>
        [Content]
        public IDock Layout
        {
            get => GetValue(LayoutProperty);
            set => SetValue(LayoutProperty, value);
        }

        /// <inheritdoc/>
        public IFactory Factory
        {
            get => GetValue(FactoryProperty);
            set => SetValue(FactoryProperty, value);
        }

        /// <inheritdoc/>
        public bool InitializeLayout
        {
            get => GetValue(InitializeLayoutProperty);
            set => SetValue(InitializeLayoutProperty, value);
        }

        /// <inheritdoc/>
        public bool InitializeFactory
        {
            get => GetValue(InitializeFactoryProperty);
            set => SetValue(InitializeFactoryProperty, value);
        }

        /// <summary>
        /// Initialize the new instance of the <see cref="DockControl"/>.
        /// </summary>
        public DockControl()
        {
            _dockManager = new DockManager();
            _dockControlState = new DockControlState(_dockManager);

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

            if (InitializeFactory == true && Factory != null)
            {
                Factory.ContextLocator = new Dictionary<string, Func<object>>();

                Factory.HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
                {
                    [nameof(IDockWindow)] = () => new HostWindow()
                };

                Factory.DockableLocator = new Dictionary<string, Func<IDockable>>();
            }

            if (InitializeLayout == true && Factory != null && Layout != null)
            {
                Factory.InitLayout(Layout);
            }
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            s_dockControls.Remove(this);

            if (InitializeLayout == true && Layout != null)
            {
                if (Layout.Close.CanExecute(null))
                {
                    Layout.Close.Execute(null);
                }
            }
        }

        internal static DragAction ToDragAction(PointerEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                return DragAction.Link;
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                return DragAction.Move;
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                return DragAction.Copy;
            }
            else
            {
                return DragAction.Move;
            }
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            _dockControlState.Process(e.GetPosition(this), new Vector(), EventType.Pressed, ToDragAction(e), this, s_dockControls);
        }

        private void Released(object? sender, PointerReleasedEventArgs e)
        {
            _dockControlState.Process(e.GetPosition(this), new Vector(), EventType.Released, ToDragAction(e), this, s_dockControls);
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            _dockControlState.Process(e.GetPosition(this), new Vector(), EventType.Moved, ToDragAction(e), this, s_dockControls);
        }

        private void Enter(object? sender, PointerEventArgs e)
        {
            _dockControlState.Process(e.GetPosition(this), new Vector(), EventType.Enter, ToDragAction(e), this, s_dockControls);
        }

        private void Leave(object? sender, PointerEventArgs e)
        {
            _dockControlState.Process(e.GetPosition(this), new Vector(), EventType.Leave, ToDragAction(e), this, s_dockControls);
        }

        private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            _dockControlState.Process(new Point(), new Vector(), EventType.CaptureLost, DragAction.None, this, s_dockControls);
        }

        private void WheelChanged(object? sender, PointerWheelEventArgs e)
        {
            _dockControlState.Process(e.GetPosition(this), e.Delta, EventType.WheelChanged, ToDragAction(e), this, s_dockControls);
        }
    }
}
