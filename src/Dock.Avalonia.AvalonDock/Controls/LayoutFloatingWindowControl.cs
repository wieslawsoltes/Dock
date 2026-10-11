using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Layout;
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
    /// <summary>Defines the current native window drag state.</summary>
    public static readonly DirectProperty<LayoutFloatingWindowControl, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<LayoutFloatingWindowControl, bool>(nameof(IsDragging), window => window.IsDragging);
    /// <summary>Defines whether the native window is maximized.</summary>
    public static readonly DirectProperty<LayoutFloatingWindowControl, bool> IsMaximizedProperty =
        AvaloniaProperty.RegisterDirect<LayoutFloatingWindowControl, bool>(nameof(IsMaximized), window => window.IsMaximized);

    private bool _dragging;
    private bool _maximized;
    private bool _notifyOpened;

    static LayoutFloatingWindowControl()
    {
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
        ((INotifyCollectionChanged)Classes).CollectionChanged += NativeDragChanged;
    }

    /// <summary>Gets the public floating model presented by this native window.</summary>
    public abstract ILayoutElement Model { get; }
    /// <summary>Gets whether loaded application content is protected from replacement.</summary>
    public bool IsContentImmutable { get => GetValue(IsContentImmutableProperty); private set => SetValue(IsContentImmutableProperty, value); }
    /// <summary>Gets whether Dock is dragging this native window.</summary>
    public bool IsDragging => _dragging;
    /// <summary>Gets whether the native window is maximized.</summary>
    public bool IsMaximized => _maximized;
    internal bool HasClosed { get; private set; }
    internal bool DeferNativePresentation { get; set; }

    void IHostWindow.Present(bool isDialog)
    {
        if (DeferNativePresentation)
        {
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
        NotifyOpened();
        base.OnOpened(e);
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
        HasClosed = true;
        ((INotifyCollectionChanged)Classes).CollectionChanged -= NativeDragChanged;
        try
        {
            SetIsDragging(false);
            Model.Root?.Manager?.RemoveFloatingWindow(this);
        }
        finally { base.OnClosed(e); }
    }

    private void NativeDragChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        SetIsDragging(Classes.Contains(":dragging"));
}
