using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Platform;
using Avalonia.VisualTree;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

// NativeControlHost owns platform parenting, clipping, position, visibility and destruction.
internal sealed class LayoutAutoHideNativeHost : NativeControlHost
{
    public static readonly DirectProperty<LayoutAutoHideNativeHost, Decorator?> InlinePresenterProperty =
        AvaloniaProperty.RegisterDirect<LayoutAutoHideNativeHost, Decorator?>(nameof(InlinePresenter),
            host => host.InlinePresenter, (host, value) => host.InlinePresenter = value);
    private Decorator? _inlinePresenter;
    private LayoutAutoHideWindowControl? _owner;
    private bool _nativeAttached;
    private IControlTemplate? _template;

    public Decorator? InlinePresenter
    {
        get => _inlinePresenter;
        set => SetAndRaise(InlinePresenterProperty, ref _inlinePresenter, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // X11 and Headless have no embeddable top-level implementation. Their existing
        // in-tree presenter remains active; the HWND/NSView callbacks are not fabricated.
        string? descriptor = TopLevel.GetTopLevel(this)?.TryGetPlatformHandle()?.HandleDescriptor;
        if (descriptor is not ("HWND" or "NSWindow" or "NSView")) return;
        _owner = this.FindAncestorOfType<LayoutAutoHideWindowControl>();
        if (_owner is null) return;
        // A detached old template can briefly reattach before ApplyTemplate replaces
        // it. Keep its presenter inline so the normal template teardown can release it.
        if (_template is not null && !ReferenceEquals(_template, _owner.Template)) return;
        _template = _owner.Template;
        _nativeAttached = true;
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (!_nativeAttached) return;
        _nativeAttached = false;
        base.OnDetachedFromVisualTree(e);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        LayoutAutoHideWindowControl owner = _owner ?? throw new InvalidOperationException("The preview is detached.");
        HandleRef handle = owner.CreateNativeHost(new HandleRef(this, parent.Handle), InlinePresenter);
        if (handle.Handle == IntPtr.Zero) throw new InvalidOperationException("The preview returned an invalid child handle.");
        return new PlatformHandle(handle.Handle, parent.HandleDescriptor);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control) =>
        _owner?.DestroyNativeHost(new HandleRef(this, control.Handle));
}
