using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Presenters;
using Avalonia.Platform;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using Dock.Avalonia.Contract;
using Dock.Avalonia.Controls;

namespace AvalonDock.Infrastructure;

// Moves the existing native Dock presenter into a real platform child without recreating its editor.
internal sealed class LayoutAutoHideNativeRoot(LayoutAutoHideWindowControl owner)
{
    private EmbeddableControlRoot? _root;
    private IPlatformHandle? _handle;
    private Decorator? _inline;
    private ContentPresenter? _presenter;

    internal bool IsHosted => _presenter is not null;
    internal Decorator? InlinePresenter { get; set; }

    internal HandleRef Build()
    {
        if (_handle is { } previous) Destroy(new HandleRef(owner, previous.Handle));
        Decorator inline = InlinePresenter ?? throw new InvalidOperationException("The native preview requires its inline presenter host.");
        ContentPresenter presenter = inline.Child as ContentPresenter
            ?? throw new InvalidOperationException("The native preview requires its existing content presenter.");
        EmbeddableControlRoot root = new EmbeddedRoot(owner);
        try
        {
            IPlatformHandle handle = root.TryGetPlatformHandle()
                ?? throw new NotSupportedException("This backend did not create a native embedded handle.");
            if (handle.Handle == IntPtr.Zero) throw new InvalidOperationException("The embedded preview handle is invalid.");
            inline.Child = null;
            ((ISetLogicalParent)presenter).SetParent(owner);
            root.Content = presenter;
            root.Prepare();
            root.StartRendering();
            _root = root;
            _handle = handle;
            _inline = inline;
            _presenter = presenter;
            owner.InvalidateMeasure();
            return new HandleRef(owner, handle.Handle);
        }
        catch
        {
            root.Content = null;
            root.Presenter?.UpdateChild();
            root.Dispose();
            RestoreInlinePresenter(inline, presenter);
            throw;
        }
    }

    internal void Destroy(HandleRef handle)
    {
        if (_root is not { } root || _handle?.Handle != handle.Handle) return;
        Decorator? inline = _inline;
        ContentPresenter? presenter = _presenter;
        _root = null;
        _handle = null;
        _inline = null;
        _presenter = null;
        root.Content = null;
        root.Presenter?.UpdateChild();
        root.Dispose();
        if (inline is not null && presenter is not null && ReferenceEquals(owner.Presenter, presenter))
            RestoreInlinePresenter(inline, presenter);
        owner.InvalidateMeasure();
    }

    internal void Arrange(Size size) => _presenter?.Arrange(new Rect(size));

    internal void ReturnPresenterToInline()
    {
        if (_root is not { } root || _inline is not { } inline || _presenter is not { } presenter) return;
        _inline = null;
        _presenter = null;
        root.Content = null;
        root.Presenter?.UpdateChild();
        RestoreInlinePresenter(inline, presenter);
    }

    private void RestoreInlinePresenter(Decorator inline, ContentPresenter presenter)
    {
        ((ISetLogicalParent)presenter).SetParent(null);
        inline.Child = presenter;
        ((ISetLogicalParent)presenter).SetParent(null);
        ((ISetLogicalParent)presenter).SetParent(owner);
    }

    private sealed class EmbeddedRoot : EmbeddableControlRoot, IExternalDockSurface
    {
        internal EmbeddedRoot(LayoutAutoHideWindowControl owner) : base(owner)
        {
            owner.FindAncestorOfType<DockControl>()?.RegisterExternalDockSurface(this);
        }

        public DockControl? DockControl { get; set; }
        public Control SurfaceControl => this;

        protected override void OnClosed(EventArgs e)
        {
            DockControl?.UnregisterExternalDockSurface(this);
            base.OnClosed(e);
        }
    }

    internal Size Measure(Size available)
    {
        if (_presenter is not { } presenter) return default;
        presenter.Measure(available);
        return presenter.DesiredSize;
    }
}
