using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        IsContentImmutable = isContentImmutable;
        Classes.Add("avalondock");
        if (model is LayoutDocumentFloatingWindow document)
        {
            Tag = document;
            Classes.Add("avalondock-document-window");
        }
        Styles.Add(new AvalonDockTheme());
        model.Root?.Manager?.AttachThemeResources(this);
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
        try { Window?.Exit(); }
        finally { _preserveCloseOrigin = previous; }
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
        bool hiddenLayout = Model is LayoutAnchorableFloatingWindow { IsVisible: false };
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

    /// <summary>Notifies the native factory when an explicitly created window is first shown.</summary>
    protected override void OnOpened(EventArgs e)
    {
        Model.Root?.Manager?.AttachThemeResources(this);
        NotifyOpened();
        base.OnOpened(e);
        if (Model is LayoutAnchorableFloatingWindow { IsVisible: false }) HideForLayout();
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

    /// <summary>Checks whether the contents' current close commands permit closing this window.</summary>
    protected virtual bool CanClose(object? parameter = null) => false;
    /// <summary>Checks whether the contents' current hide commands permit hiding this window.</summary>
    protected virtual bool CanHide(object? parameter = null) => false;
    /// <summary>Hides the window's contents when a user close request cannot close them.</summary>
    protected virtual void DoHide() { }

    internal void CloseOrHideContents()
    {
        if (CanClose() && Model is LayoutFloatingWindow model)
            LayoutFloatingWindowOperations.Execute(model, hide: false, parameter: null);
        else if (CanHide()) DoHide();
    }

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
            SetAndRaise(IsMaximizedProperty, ref _maximized, WindowState == WindowState.Maximized);
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
        ((INotifyCollectionChanged)Classes).CollectionChanged -= NativeDragChanged;
        try
        {
            SetIsDragging(false);
            root?.Manager?.RemoveFloatingWindow(this);
        }
        finally
        {
            try { base.OnClosed(e); }
            finally { root?.CollectGarbage(); }
        }
    }

    private void NativeDragChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        SetIsDragging(Classes.Contains(":dragging"));
}
