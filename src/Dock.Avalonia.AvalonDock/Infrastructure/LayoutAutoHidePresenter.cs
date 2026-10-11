using System;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Controls;

namespace AvalonDock.Infrastructure;

// Adds the public preview surface around the unchanged native tool template.
// Ordinary panes retain that template directly.
internal sealed class LayoutAutoHidePresenter : ContentControl
{
    public static readonly StyledProperty<Control?> BodyProperty =
        AvaloniaProperty.Register<LayoutAutoHidePresenter, Control?>(nameof(Body));
    private ManagerBinding? _binding;
    private bool _attached;

    public LayoutAutoHidePresenter()
    {
        HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
    }

    public Control? Body { get => GetValue(BodyProperty); set => SetValue(BodyProperty, value); }
    protected override Type StyleKeyOverride => typeof(ContentControl);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BodyProperty)
        {
            if (Content is LayoutAutoHideWindowControl window) window.SetContent(Body);
            else SetCurrentValue(ContentProperty, Body);
        }
        if (change.Property == DataContextProperty && _attached) Refresh();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        Refresh();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        _binding?.Dispose(); _binding = null;
        if (Content is LayoutAutoHideWindowControl) Present(null);
        base.OnDetachedFromVisualTree(e);
    }

    private void Refresh()
    {
        _binding?.Dispose(); _binding = null;
        if (DataContext is IToolDock { Factory: LayoutFactory factory } native
            && factory.FindRoot(native, static _ => true) is { } root && ReferenceEquals(root.PinnedDock, native)
            && factory.GetLayoutContainer(root) is LayoutRoot { Manager: { } manager } layout && ReferenceEquals(manager.Layout, layout))
        {
            _binding = new(manager, this);
            return;
        }
        if (Content is LayoutAutoHideWindowControl) Present(null);
        SetCurrentValue(ContentProperty, Body);
    }

    private void Present(LayoutAutoHideWindowControl? window)
    {
        if (ReferenceEquals(Content, window)) { window?.SetContent(Body); return; }
        if (Content is LayoutAutoHideWindowControl previous) previous.SetContent(null);
        SetCurrentValue(ContentProperty, null);
        Presenter?.UpdateChild();
        if (window?.Parent is LayoutAutoHidePresenter previousHost && !ReferenceEquals(previousHost, this)) previousHost.Present(null);
        SetCurrentValue(ContentProperty, window);
        window?.SetContent(Body);
    }

    private sealed class ManagerBinding : IDisposable
    {
        private readonly WeakReference<DockingManager> _manager;
        private readonly WeakReference<LayoutAutoHidePresenter> _owner;
        internal ManagerBinding(DockingManager manager, LayoutAutoHidePresenter owner)
        {
            _manager = new(manager); _owner = new(owner);
            manager.PropertyChanged += Changed;
            owner.Present(manager.AutoHideWindow);
        }
        private void Changed(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutAutoHidePresenter? owner)) { Dispose(); return; }
            if (args.Property == DockingManager.AutoHideWindowProperty && sender is DockingManager manager) owner.Present(manager.AutoHideWindow);
        }
        public void Dispose()
        {
            if (_manager.TryGetTarget(out DockingManager? manager)) manager.PropertyChanged -= Changed;
        }
    }
}
