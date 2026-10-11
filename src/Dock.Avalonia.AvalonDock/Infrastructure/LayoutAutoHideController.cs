using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;

namespace AvalonDock.Infrastructure;

// Observes the primary root's native preview and synchronizes its public view.
internal sealed class LayoutAutoHideController
{
    private readonly DockingManager _manager;
    private readonly LayoutAutoHideClosing _closing;
    private LayoutRoot? _layout;
    private Subscription? _subscription;

    internal LayoutAutoHideController(DockingManager manager)
    {
        _manager = manager;
        _closing = new(manager);
    }

    internal void Attach(LayoutRoot layout)
    {
        if (!ReferenceEquals(layout, _layout))
        {
            Detach(hide: false);
            _layout = layout;
            _subscription = new((RootDock)layout.DockModel, this);
            _manager.AddHandler(Thumb.DragStartedEvent, DragStarted, handledEventsToo: true);
            _manager.AddHandler(Thumb.DragCompletedEvent, DragCompleted, handledEventsToo: true);
        }
        Refresh();
    }

    internal void Detach(bool hide)
    {
        LayoutRoot? previous = _layout;
        _layout = null;
        _subscription?.Dispose(); _subscription = null;
        _closing.Stop();
        _manager.AutoHideWindow?.SetModel(null);
        _manager.RemoveHandler(Thumb.DragStartedEvent, DragStarted);
        _manager.RemoveHandler(Thumb.DragCompletedEvent, DragCompleted);
        if (hide && previous?.DockModel is RootDock root)
        {
            // Detachment must release the preview even for a tool held open by
            // the native keep-visible policy. Toggle closes an existing preview.
            if (root.PinnedDock?.ActiveDockable is { } active && root.PinnedDock.VisibleDockables?.Contains(active) == true)
                previous.DockFactory.TogglePreviewPinnedDockable(active);
            else previous.DockFactory.HidePreviewingDockables(root);
        }
    }

    internal void Refresh()
    {
        IToolDock? preview = (_layout?.DockModel as IRootDock)?.PinnedDock;
        _subscription?.WatchPreview(preview as AvaloniaObject);
        LayoutAnchorable? model = preview?.ActiveDockable is { } native && preview.VisibleDockables?.Contains(native) == true
            ? _layout?.Context.Get(native) as LayoutAnchorable : null;
        _manager.AutoHideWindow?.SetModel(model);
        _closing.SetModel(_manager.AutoHideWindow is null ? null : model);
    }

    internal void ResetDelay() => _closing.ResetDelay();

    private void DragStarted(object? sender, VectorEventArgs args)
    {
        if (args.Source is GridSplitter splitter && splitter.FindAncestorOfType<PinnedDockControl>() is { } preview
            && ReferenceEquals(preview.DataContext, _layout?.DockModel)) _closing.IsResizing = true;
    }
    private void DragCompleted(object? sender, VectorEventArgs args)
    {
        if (!_closing.IsResizing) return;
        _closing.IsResizing = false;
    }

    private sealed class Subscription : IDisposable
    {
        private readonly WeakReference<RootDock> _root;
        private readonly WeakReference<LayoutAutoHideController> _owner;
        private WeakReference<AvaloniaObject>? _preview;

        internal Subscription(RootDock root, LayoutAutoHideController owner)
        {
            _root = new(root); _owner = new(owner);
            root.PropertyChanged += RootChanged;
        }
        private void RootChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutAutoHideController? owner)) { Dispose(); return; }
            if (args.Property == RootDock.PinnedDockProperty) owner.Refresh();
        }
        internal void WatchPreview(AvaloniaObject? preview)
        {
            AvaloniaObject? previous = _preview?.TryGetTarget(out AvaloniaObject? value) == true ? value : null;
            if (ReferenceEquals(preview, previous)) return;
            if (previous is not null) previous.PropertyChanged -= PreviewChanged;
            _preview = preview is null ? null : new(preview);
            if (preview is not null) preview.PropertyChanged += PreviewChanged;
        }
        private void PreviewChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutAutoHideController? owner)) { Dispose(); return; }
            if (args.Property.Name is nameof(IToolDock.ActiveDockable) or nameof(IToolDock.VisibleDockables)) owner.Refresh();
        }
        public void Dispose()
        {
            if (_root.TryGetTarget(out RootDock? root)) root.PropertyChanged -= RootChanged;
            WatchPreview(null);
        }
    }
}
