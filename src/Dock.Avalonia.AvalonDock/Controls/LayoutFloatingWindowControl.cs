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
/// <remarks>Floating hosts are excluded from the taskbar by default. Application styles
/// and local values can override <see cref="Window.ShowInTaskbar"/>.</remarks>
public abstract class LayoutFloatingWindowControl : HostWindow, ILayoutControl, IHostWindow
{
    /// <summary>Defines whether application content can be replaced after loading.</summary>
    public static readonly StyledProperty<bool> IsContentImmutableProperty =
        AvaloniaProperty.Register<LayoutFloatingWindowControl, bool>(nameof(IsContentImmutable));
    /// <summary>Defines the thickness of the floating window's resize hit areas.</summary>
    public static readonly StyledProperty<Thickness> ResizeBorderThicknessProperty =
        AvaloniaProperty.Register<LayoutFloatingWindowControl, Thickness>(name: nameof(ResizeBorderThickness), metadata: AvalonDockProperty.StyledMetadata<Thickness>(defaultValue: new Thickness(10)));
    /// <summary>Defines the current native window drag state.</summary>
    public static readonly DirectProperty<LayoutFloatingWindowControl, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<LayoutFloatingWindowControl, bool>(nameof(IsDragging), window => window.IsDragging);
    /// <summary>Defines the maximized-state flag used by floating-window bindings and styles.</summary>
    public static readonly StyledProperty<bool> IsMaximizedProperty =
        AvaloniaProperty.Register<LayoutFloatingWindowControl, bool>(nameof(IsMaximized));

    private bool _dragging;
    private bool _nativeDragNotification;
    private bool _nativeDragging;
    private bool _notifyOpened;
    private bool _internalCloseRequested;
    private bool _preserveCloseOrigin;
    private bool _messageHookAttached;
    private bool _observingModel;
    private bool _observeModelRequested;
    private bool _closeEmptyDocument;
    private bool _cleanupModelInDerivedHook;
    private ILayoutRoot? _closingRoot;
    private readonly LayoutFloatingWindow? _floatingModel;

    static LayoutFloatingWindowControl()
    {
        ShowInTaskbarProperty.OverrideDefaultValue<LayoutFloatingWindowControl>(false);
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
    /// <summary>Gets the maximized-state flag synchronized by native window state changes.</summary>
    /// <remarks>The property supports bindings and styles. Setting its value does not change WindowState.</remarks>
    public bool IsMaximized => GetValue(IsMaximizedProperty);
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
        if (internalClose && _internalCloseRequested) return;
        if (internalClose) _internalCloseRequested = true;
        if (HasClosed) return;
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
            if (hiddenLayout && !DeferNativePresentation)
            {
                IsHiddenByLayout = true;
                // Hidden restored hosts have no visual attachment to complete
                // deferred initialization. Observe the model before waiting
                // for its contents to become visible again.
                InitializeIfNeeded();
            }
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

    /// <summary>Defers the native initialization hook until the derived host exposes its model.</summary>
    /// <remarks>Show, EndInit, or visual attachment retries initialization after construction.</remarks>
    protected override bool IsInitializationDeferred => Model is null;

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
    private protected void ObserveLayoutModel(bool closeEmptyDocument = false, bool cleanupInDerivedHook = false)
    {
        _observeModelRequested = true;
        _closeEmptyDocument |= closeEmptyDocument;
        _cleanupModelInDerivedHook |= cleanupInDerivedHook;
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

    private protected void StopObservingLayoutModel()
    {
        _observingModel = false;
        if (_floatingModel is LayoutDocumentFloatingWindow document)
            document.RootDocumentChanged -= FloatingDocumentChanged;
        else if (_floatingModel is not null) _floatingModel.PropertyChanged -= FloatingModelChanged;
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
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // WPF closes owned windows without asking each child's Closing policy.
        // Avalonia routes the owner's request through children first. Preserve
        // the owner's own veto and let OnClosed release these native hosts.
        if (e.CloseReason == WindowCloseReason.OwnerWindowClosing)
        {
            e.Cancel = false;
            return;
        }
        OnClosing((CancelEventArgs)e);
    }

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
            e.Cancel = true;
            if (CanHide()) DoHide();
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
    /// <summary>Captures or releases the primary mouse when a derived host changes its drag state.</summary>
    /// <remarks>Notifications from Dock's active drag retain Dock's existing pointer capture.</remarks>
    protected virtual void OnIsDraggingChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_nativeDragNotification) return;
        if ((bool)e.NewValue!) CaptureMouse();
        else ReleaseMouseCapture();
    }

    /// <summary>Synchronizes the public maximized state after the native window state changes.</summary>
    protected virtual void OnStateChanged(EventArgs e) =>
        SetCurrentValue(IsMaximizedProperty, WindowState == WindowState.Maximized);

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

    /// <summary>Completes closure using the captured root, then requests focus on the owner or current manager's first focusable descendant.</summary>
    protected override void OnClosed(EventArgs e)
    {
        ILayoutRoot? root = Model?.Root;
        ILayoutRoot? previousRoot = _closingRoot;
        _closingRoot = root;
        HasClosed = true;
        if (!_cleanupModelInDerivedHook) StopObservingLayoutModel();
        ((INotifyCollectionChanged)Classes).CollectionChanged -= NativeDragChanged;
        try
        {
            base.OnClosed(e);
            if (_internalCloseRequested && Model is LayoutFloatingWindow model && ReferenceEquals(model.Root, root) &&
                (model.DockWindow.Host is null || ReferenceEquals(model.DockWindow.Host, this)))
            {
                if (root is not null) root.FloatingWindows.Remove(model);
                else if (model.DockWindow.Host is null && model.DockWindow.Owner is IRootDock owner &&
                    owner.Windows?.Contains(model.DockWindow) != true)
                    model.DockWindow.Factory?.RemoveWindow(model.DockWindow);
            }
            RestoreOwnerOrManagerFocus();
        }
        finally { _closingRoot = previousRoot; }
    }

    private protected void RestoreOwnerOrManagerFocus()
    {
        InputElement? target = Owner;
        if (target is null && Model?.Root?.Manager is { } manager)
        {
            foreach (InputElement element in manager.FindVisualChildren<InputElement>())
            {
                if (!element.Focusable) continue;
                target = element;
                break;
            }
        }
        // Upstream attempts one focusable target, even when it is disabled or
        // hidden. Failure does not select a different descendant.
        target?.Focus();
    }

    /// <summary>Removes the host and collects its captured layout before raising Closed.</summary>
    protected override void OnClosedCore(EventArgs e, IDockWindow? closingWindow)
    {
        // Dock has captured its factory and marked the host closed before this
        // phase, so collecting an empty floating model retains native closure
        // notification even when collection clears that model's factory.
        SetIsDragging(false);
        _closingRoot?.Manager?.RemoveFloatingWindow(this);
        _closingRoot?.CollectGarbage();
        if (_messageHookAttached)
        {
            Win32Properties.RemoveWndProcHookCallback(this, DispatchWindowMessage);
            _messageHookAttached = false;
        }
        base.OnClosedCore(e, closingWindow);
    }

    private void NativeDragChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool dragging = Classes.Contains(":dragging");
        if (_nativeDragging == dragging) return;
        _nativeDragging = dragging;
        _nativeDragNotification = true;
        try { SetIsDragging(dragging); }
        finally { _nativeDragNotification = false; }
    }
}
