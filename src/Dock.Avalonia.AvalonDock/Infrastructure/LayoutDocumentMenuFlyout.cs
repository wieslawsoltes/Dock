using System;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// ChildrenSorted returns a new snapshot after collection changes. Bind that
// source directly so an open native presenter follows snapshot replacement.
internal sealed class LayoutDocumentMenuFlyout : MenuFlyout
{
    public static readonly StyledProperty<LayoutDocumentPane?> PaneProperty =
        AvaloniaProperty.Register<LayoutDocumentMenuFlyout, LayoutDocumentPane?>(nameof(Pane));

    public LayoutDocumentPane? Pane { get => GetValue(PaneProperty); set => SetValue(PaneProperty, value); }
    private WeakReference<DockingManager>? _manager;

    protected override void OnOpened()
    {
        if (Pane?.Root?.Manager is { } manager)
        {
            _manager = new(manager);
            manager.LayoutChanging += LayoutChanging;
        }
        base.OnOpened();
    }

    protected override void OnClosed()
    {
        if (_manager?.TryGetTarget(out DockingManager? manager) == true) manager.LayoutChanging -= LayoutChanging;
        _manager = null;
        base.OnClosed();
    }

    private void LayoutChanging(object? sender, EventArgs args) => HideCore(false);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        // Native pane recycling can preserve the button across layout replacement.
        // Its open menu still belongs to the old pane and must close explicitly.
        if (change.Property == PaneProperty && IsOpen) Hide();
    }

    protected override Control CreatePresenter()
    {
        Control presenter = base.CreatePresenter();
        presenter.Bind(ItemsControl.ItemsSourceProperty, this.GetObservable(ItemsSourceProperty));
        return presenter;
    }
}
