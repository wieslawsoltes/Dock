// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Avalonia.Automation.Peers;
using Dock.Avalonia.Internal;
using Dock.Avalonia.Mdi;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="MdiDocumentWindow"/> xaml.
/// </summary>
[PseudoClasses(":active", ":maximized", ":minimized")]
public class MdiDocumentWindow : TemplatedControl
{
    /// <summary>
    /// Define the <see cref="IconTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentWindow, object?>(nameof(IconTemplate));

    /// <summary>
    /// Define the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentWindow, IDataTemplate>(nameof(HeaderTemplate));

    /// <summary>
    /// Define the <see cref="ModifiedTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ModifiedTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentWindow, IDataTemplate>(nameof(ModifiedTemplate));

    /// <summary>
    /// Define the <see cref="CloseTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> CloseTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentWindow, IDataTemplate>(nameof(CloseTemplate));

    /// <summary>
    /// Define the <see cref="CloseButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> CloseButtonThemeProperty =
        AvaloniaProperty.Register<MdiDocumentWindow, ControlTheme?>(nameof(CloseButtonTheme));

    /// <summary>
    /// Define the <see cref="DocumentContextMenu"/> property.
    /// </summary>
    public static readonly StyledProperty<ContextMenu?> DocumentContextMenuProperty =
        AvaloniaProperty.Register<MdiDocumentWindow, ContextMenu?>(nameof(DocumentContextMenu));

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<MdiDocumentWindow, bool>(nameof(IsActive));

    /// <summary>
    /// Define the <see cref="MdiState"/> property.
    /// </summary>
    public static readonly StyledProperty<MdiWindowState> MdiStateProperty =
        AvaloniaProperty.Register<MdiDocumentWindow, MdiWindowState>(nameof(MdiState));

    private IDisposable? _dataContextSubscription;
    private INotifyPropertyChanged? _dockSubscription;
    private INotifyPropertyChanged? _documentSubscription;
    private IMdiDocument? _currentDocument;
    private IDock? _currentDock;
    private Control? _header;
    private Control? _dragHandle;
    private Control? _contentBorder;
    private Button? _minimizeButton;
    private Button? _maximizeRestoreButton;
    private MdiWindowState _lastNonMinimizedState = MdiWindowState.Normal;
    private bool _isDragging;
    private bool _isResizing;
    private IPointer? _capturedPointer;
    private Point _dragStartPoint;
    private Rect _dragStartBounds;
    private MdiResizeDirection _resizeDirection;
    private Point _lastPointerPosition;
    private bool _managedWindowDragActive;
    private IDockWindow? _managedDragWindow;

    /// <summary>
    /// Gets or sets tab icon template.
    /// </summary>
    public object? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab header template.
    /// </summary>
    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab modified template.
    /// </summary>
    public IDataTemplate ModifiedTemplate
    {
        get => GetValue(ModifiedTemplateProperty);
        set => SetValue(ModifiedTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab close template.
    /// </summary>
    public IDataTemplate CloseTemplate
    {
        get => GetValue(CloseTemplateProperty);
        set => SetValue(CloseTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the close button theme.
    /// </summary>
    public ControlTheme? CloseButtonTheme
    {
        get => GetValue(CloseButtonThemeProperty);
        set => SetValue(CloseButtonThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the document context menu.
    /// </summary>
    public ContextMenu? DocumentContextMenu
    {
        get => GetValue(DocumentContextMenuProperty);
        set => SetValue(DocumentContextMenuProperty, value);
    }

    /// <summary>
    /// Gets or sets if this is the currently active dockable.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets MDI window state.
    /// </summary>
    public MdiWindowState MdiState
    {
        get => GetValue(MdiStateProperty);
        set => SetValue(MdiStateProperty, value);
    }

    /// <summary>
    /// Initializes new instance of the <see cref="MdiDocumentWindow"/> class.
    /// </summary>
    public MdiDocumentWindow()
    {
        UpdatePseudoClasses(IsActive, MdiState);
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new MdiDocumentWindowAutomationPeer(this);
    }

    internal bool TryGetContentOffset(out Point offset)
    {
        offset = default;

        if (_contentBorder is null)
        {
            ApplyTemplate();
        }

        if (_contentBorder is null)
        {
            return false;
        }

        var origin = _contentBorder.TranslatePoint(new Point(0, 0), this);
        if (!origin.HasValue)
        {
            return false;
        }

        offset = origin.Value;
        return true;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _dataContextSubscription = this.GetObservable(DataContextProperty)
            .Subscribe(new AnonymousObserver<object?>(OnDataContextChanged));

        AddHandler(PointerMovedEvent, PointerMovedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, PointerReleasedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerCaptureLostEvent, PointerCaptureLostHandler, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _dataContextSubscription?.Dispose();
        _dataContextSubscription = null;
        DetachDockSubscription();

        RemoveHandler(PointerMovedEvent, PointerMovedHandler);
        RemoveHandler(PointerReleasedEvent, PointerReleasedHandler);
        RemoveHandler(PointerCaptureLostEvent, PointerCaptureLostHandler);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_contentBorder is { })
        {
            _contentBorder.RemoveHandler(PointerPressedEvent, ContentPressedHandler);
        }

        _header = e.NameScope.Find<Control>("PART_Header");
        _dragHandle = e.NameScope.Find<Control>("PART_DragHandle");
        _contentBorder = e.NameScope.Find<Control>("PART_ContentBorder");
        _minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton");
        _maximizeRestoreButton = e.NameScope.Find<Button>("PART_MaximizeRestoreButton");

        if (_header is { })
        {
            _header.AddHandler(PointerPressedEvent, HeaderPressedHandler, RoutingStrategies.Tunnel);
            _header.AddHandler(DoubleTappedEvent, HeaderDoubleTappedHandler, RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
        }

        if (_contentBorder is { })
        {
            _contentBorder.AddHandler(PointerPressedEvent, ContentPressedHandler, RoutingStrategies.Tunnel);
        }

        if (_minimizeButton is { })
        {
            _minimizeButton.Click += MinimizeButtonClicked;
        }

        if (_maximizeRestoreButton is { })
        {
            _maximizeRestoreButton.Click += MaximizeRestoreButtonClicked;
        }

        AttachResizeHandle(e.NameScope.Find<Control>("PART_ResizeTop"), MdiResizeDirection.Top);
        AttachResizeHandle(e.NameScope.Find<Control>("PART_ResizeBottom"), MdiResizeDirection.Bottom);
        AttachResizeHandle(e.NameScope.Find<Control>("PART_ResizeLeft"), MdiResizeDirection.Left);
        AttachResizeHandle(e.NameScope.Find<Control>("PART_ResizeRight"), MdiResizeDirection.Right);
        AttachResizeHandle(e.NameScope.Find<Control>("PART_ResizeTopLeft"), MdiResizeDirection.Top | MdiResizeDirection.Left);
        AttachResizeHandle(e.NameScope.Find<Control>("PART_ResizeTopRight"), MdiResizeDirection.Top | MdiResizeDirection.Right);
        AttachResizeHandle(e.NameScope.Find<Control>("PART_ResizeBottomLeft"), MdiResizeDirection.Bottom | MdiResizeDirection.Left);
        AttachResizeHandle(e.NameScope.Find<Control>("PART_ResizeBottomRight"), MdiResizeDirection.Bottom | MdiResizeDirection.Right);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MdiStateProperty && MdiState != MdiWindowState.Minimized)
        {
            _lastNonMinimizedState = MdiState;
        }

        if (change.Property == IsActiveProperty || change.Property == MdiStateProperty)
        {
            UpdatePseudoClasses(IsActive, MdiState);
        }
    }

    private void OnDataContextChanged(object? dataContext)
    {
        DetachDockSubscription();

        _currentDocument = dataContext as IMdiDocument;
        _currentDock = _currentDocument?.Owner as IDock;
        _lastNonMinimizedState = _currentDocument?.MdiState is { } state && state != MdiWindowState.Minimized
            ? state
            : MdiWindowState.Normal;

        if (_currentDocument is INotifyPropertyChanged documentChanged)
        {
            _documentSubscription = documentChanged;
            _documentSubscription.PropertyChanged += DocumentPropertyChanged;
        }

        if (_currentDock is INotifyPropertyChanged propertyChanged)
        {
            _dockSubscription = propertyChanged;
            _dockSubscription.PropertyChanged += DockPropertyChanged;
        }

        UpdateIsActive();
    }

    private void DockPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDock.ActiveDockable))
        {
            UpdateIsActive();
        }
    }

    private void DocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName == nameof(IMdiDocument.MdiBounds)
            || e.PropertyName == nameof(IMdiDocument.MdiState)
            || e.PropertyName == nameof(IMdiDocument.MdiZIndex))
        {
            InvalidateMdiLayout();
        }
    }

    private void UpdateIsActive()
    {
        if (_currentDock is null || _currentDocument is null)
        {
            SetCurrentValue(IsActiveProperty, false);
            return;
        }

        SetCurrentValue(IsActiveProperty, ReferenceEquals(_currentDock.ActiveDockable, _currentDocument));
    }

    private void DetachDockSubscription()
    {
        if (_documentSubscription is not null)
        {
            _documentSubscription.PropertyChanged -= DocumentPropertyChanged;
            _documentSubscription = null;
        }

        if (_dockSubscription is not null)
        {
            _dockSubscription.PropertyChanged -= DockPropertyChanged;
            _dockSubscription = null;
        }

        _currentDocument = null;
        _currentDock = null;
    }

    private void HeaderPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (_currentDocument is null || _currentDocument.MdiState != MdiWindowState.Normal)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (IsWithinDragHandle(e.Source as Control) || IsWithinButton(e.Source as Control))
        {
            return;
        }

        ActivateDocument();
        BeginDrag(e);
    }

    private void HeaderDoubleTappedHandler(object? sender, RoutedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        ToggleMaximizeRestore();
    }

    private void ContentPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        ActivateDocument();
    }

    private void BeginDrag(PointerPressedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        if (GetValue(DockProperties.IsDragEnabledProperty) != true)
        {
            return;
        }

        if (_currentDocument is IDockable { CanDrag: false })
        {
            return;
        }

        if (!TryBeginManagedWindowDrag())
        {
            return;
        }

        _isDragging = true;
        _dragStartPoint = GetPointerPosition(e);
        _lastPointerPosition = _dragStartPoint;
        _dragStartBounds = ToAvaloniaRect(_currentDocument.MdiBounds);
        _capturedPointer = e.Pointer;
        e.Pointer.Capture(this);
        e.Handled = true;
        ProcessManagedWindowDrag(_lastPointerPosition, EventType.Pressed);
    }

    private void PointerMovedHandler(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            DragMove(e);
            return;
        }

        if (_isResizing)
        {
            ResizeMove(e);
        }
    }

    private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _lastPointerPosition = GetPointerPosition(e);
            ProcessManagedWindowDrag(_lastPointerPosition, EventType.Released);
        }

        EndDragOrResize();
    }

    private void PointerCaptureLostHandler(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_isDragging)
        {
            ProcessManagedWindowDrag(_lastPointerPosition, EventType.CaptureLost);
        }

        EndDragOrResize();
    }

    private void DragMove(PointerEventArgs e)
    {
        if (_currentDocument is null || _currentDocument.MdiState != MdiWindowState.Normal)
        {
            return;
        }

        var position = GetPointerPosition(e);
        _lastPointerPosition = position;
        var delta = position - _dragStartPoint;
        var manager = GetLayoutManager(out var entries, out var finalSize);
        var bounds = manager.GetDragBounds(_currentDocument, _dragStartBounds, delta, finalSize, entries);
        if (DockSettings.EnableWindowMagnetism && entries.Count > 0)
        {
            bounds = ApplyWindowMagnetism(bounds, _currentDocument, entries);
            bounds = ClampToAvailableSize(bounds, entries, finalSize);
        }
        _currentDocument.MdiBounds = ToDockRect(bounds);
        InvalidateMdiLayout();
        e.Handled = true;
        ProcessManagedWindowDrag(_lastPointerPosition, EventType.Moved);
        NotifyManagedWindowDrag();
    }

    private void ResizeMove(PointerEventArgs e)
    {
        if (_currentDocument is null || _currentDocument.MdiState != MdiWindowState.Normal)
        {
            return;
        }

        var position = GetPointerPosition(e);
        var delta = position - _dragStartPoint;
        var manager = GetLayoutManager(out var entries, out var finalSize);
        var bounds = manager.GetResizeBounds(_currentDocument, _dragStartBounds, delta, _resizeDirection, finalSize, entries);
        _currentDocument.MdiBounds = ToDockRect(bounds);
        InvalidateMdiLayout();
        e.Handled = true;
    }

    private void EndDragOrResize()
    {
        if (_managedWindowDragActive)
        {
            NotifyManagedWindowDragEnd();
        }

        _isDragging = false;
        _isResizing = false;
        _resizeDirection = MdiResizeDirection.None;
        if (_capturedPointer is { })
        {
            _capturedPointer.Capture(null);
            _capturedPointer = null;
        }
    }

    private void ProcessManagedWindowDrag(Point localPosition, EventType eventType)
    {
        if (_currentDocument is not ManagedDockWindowDocument managedDocument)
        {
            return;
        }

        if (managedDocument.Window?.Host is not ManagedHostWindow managedHost)
        {
            return;
        }

        var relativeVisual = this.FindAncestorOfType<MdiLayoutPanel>() as Visual ?? this;
        var screenPoint = relativeVisual.PointToScreen(localPosition);
        managedHost.ProcessDrag(screenPoint, eventType);
    }

    private bool TryBeginManagedWindowDrag()
    {
        if (_currentDocument is not ManagedDockWindowDocument managedDocument)
        {
            _managedWindowDragActive = false;
            _managedDragWindow = null;
            return true;
        }

        var window = managedDocument.Window;
        if (window?.Factory is not { } factory)
        {
            _managedWindowDragActive = false;
            _managedDragWindow = null;
            return true;
        }

        if (!factory.OnWindowMoveDragBegin(window))
        {
            _managedWindowDragActive = false;
            _managedDragWindow = null;
            return false;
        }

        if (DockSettings.BringWindowsToFrontOnDrag)
        {
            WindowActivationHelper.ActivateAllWindows(factory, this);
        }

        _managedWindowDragActive = true;
        _managedDragWindow = window;
        return true;
    }

    private void NotifyManagedWindowDrag()
    {
        if (_managedWindowDragActive && _managedDragWindow?.Factory is { } factory)
        {
            factory.OnWindowMoveDrag(_managedDragWindow);
        }
    }

    private void NotifyManagedWindowDragEnd()
    {
        if (_managedWindowDragActive && _managedDragWindow?.Factory is { } factory)
        {
            factory.OnWindowMoveDragEnd(_managedDragWindow);
        }

        _managedWindowDragActive = false;
        _managedDragWindow = null;
    }

    private void AttachResizeHandle(Control? handle, MdiResizeDirection direction)
    {
        if (handle is null)
        {
            return;
        }

        handle.AddHandler(PointerPressedEvent, (_, args) => BeginResize(args, direction), RoutingStrategies.Tunnel);
    }

    private void BeginResize(PointerPressedEventArgs e, MdiResizeDirection direction)
    {
        if (_currentDocument is null || _currentDocument.MdiState != MdiWindowState.Normal)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        ActivateDocument();
        _isResizing = true;
        _resizeDirection = direction;
        _dragStartPoint = GetPointerPosition(e);
        _dragStartBounds = ToAvaloniaRect(_currentDocument.MdiBounds);
        _capturedPointer = e.Pointer;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void MinimizeButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        if (_currentDocument.MdiState == MdiWindowState.Minimized)
        {
            _currentDocument.MdiState = _lastNonMinimizedState;
        }
        else
        {
            _lastNonMinimizedState = _currentDocument.MdiState;
            _currentDocument.MdiState = MdiWindowState.Minimized;
        }
        InvalidateMdiLayout();
    }

    private void MaximizeRestoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        ToggleMaximizeRestore();
    }

    private void ToggleMaximizeRestore()
    {
        if (_currentDocument is null)
        {
            return;
        }

        _currentDocument.MdiState = _currentDocument.MdiState switch
        {
            MdiWindowState.Maximized => MdiWindowState.Normal,
            MdiWindowState.Minimized => MdiWindowState.Normal,
            _ => MdiWindowState.Maximized
        };
        InvalidateMdiLayout();
    }

    private void ActivateDocument()
    {
        if (_currentDocument is not IDockable dockable || dockable.Owner is not IDock owner || owner.Factory is null)
        {
            return;
        }

        owner.Factory.SetActiveDockable(dockable);
        if (owner.Factory.FindRoot(dockable, _ => true) is { } root)
        {
            owner.Factory.SetFocusedDockable(root, dockable);
        }

        var manager = GetLayoutManager(out var entries, out _);
        if (entries.Count == 0)
        {
            manager.UpdateZOrder(new[] { _currentDocument }, _currentDocument);
            return;
        }

        var documents = new List<IMdiDocument>(entries.Count);
        foreach (var entry in entries)
        {
            documents.Add(entry.Document);
        }

        manager.UpdateZOrder(documents, _currentDocument);
    }

    private IMdiLayoutManager GetLayoutManager(out IReadOnlyList<MdiLayoutEntry> entries, out Size finalSize)
    {
        if (this.FindAncestorOfType<MdiLayoutPanel>() is { } panel)
        {
            finalSize = panel.Bounds.Size;
            entries = BuildLayoutEntries(panel);
            return panel.LayoutManager ?? ClassicMdiLayoutManager.Instance;
        }

        finalSize = Bounds.Size;
        entries = _currentDocument is { }
            ? new[] { new MdiLayoutEntry(this, _currentDocument) }
            : Array.Empty<MdiLayoutEntry>();
        return ClassicMdiLayoutManager.Instance;
    }

    private static IReadOnlyList<MdiLayoutEntry> BuildLayoutEntries(MdiLayoutPanel panel)
    {
        if (panel.Children.Count == 0)
        {
            return Array.Empty<MdiLayoutEntry>();
        }

        var entries = new List<MdiLayoutEntry>(panel.Children.Count);
        foreach (var child in panel.Children)
        {
            if (child.DataContext is IMdiDocument document)
            {
                entries.Add(new MdiLayoutEntry(child, document));
            }
        }

        return entries;
    }

    private Point GetPointerPosition(PointerEventArgs e)
    {
        if (this.FindAncestorOfType<MdiLayoutPanel>() is { } panel)
        {
            return e.GetPosition(panel);
        }

        return e.GetPosition(this);
    }

    private void InvalidateMdiLayout()
    {
        if (this.FindAncestorOfType<MdiLayoutPanel>() is { } panel)
        {
            panel.InvalidateArrange();
        }
    }

    private static Rect ToAvaloniaRect(DockRect bounds)
    {
        return new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private static DockRect ToDockRect(Rect bounds)
    {
        return new DockRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private static Rect ApplyWindowMagnetism(Rect bounds, IMdiDocument current, IReadOnlyList<MdiLayoutEntry> entries)
    {
        var snap = DockSettings.WindowMagnetDistance;
        if (snap <= 0)
        {
            return bounds;
        }

        var rect = bounds;
        var x = rect.X;
        var y = rect.Y;

        foreach (var entry in entries)
        {
            if (ReferenceEquals(entry.Document, current))
            {
                continue;
            }

            if (entry.Document.MdiState != MdiWindowState.Normal)
            {
                continue;
            }

            var other = ToAvaloniaRect(entry.Document.MdiBounds);
            if (!IsValidBounds(other))
            {
                continue;
            }

            var verticalOverlap = rect.Top < other.Bottom && rect.Bottom > other.Top;
            var horizontalOverlap = rect.Left < other.Right && rect.Right > other.Left;

            if (verticalOverlap)
            {
                if (Math.Abs(rect.Left - other.Right) <= snap)
                {
                    x = other.Right;
                }
                else if (Math.Abs(rect.Right - other.Left) <= snap)
                {
                    x = other.Left - rect.Width;
                }
            }

            if (horizontalOverlap)
            {
                if (Math.Abs(rect.Top - other.Bottom) <= snap)
                {
                    y = other.Bottom;
                }
                else if (Math.Abs(rect.Bottom - other.Top) <= snap)
                {
                    y = other.Top - rect.Height;
                }
            }
        }

        if (x == rect.X && y == rect.Y)
        {
            return rect;
        }

        return new Rect(x, y, rect.Width, rect.Height);
    }

    private static Rect ClampToAvailableSize(Rect bounds, IReadOnlyList<MdiLayoutEntry> entries, Size finalSize)
    {
        var availableSize = GetAvailableSize(entries, finalSize);
        var maxX = Math.Max(0, availableSize.Width - bounds.Width);
        var maxY = Math.Max(0, availableSize.Height - bounds.Height);

        var x = bounds.X;
        if (x < 0)
        {
            x = 0;
        }
        else if (x > maxX)
        {
            x = maxX;
        }

        var y = bounds.Y;
        if (y < 0)
        {
            y = 0;
        }
        else if (y > maxY)
        {
            y = maxY;
        }

        return new Rect(x, y, bounds.Width, bounds.Height);
    }

    private static Size GetAvailableSize(IReadOnlyList<MdiLayoutEntry> entries, Size finalSize)
    {
        var minimizedCount = 0;
        foreach (var entry in entries)
        {
            if (entry.Document.MdiState == MdiWindowState.Minimized)
            {
                minimizedCount++;
            }
        }

        var reservedHeight = minimizedCount > 0
            ? MdiLayoutDefaults.MinimizedHeight + MdiLayoutDefaults.MinimizedSpacing
            : 0;

        return new Size(finalSize.Width, Math.Max(0, finalSize.Height - reservedHeight));
    }

    private static bool IsValidBounds(Rect bounds)
    {
        return !double.IsNaN(bounds.Width)
               && !double.IsNaN(bounds.Height)
               && bounds.Width > 0
               && bounds.Height > 0;
    }

    private bool IsWithinButton(Control? source)
    {
        if (_header is null || source is null)
        {
            return false;
        }

        return source is Button || WindowDragHelper.IsChildOfType<Button>(_header, source);
    }

    private bool IsWithinDragHandle(Control? source)
    {
        if (_dragHandle is null || source is null)
        {
            return false;
        }

        return IsDescendantOf(_dragHandle, source);
    }

    private static bool IsDescendantOf(Control root, Control? control)
    {
        var current = control;
        while (current != null)
        {
            if (ReferenceEquals(current, root))
            {
                return true;
            }

            current = current.Parent as Control;
        }

        return false;
    }

    private void UpdatePseudoClasses(bool isActive, MdiWindowState state)
    {
        PseudoClasses.Set(":active", isActive);
        PseudoClasses.Set(":maximized", state == MdiWindowState.Maximized);
        PseudoClasses.Set(":minimized", state == MdiWindowState.Minimized);
    }
}
