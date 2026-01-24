// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using Dock.Avalonia.CommandBars;
using Dock.Avalonia.Contract;
using Dock.Avalonia.Diagnostics;
using Dock.Avalonia.Internal;
using Dock.Avalonia.Selectors;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DockControl"/> xaml.
/// </summary>
[TemplatePart("PART_ContentControl", typeof(ContentControl))]
[TemplatePart("PART_CommandBarHost", typeof(DockCommandBarHost))]
[TemplatePart("PART_SelectorOverlay", typeof(DockSelectorOverlay))]
[TemplatePart("PART_ManagedWindowLayer", typeof(ManagedWindowLayer))]
public class DockControl : TemplatedControl, IDockControl, IDockSelectorService
{
    private readonly DockManager _dockManager;
    private readonly DockControlState _dockControlState;
    private bool _isInitialized;
    private ContentControl? _contentControl;
    private ManagedWindowLayer? _managedWindowLayer;
    private DockCommandBarHost? _commandBarHost;
    private DockCommandBarManager? _commandBarManager;
    private DockSelectorOverlay? _selectorOverlay;
    private DockSelectorMode _selectorMode;
    private KeyGesture? _selectorGesture;
    private readonly Dictionary<IDockable, long> _activationOrder = new();
    private long _activationCounter;
    private IFactory? _subscribedFactory;
    private IFactory? _managedLayerFactory;

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
    /// Defines the <see cref="HostWindowFactory"/> property.
    /// </summary>
    public static readonly StyledProperty<Func<IHostWindow?>?> HostWindowFactoryProperty =
        AvaloniaProperty.Register<DockControl, Func<IHostWindow?>?>(nameof(HostWindowFactory));

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

    /// <summary>
    /// Defines the <see cref="EnableManagedWindowLayer"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableManagedWindowLayerProperty =
        AvaloniaProperty.Register<DockControl, bool>(nameof(EnableManagedWindowLayer), true);

    /// <summary>
    /// Defines the <see cref="AutoCreateDataTemplates"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoCreateDataTemplatesProperty =
        AvaloniaProperty.Register<DockControl, bool>(nameof(AutoCreateDataTemplates), true);

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

    /// <summary>
    /// Gets or sets the factory used to create host windows.
    /// </summary>
    public Func<IHostWindow?>? HostWindowFactory
    {
        get => GetValue(HostWindowFactoryProperty);
        set => SetValue(HostWindowFactoryProperty, value);
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
    /// Gets or sets whether the managed window layer is enabled for this control.
    /// </summary>
    public bool EnableManagedWindowLayer
    {
        get => GetValue(EnableManagedWindowLayerProperty);
        set => SetValue(EnableManagedWindowLayerProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to automatically create default DataTemplates in code-behind.
    /// When true (default), the control will add default DataTemplates for all dock types.
    /// When false, no DataTemplates are added, allowing complete user control via XAML.
    /// </summary>
    public bool AutoCreateDataTemplates
    {
        get => GetValue(AutoCreateDataTemplatesProperty);
        set => SetValue(AutoCreateDataTemplatesProperty, value);
    }

    /// <inheritdoc/>
    public bool IsOpen => _selectorOverlay?.IsOpen == true;

    private IDragOffsetCalculator _dragOffsetCalculator = new DefaultDragOffsetCalculator();

    /// <summary>
    /// Gets or sets drag offset calculator.
    /// </summary>
    public IDragOffsetCalculator DragOffsetCalculator
    {
        get => _dragOffsetCalculator;
        set
        {
            _dragOffsetCalculator = value;
            if (_dockControlState is { })
            {
                _dockControlState.DragOffsetCalculator = value;
            }
        }
    }

    /// <summary>
    /// Initialize the new instance of the <see cref="DockControl"/>.
    /// </summary>
    public DockControl()
    {
        _dockManager = new DockManager(new DockService());
        _dockControlState = new DockControlState(_dockManager, _dragOffsetCalculator);
        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerReleasedEvent, ReleasedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerMovedEvent, MovedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerEnteredEvent, EnteredHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerExitedEvent, ExitedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerCaptureLostEvent, CaptureLostHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(PointerWheelChangedEvent, WheelChangedHandler, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AddHandler(KeyDownEvent, KeyDownHandler, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, KeyUpHandler, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _contentControl = e.NameScope.Find<ContentControl>("PART_ContentControl");
        _commandBarHost = e.NameScope.Find<DockCommandBarHost>("PART_CommandBarHost");
        _selectorOverlay = e.NameScope.Find<DockSelectorOverlay>("PART_SelectorOverlay");
        _managedWindowLayer = e.NameScope.Find<ManagedWindowLayer>("PART_ManagedWindowLayer");

        InitializeControlRecycling();
        
        if (_contentControl is not null)
        {
            InitializeDefaultDataTemplates();
        }

        UpdateManagedWindowLayer(Layout);
        InitializeCommandBars();
    }

    private void InitializeControlRecycling()
    {
        var recycling = ControlRecyclingDataTemplate.GetControlRecycling(this);
        if (recycling is ControlRecycling shared)
        {
            var local = new ControlRecycling
            {
                TryToUseIdAsKey = shared.TryToUseIdAsKey
            };

            ControlRecyclingDataTemplate.SetControlRecycling(this, local);
        }
    }

    private void InitializeDefaultDataTemplates()
    {
        if (_contentControl?.DataTemplates is null)
        {
            return;
        }

        // Check if auto-creation of DataTemplates is enabled
        if (!AutoCreateDataTemplates)
        {
            return;
        }

        // Create and add default DataTemplates using helper class
        foreach (var template in DockDataTemplateHelper.CreateDefaultDataTemplates())
        {
            _contentControl.DataTemplates.Add(template);
        }
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
        else if (change.Property == EnableManagedWindowLayerProperty)
        {
            UpdateManagedWindowLayer(Layout);
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

        UpdateManagedWindowLayer(layout);

        if (InitializeFactory)
        {
            layout.Factory.ContextLocator ??= new Dictionary<string, Func<object?>>();
            layout.Factory.DockableLocator ??= new Dictionary<string, Func<IDockable?>>();
            layout.Factory.DefaultContextLocator ??= GetContext;
            layout.Factory.DefaultHostWindowLocator ??= GetHostWindow;

            if (layout.Factory.HostWindowLocator is null)
            {
                layout.Factory.HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
                {
                    [nameof(IDockWindow)] = GetHostWindow
                };
            }

            IHostWindow GetHostWindow()
            {
                if (HostWindowFactory is { } factory)
                {
                    return factory();
                }

                return DockSettings.UseManagedWindows
                    ? new ManagedHostWindow()
                    : new HostWindow();
            }

            object? GetContext() => DefaultContext;
        }

        if (InitializeLayout)
        {
            layout.Factory.InitLayout(layout);
        }

        AttachFactoryEvents(layout.Factory);
        if (layout.ActiveDockable is { } activeDockable)
        {
            _activationOrder[activeDockable] = ++_activationCounter;
        }
        _commandBarManager?.Attach(layout);

        _isInitialized = true;
    }

    private void DeInitialize(IDock? layout)
    {
        UnregisterManagedWindowLayer();

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

        DetachFactoryEvents();
        _commandBarManager?.Detach();

        _isInitialized = false;
    }

    private void InitializeCommandBars()
    {
        _commandBarManager?.Detach();
        _commandBarManager = null;

        if (_commandBarHost is null)
        {
            return;
        }

        _commandBarManager = new DockCommandBarManager(_commandBarHost);
        if (Layout is { })
        {
            _commandBarManager.Attach(Layout);
        }
    }

    private void AttachFactoryEvents(IFactory? factory)
    {
        if (ReferenceEquals(_subscribedFactory, factory))
        {
            return;
        }

        DetachFactoryEvents();

        if (factory is null)
        {
            return;
        }

        _subscribedFactory = factory;
        _subscribedFactory.ActiveDockableChanged += FactoryActiveDockableChanged;
    }

    private void DetachFactoryEvents()
    {
        if (_subscribedFactory is null)
        {
            return;
        }

        _subscribedFactory.ActiveDockableChanged -= FactoryActiveDockableChanged;
        _subscribedFactory = null;
    }

    private void FactoryActiveDockableChanged(object? sender, ActiveDockableChangedEventArgs e)
    {
        if (e.Dockable is null || Layout?.Factory is null)
        {
            return;
        }

        var layoutRoot = Layout.Factory.FindRoot(Layout, _ => true);
        var dockableRoot = Layout.Factory.FindRoot(e.Dockable, _ => true);
        if (layoutRoot is not null && dockableRoot is not null && !ReferenceEquals(layoutRoot, dockableRoot))
        {
            return;
        }

        _activationOrder[e.Dockable] = ++_activationCounter;
    }

    private void UpdateManagedWindowLayer(IDock? layout)
    {
        if (_managedWindowLayer is null)
        {
            return;
        }

        if (EnableManagedWindowLayer && DockSettings.UseManagedWindows && layout?.Factory is { } factory)
        {
            if (!ReferenceEquals(_managedLayerFactory, factory))
            {
                UnregisterManagedWindowLayer();
            }

            ManagedWindowRegistry.RegisterLayer(factory, _managedWindowLayer);
            _managedLayerFactory = factory;
            _managedWindowLayer.IsVisible = true;
            return;
        }

        UnregisterManagedWindowLayer();
        _managedWindowLayer.IsVisible = false;
    }

    private void UnregisterManagedWindowLayer()
    {
        if (_managedWindowLayer is null || _managedLayerFactory is null)
        {
            return;
        }

        ManagedWindowRegistry.UnregisterLayer(_managedLayerFactory, _managedWindowLayer);
        _managedLayerFactory = null;
    }

    /// <inheritdoc/>
    public void ShowSelector(DockSelectorMode mode)
    {
        if (!DockSettings.SelectorEnabled)
        {
            return;
        }

        OpenSelector(mode, null);
    }

    /// <inheritdoc/>
    public void HideSelector()
    {
        if (_selectorOverlay is null)
        {
            return;
        }

        _selectorOverlay.IsOpen = false;
        _selectorOverlay.Items = null;
        _selectorOverlay.SelectedItem = null;
        _selectorGesture = null;
    }

    private void KeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (!DockSettings.SelectorEnabled)
        {
            return;
        }

        if (IsOpen && HandleSelectorNavigation(e))
        {
            e.Handled = true;
            return;
        }

        if (TryStartSelector(e))
        {
            e.Handled = true;
        }
    }

    private void KeyUpHandler(object? sender, KeyEventArgs e)
    {
        if (!IsOpen || _selectorGesture is null)
        {
            return;
        }

        var required = _selectorGesture.KeyModifiers;
        if ((e.KeyModifiers & required) == required)
        {
            return;
        }

        CommitSelectorSelection();
        e.Handled = true;
    }

    private bool TryStartSelector(KeyEventArgs e)
    {
        if (_selectorOverlay is null)
        {
            return false;
        }

        if (MatchesSelectorGesture(e, DockSettings.DocumentSelectorKeyGesture, out var reverse))
        {
            OpenSelector(DockSelectorMode.Documents, DockSettings.DocumentSelectorKeyGesture);
            MoveSelectorSelection(reverse ? -1 : 1);
            return true;
        }

        if (MatchesSelectorGesture(e, DockSettings.ToolSelectorKeyGesture, out var toolReverse))
        {
            OpenSelector(DockSelectorMode.Tools, DockSettings.ToolSelectorKeyGesture);
            MoveSelectorSelection(toolReverse ? -1 : 1);
            return true;
        }

        return false;
    }

    private bool HandleSelectorNavigation(KeyEventArgs e)
    {
        if (!IsOpen)
        {
            return false;
        }

        if (e.Key == Key.Escape)
        {
            HideSelector();
            return true;
        }

        if (e.Key == Key.Enter)
        {
            CommitSelectorSelection();
            return true;
        }

        if (e.Key == Key.Tab)
        {
            var reverse = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            MoveSelectorSelection(reverse ? -1 : 1);
            return true;
        }

        if (e.Key == Key.Left || e.Key == Key.Up)
        {
            MoveSelectorSelection(-1);
            return true;
        }

        if (e.Key == Key.Right || e.Key == Key.Down)
        {
            MoveSelectorSelection(1);
            return true;
        }

        return false;
    }

    private void OpenSelector(DockSelectorMode mode, KeyGesture? gesture)
    {
        if (_selectorOverlay is null)
        {
            return;
        }

        _selectorMode = mode;
        _selectorGesture = gesture;
        var items = BuildSelectorItems(mode);
        _selectorOverlay.Items = items;
        _selectorOverlay.Mode = mode;
        _selectorOverlay.IsOpen = true;

        var activeDockable = ResolveActiveDockable(mode);
        _selectorOverlay.SelectedItem = items.FirstOrDefault(item => ReferenceEquals(item.Dockable, activeDockable))
                                        ?? items.FirstOrDefault();
    }

    private void CommitSelectorSelection()
    {
        if (_selectorOverlay?.SelectedItem is { } selected)
        {
            ActivateSelectorItem(selected);
        }

        HideSelector();
    }

    private void MoveSelectorSelection(int delta)
    {
        if (_selectorOverlay?.Items is not { } items || items.Count == 0)
        {
            return;
        }

        var current = _selectorOverlay.SelectedItem;
        var index = -1;
        for (var i = 0; i < items.Count; i++)
        {
            if (ReferenceEquals(items[i], current))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            index = 0;
        }

        var nextIndex = index + delta;
        if (nextIndex < 0)
        {
            nextIndex = items.Count - 1;
        }
        else if (nextIndex >= items.Count)
        {
            nextIndex = 0;
        }

        _selectorOverlay.SelectedItem = items[nextIndex];
    }

    private static bool MatchesSelectorGesture(KeyEventArgs e, KeyGesture gesture, out bool reverse)
    {
        reverse = false;

        if (e.Key != gesture.Key)
        {
            return false;
        }

        var required = gesture.KeyModifiers;
        if ((e.KeyModifiers & required) != required)
        {
            return false;
        }

        var extras = e.KeyModifiers & ~required;
        if (extras != KeyModifiers.None && extras != KeyModifiers.Shift)
        {
            return false;
        }

        reverse = extras.HasFlag(KeyModifiers.Shift);
        return true;
    }

    private IReadOnlyList<DockSelectorItem> BuildSelectorItems(DockSelectorMode mode)
    {
        if (Layout?.Factory is null)
        {
            return Array.Empty<DockSelectorItem>();
        }

        var factory = Layout.Factory;
        var root = factory.FindRoot(Layout, _ => true) as IRootDock;
        if (root is null)
        {
            return Array.Empty<DockSelectorItem>();
        }

        var dockables = new List<IDockable>();
        var visited = new HashSet<IDockable>();
        CollectDockables(root, dockables, visited);
        PruneActivationOrder(dockables);

        var items = new List<DockSelectorItem>();
        foreach (var dockable in dockables)
        {
            if (dockable is IDockSelectorInfo selectorInfo && !selectorInfo.ShowInSelector)
            {
                continue;
            }

            var isDocument = dockable is IDocument;
            var isTool = dockable is ITool;

            if (mode == DockSelectorMode.Documents && !isDocument)
            {
                continue;
            }

            if (mode == DockSelectorMode.Tools && !isTool)
            {
                continue;
            }

            _activationOrder.TryGetValue(dockable, out var activationOrder);
            var dockableRoot = factory.FindRoot(dockable, _ => true) as IRootDock;
            var isFloating = dockableRoot is not null && !ReferenceEquals(dockableRoot, root);
            var isHidden = root.HiddenDockables?.Contains(dockable) == true;
            var isPinned = factory.IsDockablePinned(dockable, root);

            items.Add(new DockSelectorItem(dockable, activationOrder, isDocument, isTool, isPinned, isHidden, isFloating));
        }

        return items
            .OrderByDescending(item => item.ActivationOrder)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void CollectDockables(IDockable dockable, IList<IDockable> dockables, HashSet<IDockable> visited)
    {
        if (!visited.Add(dockable))
        {
            return;
        }

        if (dockable is IDocument or ITool)
        {
            dockables.Add(dockable);
        }

        if (dockable is IRootDock root)
        {
            AddDockables(root.HiddenDockables, dockables, visited);
            AddDockables(root.LeftPinnedDockables, dockables, visited);
            AddDockables(root.RightPinnedDockables, dockables, visited);
            AddDockables(root.TopPinnedDockables, dockables, visited);
            AddDockables(root.BottomPinnedDockables, dockables, visited);

            if (root.Windows is { })
            {
                foreach (var window in root.Windows)
                {
                    if (window.Layout is { })
                    {
                        CollectDockables(window.Layout, dockables, visited);
                    }
                }
            }
        }

        if (dockable is IDock dock && dock.VisibleDockables is { })
        {
            foreach (var child in dock.VisibleDockables)
            {
                CollectDockables(child, dockables, visited);
            }
        }

        if (dockable is ISplitViewDock splitViewDock)
        {
            if (splitViewDock.PaneDockable is { })
            {
                CollectDockables(splitViewDock.PaneDockable, dockables, visited);
            }

            if (splitViewDock.ContentDockable is { } contentDockable
                && !ReferenceEquals(contentDockable, splitViewDock.PaneDockable))
            {
                CollectDockables(contentDockable, dockables, visited);
            }
        }
    }

    private static void AddDockables(IList<IDockable>? source, IList<IDockable> dockables, HashSet<IDockable> visited)
    {
        if (source is null)
        {
            return;
        }

        foreach (var dockable in source)
        {
            CollectDockables(dockable, dockables, visited);
        }
    }

    private void PruneActivationOrder(IReadOnlyCollection<IDockable> dockables)
    {
        if (_activationOrder.Count == 0)
        {
            return;
        }

        var current = new HashSet<IDockable>(dockables);
        var stale = _activationOrder.Keys.Where(key => !current.Contains(key)).ToList();
        foreach (var key in stale)
        {
            _activationOrder.Remove(key);
        }
    }

    private IDockable? ResolveActiveDockable(DockSelectorMode mode)
    {
        if (Layout is null)
        {
            return null;
        }

        var active = Layout.ActiveDockable;
        if (active is IDock activeDock && activeDock.ActiveDockable is { })
        {
            active = activeDock.ActiveDockable;
        }

        return mode switch
        {
            DockSelectorMode.Documents => active is IDocument ? active : null,
            DockSelectorMode.Tools => active is ITool ? active : null,
            _ => active
        };
    }

    private void ActivateSelectorItem(DockSelectorItem item)
    {
        var dockable = item.Dockable;
        var factory = dockable.Factory;
        if (factory is null)
        {
            return;
        }

        var dockableRoot = factory.FindRoot(dockable, _ => true) as IRootDock;
        if (dockableRoot is { HiddenDockables: { } hidden } && hidden.Contains(dockable))
        {
            factory.RestoreDockable(dockable);
        }

        if (dockableRoot is { } root && factory.IsDockablePinned(dockable, root))
        {
            factory.PreviewPinnedDockable(dockable);
        }

        factory.SetActiveDockable(dockable);

        var focusRoot = factory.FindRoot(dockable, d => d is IRootDock { IsFocusableRoot: true }) as IRootDock
                        ?? dockableRoot;
        if (focusRoot is { })
        {
            factory.SetFocusedDockable(focusRoot, dockable);
        }

        BringDockableWindowToFront(dockableRoot);
    }

    private void BringDockableWindowToFront(IRootDock? dockableRoot)
    {
        if (dockableRoot is null || Layout?.Factory is null)
        {
            return;
        }

        var layoutRoot = Layout.Factory.FindRoot(Layout, _ => true) as IRootDock;
        if (layoutRoot is null || ReferenceEquals(layoutRoot, dockableRoot))
        {
            return;
        }

        var window = layoutRoot.Windows?.FirstOrDefault(candidate => ReferenceEquals(candidate.Layout, dockableRoot));
        if (window is null)
        {
            return;
        }

        window.SetActive();
        window.Present(false);
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

        var root = Layout?.Factory?.FindRoot(Layout);

        if (root is { Window: null })
        {
            var windowModel = Layout?.Factory?.CreateWindowFrom(root);

            if (windowModel != null)
            {
                if (TopLevel.GetTopLevel(this) is IHostWindow window)
                {
                    root.Factory?.InitDockWindow(windowModel, root, window);
                }

                root.Window = windowModel;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        var layout = Layout;

        if (_isInitialized)
        {
            DeInitialize(layout);
        }

        NotifyRootWindowClosed(layout);
    }

    private void NotifyRootWindowClosed(IDock? layout)
    {
        if (layout is not IRootDock root)
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is IHostWindow)
        {
            return;
        }

        if (root.Window is null)
        {
            return;
        }

        root.Factory?.OnWindowClosed(root.Window);
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

    private bool ShouldIgnorePressedForWindowDrag(PointerPressedEventArgs e)
    {
        if (e.Source is not Control source)
        {
            return false;
        }

        var tabItem = source.FindAncestorOfType<DocumentTabStripItem>();
        if (tabItem is null)
        {
            return false;
        }

        var tabStrip = tabItem.FindAncestorOfType<DocumentTabStrip>();
        if (tabStrip is null)
        {
            return false;
        }

        return tabStrip.Items is { Count: 1 }
               && tabStrip.DataContext is Dock.Model.Core.IDock { CanCloseLastDockable: false };
    }

    private static bool ShouldIgnorePressedForItemDrag(PointerPressedEventArgs e)
    {
        if (e.Source is not Control source)
        {
            return false;
        }

        return source.FindAncestorOfType<DocumentTabStripItem>() is not null ||
               source.FindAncestorOfType<ToolTabStripItem>() is not null ||
               source.FindAncestorOfType<ToolPinItemControl>() is not null;
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            var pos = e.GetPosition(this);
            if (this.InputHitTest(pos) is Control initialControl)
            {
                IDockable? dockable = null;
                Control? control = initialControl;
                while (control is { } && dockable is null)
                {
                    dockable = control.DataContext as IDockable;
                    control = control.FindAncestorOfType<Control>();
                }

                DockDiagnosticEvents.RaiseSelectDockableRequested(dockable);
            }

            return;
        }

        if (e.Source is Visual visual && visual.FindAncestorOfType<ScrollBar>() != null)
        {
            return;
        }

        if (ShouldIgnorePressedForWindowDrag(e) || ShouldIgnorePressedForItemDrag(e))
        {
            return;
        }

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
        if (e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

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
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

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
