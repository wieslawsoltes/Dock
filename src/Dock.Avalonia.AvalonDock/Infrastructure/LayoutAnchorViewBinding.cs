using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Follows the model's current manager rather than capturing the manager which
// first created a side, group, or anchor control.
internal sealed class LayoutAnchorViewBinding(TemplatedControl control, LayoutElement model,
    StyledProperty<IControlTemplate?> templateProperty, Action<string?> changed)
{
    private Subscription? _subscription;

    internal void Initialize()
    {
        _subscription ??= new(model, this);
        Refresh();
    }

    private void Refresh()
    {
        DockingManager? manager = model.Root?.Manager;
        _subscription?.WatchManager(manager);
        control.SetCurrentValue(TemplatedControl.TemplateProperty, manager?.GetValue(templateProperty));
        changed(null);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly WeakReference<LayoutElement> _model;
        private readonly WeakReference<LayoutAnchorViewBinding> _owner;
        private WeakReference<DockingManager>? _manager;
        internal Subscription(LayoutElement model, LayoutAnchorViewBinding owner)
        {
            _model = new(model); _owner = new(owner);
            model.PropertyChanged += ModelChanged;
        }
        private void ModelChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutAnchorViewBinding? owner)) { Dispose(); return; }
            if (args.PropertyName is nameof(ILayoutElement.Root) or nameof(ILayoutElement.Parent)) owner.Refresh();
            else owner.Notify(args.PropertyName);
        }
        internal void WatchManager(DockingManager? manager)
        {
            DockingManager? previous = _manager?.TryGetTarget(out DockingManager? value) == true ? value : null;
            if (ReferenceEquals(manager, previous)) return;
            if (previous is not null) previous.PropertyChanged -= ManagerChanged;
            _manager = manager is null ? null : new(manager);
            if (manager is not null) manager.PropertyChanged += ManagerChanged;
        }
        private void ManagerChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutAnchorViewBinding? owner)) { Dispose(); return; }
            if (args.Property == owner.TemplateProperty || args.Property == DockingManager.LayoutProperty) owner.Refresh();
        }
        public void Dispose()
        {
            if (_model.TryGetTarget(out LayoutElement? model)) model.PropertyChanged -= ModelChanged;
            WatchManager(null);
        }
    }

    private StyledProperty<IControlTemplate?> TemplateProperty => templateProperty;
    private void Notify(string? property) => changed(property);
}
