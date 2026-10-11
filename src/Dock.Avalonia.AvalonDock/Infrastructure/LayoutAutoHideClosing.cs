using System;
using Avalonia.Threading;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// A weak timer observes the public preview; native Dock owns closing and layout.
internal sealed class LayoutAutoHideClosing(DockingManager manager)
{
    private DispatcherTimer? _timer;
    private LayoutAnchorable? _model;
    private bool _closing;
    internal bool IsResizing { get; set; }

    internal void SetModel(LayoutAnchorable? model)
    {
        if (ReferenceEquals(model, _model)) return;
        Stop();
        _model = model;
        if (model is null) return;
        if (_timer is null)
        {
            WeakReference<LayoutAutoHideClosing> owner = new(this);
            _timer = new DispatcherTimer(DispatcherPriority.Background);
            _timer.Tick += (sender, _) =>
            {
                if (owner.TryGetTarget(out LayoutAutoHideClosing? closing)) closing.Tick();
                else ((DispatcherTimer)sender!).Stop();
            };
        }
        _timer.Interval = TimeSpan.FromMilliseconds(50);
        _timer.Start();
    }

    internal void ResetDelay()
    {
        if (_closing && _timer is not null)
            _timer.Interval = TimeSpan.FromMilliseconds(manager.AutoHideWindowClosingTimer);
    }
    internal void Stop()
    {
        _timer?.Stop();
        _model = null; _closing = false; IsResizing = false;
    }

    private void Tick()
    {
        if (_model is not { } model || !ReferenceEquals(manager.AutoHideWindow?.Model, model)) { Stop(); return; }
        bool keepOpen = model.IsActive || IsResizing || model.DockModel.KeepPinnedDockableVisible ||
            manager.AutoHideWindow is { IsPointerOver: true } || IsAnchorOver(manager.LeftSidePanel, model) ||
            IsAnchorOver(manager.TopSidePanel, model) || IsAnchorOver(manager.RightSidePanel, model) || IsAnchorOver(manager.BottomSidePanel, model);
        if (_closing && !keepOpen)
        {
            Stop();
            manager.DockFactory.HidePreviewingDockables(manager.DockLayout);
            return;
        }
        // A preview is monitored until idle, then checked once at the close deadline.
        // Reentering at that deadline returns to monitoring before a new idle delay.
        _closing = !keepOpen;
        _timer!.Interval = TimeSpan.FromMilliseconds(_closing ? manager.AutoHideWindowClosingTimer : 50);
    }

    private static bool IsAnchorOver(LayoutAnchorSideControl? side, LayoutAnchorable model)
    {
        if (side is null) return false;
        foreach (LayoutAnchorGroupControl group in side.Children)
            foreach (LayoutAnchorControl anchor in group.Children)
                if (ReferenceEquals(anchor.Model, model) && anchor.IsPointerOver && anchor.IsEffectivelyVisible) return true;
        return false;
    }
}
