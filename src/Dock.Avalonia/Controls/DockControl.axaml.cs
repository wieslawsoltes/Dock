using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DockControl"/> xaml.
/// </summary>
public class DockControl : TemplatedControl, IDockControl
{
    private readonly DockManager _dockManager;
    private readonly DockControlState _dockControlState;
    private bool _isInitialized;

    /// <summary>
    /// Defines the <see cref="Layout"/> property.
    /// </summary>
    public static readonly StyledProperty<IDock?> LayoutProperty =
        AvaloniaProperty.Register<DockControl, IDock?>(nameof(Layout));

    /// <summary>
    /// Defines the <see cref="DefaultContext"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DefaultContextProperty =
        AvaloniaProperty.Register<DockControl, object?>(nameof(DefaultContext));

    /// <summary>
    /// Defines the <see cref="InitializeLayout"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> InitializeLayoutProperty =
        AvaloniaProperty.Register<DockControl, bool>(nameof(InitializeLayout));

    /// <summary>
    /// Defines the <see cref="InitializeFactory"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> InitializeFactoryProperty =
        AvaloniaProperty.Register<DockControl, bool>(nameof(InitializeFactory));

    /// <summary>
    /// Defines the <see cref="Factory"/> property.
    /// </summary>
    public static readonly StyledProperty<IFactory?> FactoryProperty =
        AvaloniaProperty.Register<DockControl, IFactory?>(nameof(Factory));

    /// <summary>
    /// Defines the <see cref="IsDraggingDock"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDraggingDockProperty =
        AvaloniaProperty.Register<DockControl, bool>(nameof(IsDraggingDock));

    /// <inheritdoc/>
    public IDockManager DockManager => _dockManager;

    /// <inheritdoc/>
    public IDockControlState DockControlState => _dockControlState;

    /// <inheritdoc/>
    [Content]
    public IDock? Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <inheritdoc/>
    public object? DefaultContext
    {
        get => GetValue(DefaultContextProperty);
        set => SetValue(DefaultContextProperty, value);
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

    /// <inheritdoc/>
    public IFactory? Factory
    {
        get => GetValue(FactoryProperty);
        set => SetValue(FactoryProperty, value);
    }

    /// <summary>
    /// Gets or sets whether any dock is being dragged.
    /// </summary>
    public bool IsDraggingDock
    {
        get => GetValue(IsDraggingDockProperty);
        set => SetValue(IsDraggingDockProperty, value);
    }

    /// <summary>
    /// Initialize the new instance of the <see cref="DockControl"/>.
    /// </summary>
    public DockControl()
    {
        _dockManager = new DockManager();
        _dockControlState = new DockControlState(_dockManager);
        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerReleasedEvent, ReleasedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerMovedEvent, MovedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerEnteredEvent, EnteredHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerExitedEvent, ExitedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerCaptureLostEvent, CaptureLostHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerWheelChangedEvent, WheelChangedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LayoutProperty)
        {
            if (_isInitialized)
            {
                DeInitialize(change.GetOldValue<IDock>());
            }

            Initialize(change.GetNewValue<IDock>()); 
        }
    }

    private void Initialize(IDock? layout)
    {
        if (layout is null)
        {
            return;
        }

        if (layout.Factory is null)
        {
            if (Factory is { })
            {
                layout.Factory = Factory;
            }
            else
            {
                return;
            }
        }

        layout.Factory.DockControls.Add(this);

        if (InitializeFactory)
        {
            layout.Factory.ContextLocator = new Dictionary<string, Func<object?>>();
            layout.Factory.HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };
            layout.Factory.DockableLocator = new Dictionary<string, Func<IDockable?>>();
            layout.Factory.DefaultContextLocator = GetContext;
            layout.Factory.DefaultHostWindowLocator = GetHostWindow;
 
            IHostWindow GetHostWindow() => new HostWindow();

            object? GetContext() => DefaultContext;
        }

        if (InitializeLayout)
        {
            layout.Factory.InitLayout(layout);
        }

        _isInitialized = true;
    }

    private void DeInitialize(IDock? layout)
    {
        if (layout?.Factory is null)
        {
            return;
        }

        layout.Factory.DockControls.Remove(this);

        if (InitializeLayout)
        {
            if (layout.Close.CanExecute(null))
            {
                layout.Close.Execute(null);
            }
        }

        _isInitialized = false;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (_isInitialized)
        {
            DeInitialize(Layout);
        }

        Initialize(Layout);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_isInitialized)
        {
            DeInitialize(Layout);
        }
    }

    private static DragAction ToDragAction(PointerEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return DragAction.Link;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return DragAction.Move;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return DragAction.Copy;
        }

        return DragAction.Move;
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (Layout?.Factory?.DockControls is { })
        {
            var position = e.GetPosition(this);
            var delta = new Vector();
            var action = ToDragAction(e);
            _dockControlState.Process(position, delta, EventType.Pressed, action, this, Layout.Factory.DockControls);
        }
    }

    private void ReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (Layout?.Factory?.DockControls is { })
        {
            var position = e.GetPosition(this);
            var delta = new Vector();
            var action = ToDragAction(e);
            _dockControlState.Process(position, delta, EventType.Released, action, this, Layout.Factory.DockControls);
        }
    }

    private void MovedHandler(object? sender, PointerEventArgs e)
    {
        if (Layout?.Factory?.DockControls is { })
        {
            var position = e.GetPosition(this);
            var delta = new Vector();
            var action = ToDragAction(e);
            _dockControlState.Process(position, delta, EventType.Moved, action, this, Layout.Factory.DockControls);
        }
    }

    private void EnteredHandler(object? sender, PointerEventArgs e)
    {
        if (Layout?.Factory?.DockControls is { })
        {
            var position = e.GetPosition(this);
            var delta = new Vector();
            var action = ToDragAction(e);
            _dockControlState.Process(position, delta, EventType.Enter, action, this, Layout.Factory.DockControls);
        }
    }

    private void ExitedHandler(object? sender, PointerEventArgs e)
    {
        if (Layout?.Factory?.DockControls is { })
        {
            var position = e.GetPosition(this);
            var delta = new Vector();
            var action = ToDragAction(e);
            _dockControlState.Process(position, delta, EventType.Leave, action, this, Layout.Factory.DockControls);
        }
    }

    private void CaptureLostHandler(object? sender, PointerCaptureLostEventArgs e)
    {
        if (Layout?.Factory?.DockControls is { })
        {
            var position = new Point();
            var delta = new Vector();
            var action = DragAction.None;
            _dockControlState.Process(position, delta, EventType.CaptureLost, action, this, Layout.Factory.DockControls);
        }
    }

    private void WheelChangedHandler(object? sender, PointerWheelEventArgs e)
    {
        if (Layout?.Factory?.DockControls is { })
        {
            var position = e.GetPosition(this);
            var delta = e.Delta;
            var action = ToDragAction(e);
            _dockControlState.Process(position, delta, EventType.WheelChanged, action, this, Layout.Factory.DockControls);
        }
    }
}
