using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvalonDock.Layout;
using AvalonDock.Infrastructure;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvalonDock.Controls;

/// <summary>An AvalonDock floating control implemented by Dock's native host window.</summary>
public abstract class LayoutFloatingWindowControl : HostWindow, ILayoutControl, IHostWindow
{
    /// <summary>Defines whether application content can be replaced after loading.</summary>
    public static readonly StyledProperty<bool> IsContentImmutableProperty =
        AvaloniaProperty.Register<LayoutFloatingWindowControl, bool>(nameof(IsContentImmutable));
    /// <summary>Defines the thickness of the floating window's resize hit areas.</summary>
    public static readonly StyledProperty<Thickness> ResizeBorderThicknessProperty =
        AvaloniaProperty.Register<LayoutFloatingWindowControl, Thickness>(nameof(ResizeBorderThickness), new Thickness(10));
    /// <summary>Defines the current native window drag state.</summary>
    public static readonly DirectProperty<LayoutFloatingWindowControl, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<LayoutFloatingWindowControl, bool>(nameof(IsDragging), window => window.IsDragging);
    /// <summary>Defines whether the native window is maximized.</summary>
    public static readonly DirectProperty<LayoutFloatingWindowControl, bool> IsMaximizedProperty =
        AvaloniaProperty.RegisterDirect<LayoutFloatingWindowControl, bool>(nameof(IsMaximized), window => window.IsMaximized);

    private bool _dragging;
    private bool _maximized;
    private bool _notifyOpened;
    private bool _internalCloseRequested;
    private bool _preserveCloseOrigin;
    private bool _messageHookAttached;
    private bool _observingModel;
    private bool _observeModelRequested;
    private bool _closeEmptyDocument;
    private readonly LayoutFloatingWindow? _floatingModel;

    static LayoutFloatingWindowControl()
    {
        KeyDownEvent.AddClassHandler<LayoutFloatingWindowControl>(static (window, args) => window.OnPreviewKeyDown(args), RoutingStrategies.Tunnel);
        ContentProperty.OverrideMetadata<LayoutFloatingWindowControl>(new StyledPropertyMetadata<object?>(
            coerce: static (owner, value) => owner is LayoutFloatingWindowControl { IsLoaded: true, IsContentImmutable: true } window
                ? window.Content : value));
    }

    /// <summary>Creates a native host for a public layout model.</summary>
    protected LayoutFloatingWindowControl(ILayoutElement model) : this(model, false) { }

    /// <summary>Creates a native host with optional content replacement protection.</summary>
    protected LayoutFloatingWindowControl(ILayoutElement model, bool isContentImmutable)
    {
        ArgumentNullException.ThrowIfNull(model);
        _floatingModel = model as LayoutFloatingWindow;
        if (_observeModelRequested) ObserveLayoutModel();
        IsContentImmutable = isContentImmutable;
        Classes.Add("avalondock");
        if (model is LayoutDocumentFloatingWindow document)
        {
            Tag = document;
            Classes.Add("avalondock-document-window");
        }
        Styles.Add(new AvalonDockTheme());
        if (model.Root?.Manager is { } manager)
        {
            manager.AttachThemeResources(this);
            DockAdornerWindow.SetFactory(this, DockAdornerWindow.GetFactory(manager));
        }
        ((INotifyCollectionChanged)Classes).CollectionChanged += NativeDragChanged;
    }

    /// <summary>Gets the public floating model presented by this native window.</summary>
    public abstract ILayoutElement Model { get; }
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutFloatingWindowControl);
    /// <summary>Gets or sets resize hit-area thickness in device-independent units. Defaults to 10 on each edge.</summary>
    public Thickness ResizeBorderThickness { get => GetValue(ResizeBorderThicknessProperty); set => SetValue(ResizeBorderThicknessProperty, value); }
    /// <summary>Gets whether loaded application content is protected from replacement.</summary>
    public bool IsContentImmutable { get => GetValue(IsContentImmutableProperty); private set => SetValue(IsContentImmutableProperty, value); }
    /// <summary>Gets whether Dock is dragging this native window.</summary>
    public bool IsDragging => _dragging;
    /// <summary>Gets whether the native window is maximized.</summary>
    public bool IsMaximized => _maximized;
    /// <summary>Gets whether closing has not been requested by internal layout cleanup.</summary>
    /// <remarks>Hiding and canceled user close requests do not change this value.
    /// An internal close request keeps this value false, including if that request is canceled.</remarks>
    protected bool CloseInitiatedByUser => !_internalCloseRequested;
    internal bool HasClosed { get; private set; }
    internal bool DeferNativePresentation { get; set; }
    internal bool IsHiddenByLayout { get; private set; }

    void IHostWindow.Exit()
    {
        if (HasClosed) return;
        if (!_preserveCloseOrigin) _internalCloseRequested = true;
        base.Exit();
    }

    internal void ExitFromLayout(bool internalClose)
    {
        bool previous = _preserveCloseOrigin;
        _preserveCloseOrigin = !internalClose;
        try
        {
            if (Window is { } window && ReferenceEquals(window.Host, this)) window.Exit();
            else base.Exit();
        }
        finally { _preserveCloseOrigin = previous; }
    }

    internal void RequestCloseFromLayout(bool internalClose = true)
    {
        if (HasClosed || (internalClose && _internalCloseRequested)) return;
        if (internalClose) _internalCloseRequested = true;
        Dispatcher.UIThread.Post(() =>
        {
            if (!HasClosed) ExitFromLayout(internalClose);
        }, DispatcherPriority.Send);
    }

    private void FloatingModelChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (_floatingModel is LayoutAnchorableFloatingWindow tool)
        {
            if (args.PropertyName == nameof(LayoutAnchorableFloatingWindow.RootPanel) && tool.RootPanel is null)
                RequestCloseFromLayout();
            else if (args.PropertyName == nameof(LayoutAnchorableFloatingWindow.IsVisible))
            {
                if (tool.IsVisible) RestoreForLayout();
                else HideForLayout();
            }
        }
        else if (_floatingModel is LayoutGroupFloatingWindow group && args.PropertyName == nameof(LayoutGroupFloatingWindow.IsVisible))
        {
            if (group.IsVisible) RestoreForLayout();
            else HideForLayout();
        }
        else if (args.PropertyName == nameof(LayoutFloatingWindow.IsValid) && _floatingModel is { IsValid: false })
            RequestCloseFromLayout();
    }

    private void FloatingDocumentChanged(object? sender, EventArgs args)
    {
        if (_floatingModel is LayoutDocumentFloatingWindow { RootDocument: null }) RequestCloseFromLayout();
    }

    internal void HideForLayout()
    {
        // An explicitly created, unshown host remains under the caller's
        // control until its first Show. Only restore windows we have hidden.
        if (HasClosed || !IsVisible) return;
        IsHiddenByLayout = true;
        Window?.Save();
        Hide();
    }

    internal void RestoreForLayout()
    {
        if (HasClosed || !IsHiddenByLayout) return;
        IsHiddenByLayout = false;
        Window?.Present(Window.IsModal);
    }

    void IHostWindow.Present(bool isDialog)
    {
        bool hiddenLayout = Model is LayoutAnchorableFloatingWindow { IsVisible: false } or LayoutGroupFloatingWindow { IsVisible: false };
        if (DeferNativePresentation || hiddenLayout)
        {
            if (hiddenLayout && !DeferNativePresentation) IsHiddenByLayout = true;
            _notifyOpened = true;
            if (IsVisible)
            {
                // A collection-change handler may show the host before native
                // initialization assigns its factory and registers the window.
                if (Window?.Factory is { } factory && !factory.HostWindows.Contains(this))
                    factory.HostWindows.Add(this);
                NotifyOpened();
            }
        }
        else Present(isDialog);
    }

    /// <summary>Presents this window through Dock's native owner and window-option handling.</summary>
    public new void Present(bool isDialog)
    {
        _notifyOpened = false;
        base.Present(isDialog);
    }

    /// <summary>Dispatches Avalonia initialization to the AvalonDock extensibility hook.</summary>
    protected override void OnInitialized() => OnInitialized(EventArgs.Empty);

    /// <summary>Called once when Avalonia completes floating-window initialization.</summary>
    /// <remarks>Call the base hook to retain native initialization. The public Initialized event follows this hook.</remarks>
    protected virtual void OnInitialized(EventArgs e)
    {
        base.OnInitialized();
        ObserveLayoutModel();
    }

    // Avalonia may initialize in the Window base constructor. Remember the
    // request until our constructor receives the model, without invoking an
    // abstract Model getter while the derived host is still being constructed.
    private protected void ObserveLayoutModel(bool closeEmptyDocument = false)
    {
        _observeModelRequested = true;
        _closeEmptyDocument |= closeEmptyDocument;
        if (_floatingModel is null || HasClosed) return;
        if (_closeEmptyDocument && _floatingModel is LayoutDocumentFloatingWindow { RootDocument: null })
        {
            RequestCloseFromLayout();
            return;
        }
        if (_observingModel || HasClosed) return;
        _observingModel = true;
        if (_floatingModel is LayoutDocumentFloatingWindow document)
            document.RootDocumentChanged += FloatingDocumentChanged;
        else if (_floatingModel is not null) _floatingModel.PropertyChanged += FloatingModelChanged;
    }

    /// <summary>Filters native Windows messages before Avalonia processes them.</summary>
    /// <remarks>Invoked through Avalonia's Win32 callback after the first opening until the window closes.
    /// Set handled to true and return the native result to consume a message. The base implementation
    /// leaves messages unhandled; typed hosts add caption activation and menu routing.
    /// Avalonia and Dock retain movement, resizing, and execution of native window commands.
    /// Other platforms do not deliver Win32 messages.</remarks>
    protected virtual IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) => IntPtr.Zero;

    private IntPtr DispatchWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled) =>
        FilterMessage(hwnd, unchecked((int)message), wParam, lParam, ref handled);

    /// <summary>Shows the native Windows system menu at a physical screen position.</summary>
    /// <remarks>Called for an enabled caption fallback when no custom layout-item menu owns the request.
    /// Return true when the request is consumed, including a dismissed menu. The default implementation
    /// uses the owning manager's native menu service and returns false for a non-Windows or non-native host.
    /// Native commands continue through the host's normal state and closing lifecycle.</remarks>
    protected virtual bool ShowSystemMenu(PixelPoint position) =>
        Model.Root?.Manager?.TryShowSystemMenu(this, position, CanClose() || CanHide()) == true;

    internal bool TryShowSystemMenu(PixelPoint position) => ShowSystemMenu(position);

    /// <summary>Dispatches a native close request to the AvalonDock cancellation hook.</summary>
    protected override void OnClosing(WindowClosingEventArgs e) => OnClosing((CancelEventArgs)e);

    /// <summary>Routes a derived Avalonia closing override's base call to the AvalonDock cancellation hook.</summary>
    /// <typeparam name="TEventArgs">The native window-closing argument type.</typeparam>
    /// <param name="e">The native arguments supplied by Avalonia.</param>
    /// <remarks>This overload preserves native argument overload resolution across multiple inheritance
    /// levels. Application policy belongs in the virtual CancelEventArgs overload.</remarks>
    protected void OnClosing<TEventArgs>(TEventArgs e) where TEventArgs : WindowClosingEventArgs =>
        OnClosing((CancelEventArgs)e);

    /// <summary>Processes a native close request using AvalonDock's framework-independent cancellation signature.</summary>
    /// <remarks>The native lifecycle supplies a WindowClosingEventArgs instance. Calling the base hook
    /// checks close/hide capability before raising Avalonia's Closing event. A nonclosable tool
    /// may hide before that event; content Hiding can cancel the hide. An eligible host closes
    /// without closing its content. Overrides using Avalonia's overload remain supported.</remarks>
    protected virtual void OnClosing(CancelEventArgs e)
    {
        // Internal removal and view unloading bypass user capabilities.
        if (CloseInitiatedByUser && !_preserveCloseOrigin && !CanClose())
        {
            if (CanHide()) DoHide();
            e.Cancel = true;
        }
        if (e is WindowClosingEventArgs nativeArgs) base.OnClosing(nativeArgs);
    }

    /// <summary>Notifies the native factory when an explicitly created window is first shown.</summary>
    protected override void OnOpened(EventArgs e)
    {
        // Initialization may run while a base constructor installs its styles.
        // Native callbacks must wait until derived hosts have their model state.
        if (!_messageHookAttached)
        {
            Win32Properties.AddWndProcHookCallback(this, DispatchWindowMessage);
            _messageHookAttached = true;
        }
        Model.Root?.Manager?.AttachThemeResources(this);
        NotifyOpened();
        base.OnOpened(e);
        if (Model is LayoutAnchorableFloatingWindow { IsVisible: false } or LayoutGroupFloatingWindow { IsVisible: false }) HideForLayout();
    }

    private void NotifyOpened()
    {
        if (!_notifyOpened || Window?.Factory is not { } factory) return;
        _notifyOpened = false;
        factory.OnWindowOpened(Window);
    }

    /// <summary>Updates the drag notification state used by derived window controls.</summary>
    protected void SetIsDragging(bool value) => SetAndRaise(IsDraggingProperty, ref _dragging, value);
    /// <summary>Called when the native drag state changes. Dock owns pointer capture and drag execution.</summary>
    protected virtual void OnIsDraggingChanged(AvaloniaPropertyChangedEventArgs e) { }

    /// <summary>Synchronizes the public maximized state after the native window state changes.</summary>
    protected virtual void OnStateChanged(EventArgs e) =>
        SetAndRaise(IsMaximizedProperty, ref _maximized, WindowState == WindowState.Maximized);

    /// <summary>Checks whether the contents' current close commands permit closing this window.</summary>
    protected virtual bool CanClose(object? parameter = null) => false;
    /// <summary>Checks whether the contents' current hide commands permit hiding this window.</summary>
    protected virtual bool CanHide(object? parameter = null) => false;
    /// <summary>Hides the window's contents when a user close request cannot close them.</summary>
    protected virtual void DoHide() { }

    /// <summary>Opens the owning manager's navigator on Ctrl+Tab from floating content.</summary>
    protected virtual void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Tab && (e.KeyModifiers & KeyModifiers.Control) != 0 &&
            Model.Root?.Manager is { IsNavigatorWindowActive: false } manager)
        {
            manager.ShowNavigatorWindow();
            e.Handled = true;
        }
    }

    /// <summary>Moves the native window with arrow keys when enabled by its manager.</summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (Model.Root?.Manager is { AllowMovingFloatingWindowWithKeyboard: true })
        {
            int step = (int)Math.Round(25 * DesktopScaling);
            PixelPoint position = Position;
            switch (e.Key)
            {
                case Key.Left: Position = new(position.X - step, position.Y); break;
                case Key.Right: Position = new(position.X + step, position.Y); break;
                case Key.Up: Position = new(position.X, position.Y - step); break;
                case Key.Down: Position = new(position.X, position.Y + step); break;
            }
        }
        base.OnKeyDown(e);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty)
            OnStateChanged(EventArgs.Empty);
        else if (change.Property == IsDraggingProperty)
            OnIsDraggingChanged(change);
        else if (change.Property == ContentProperty)
            PseudoClasses.Set(":custom-content", change.NewValue is not IRootDock);
    }

    /// <summary>Removes a closed native host from its manager before native layout cleanup.</summary>
    protected override void OnClosed(EventArgs e)
    {
        ILayoutRoot? root = Model.Root;
        HasClosed = true;
        _observingModel = false;
        if (_floatingModel is LayoutDocumentFloatingWindow documentModel)
            documentModel.RootDocumentChanged -= FloatingDocumentChanged;
        else if (_floatingModel is not null) _floatingModel.PropertyChanged -= FloatingModelChanged;
        ((INotifyCollectionChanged)Classes).CollectionChanged -= NativeDragChanged;
        try
        {
            if (_messageHookAttached)
            {
                Win32Properties.RemoveWndProcHookCallback(this, DispatchWindowMessage);
                _messageHookAttached = false;
            }
            SetIsDragging(false);
            root?.Manager?.RemoveFloatingWindow(this);
        }
        finally
        {
            try { base.OnClosed(e); }
            finally
            {
                if (_internalCloseRequested && Model is LayoutFloatingWindow model && ReferenceEquals(model.Root, root) &&
                    (model.DockWindow.Host is null || ReferenceEquals(model.DockWindow.Host, this)))
                {
                    if (root is not null) root.FloatingWindows.Remove(model);
                    else if (model.DockWindow.Host is null && model.DockWindow.Owner is IRootDock owner &&
                        owner.Windows?.Contains(model.DockWindow) != true)
                        model.DockWindow.Factory?.RemoveWindow(model.DockWindow);
                }
                root?.CollectGarbage();
            }
        }
    }

    private void NativeDragChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        SetIsDragging(Classes.Contains(":dragging"));
}
