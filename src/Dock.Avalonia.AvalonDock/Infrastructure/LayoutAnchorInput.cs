using System;
using Avalonia.Controls;
using Avalonia.Threading;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Controls;

namespace AvalonDock.Infrastructure;

// Owns the anchor's preview gesture; layout and content still belong to Dock.
internal sealed class LayoutAnchorInput(LayoutAnchorControl control, LayoutAnchorable model)
{
    private IDisposable? _hover;
    private bool _showing;

    internal void Enter()
    {
        Cancel();
        WeakReference<LayoutAnchorInput> owner = new(this);
        _hover = DispatcherTimer.RunOnce(() =>
        {
            if (owner.TryGetTarget(out LayoutAnchorInput? input))
            {
                input._hover = null;
                input.Show(activate: false);
            }
        }, TimeSpan.FromMilliseconds(400));
    }

    internal void Cancel() { _hover?.Dispose(); _hover = null; }

    internal void ModelChanged(string? property)
    {
        if (!model.IsAutoHidden || !model.IsEnabled || model.DockModel.Factory?.IsDockablePinned(model.DockModel) != true)
        {
            Cancel(); return;
        }
        if (property == nameof(LayoutContent.IsActive) && model.IsActive) Show(activate: true);
        if (property == nameof(LayoutContent.IsSelected) && model.IsSelected && !_showing)
        {
            Show(activate: false);
            model.IsSelected = false;
        }
    }

    internal void Show(bool activate)
    {
        Cancel();
        if (_showing || !model.IsAutoHidden || !model.IsEnabled || !control.IsEffectivelyEnabled
            || TopLevel.GetTopLevel(control) is null || model.Root?.Manager is not { } manager
            || model.DockModel.Factory is not LayoutFactory factory || !factory.IsDockablePinned(model.DockModel)) return;
        _showing = true;
        try
        {
            IRootDock? root = factory.FindRoot(model.DockModel, static _ => true);
            if (root?.PinnedDock?.ActiveDockable != model.DockModel) factory.PreviewAnchor(model.DockModel, activate);
            if (activate && ReferenceEquals(model.Root?.Manager, manager) && model.IsAutoHidden) model.IsActive = true;
        }
        finally { _showing = false; }
    }
}
