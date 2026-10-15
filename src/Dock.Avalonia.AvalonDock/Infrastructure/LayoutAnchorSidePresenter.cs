using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;

namespace AvalonDock.Infrastructure;

// Replaces a primary root's flat pinned strip with the public side/group tree.
// Group strips and unrepresented native roots keep their compiled ItemsPresenter.
internal sealed class LayoutAnchorSidePresenter : ContentControl
{
    public static readonly StyledProperty<Control?> BodyProperty =
        AvaloniaProperty.Register<LayoutAnchorSidePresenter, Control?>(nameof(Body));
    private ToolPinnedControl? _strip;
    private ManagerBinding? _binding;

    public LayoutAnchorSidePresenter()
    {
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;
    }
    public Control? Body { get => GetValue(BodyProperty); set => SetValue(BodyProperty, value); }
    protected override Type StyleKeyOverride => typeof(ContentControl);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BodyProperty && _binding is null) SetCurrentValue(ContentProperty, Body);
        if (change.Property == DataContextProperty && _strip is not null) Refresh();
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _strip = TemplatedParent as ToolPinnedControl;
        if (_strip is not null) _strip.PropertyChanged += StripChanged;
        Refresh();
    }
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_strip is not null) _strip.PropertyChanged -= StripChanged;
        _strip = null;
        _binding?.Dispose(); _binding = null;
        // Public sides are reusable controls and must leave the old presenter.
        // Keep a fallback ItemsPresenter in its template tree so Avalonia can
        // retire its container generator when that native template is replaced.
        if (Content is Controls.LayoutAnchorSideControl)
        {
            SetCurrentValue(ContentProperty, null);
            Presenter?.UpdateChild();
        }
        base.OnDetachedFromVisualTree(e);
    }
    private void StripChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ItemsControl.ItemsSourceProperty) Refresh();
    }
    private void Refresh()
    {
        _binding?.Dispose(); _binding = null;
        if (DataContext is IRootDock { Factory: LayoutFactory factory } native
            && factory.GetLayoutContainer(native) is LayoutRoot { Manager: { } manager } root
            && ReferenceEquals(manager.Layout, root) && _strip?.ItemsSource is { } items)
        {
            var property = ReferenceEquals(items, native.LeftPinnedDockables) ? DockingManager.LeftSidePanelProperty
                : ReferenceEquals(items, native.TopPinnedDockables) ? DockingManager.TopSidePanelProperty
                : ReferenceEquals(items, native.RightPinnedDockables) ? DockingManager.RightSidePanelProperty
                : ReferenceEquals(items, native.BottomPinnedDockables) ? DockingManager.BottomSidePanelProperty : null;
            if (property is not null)
            {
                manager.EnsureAnchorSidePanels();
                _binding = new(manager, this, property);
                return;
            }
        }
        SetCurrentValue(ContentProperty, Body);
    }

    private sealed class ManagerBinding : IDisposable
    {
        private readonly WeakReference<DockingManager> _manager;
        private readonly WeakReference<LayoutAnchorSidePresenter> _owner;
        private readonly StyledProperty<Controls.LayoutAnchorSideControl?> _property;
        internal ManagerBinding(DockingManager manager, LayoutAnchorSidePresenter owner, StyledProperty<Controls.LayoutAnchorSideControl?> property)
        {
            _manager = new(manager); _owner = new(owner); _property = property;
            manager.PropertyChanged += Changed;
            owner.SetCurrentValue(ContentProperty, manager.GetValue(property));
        }
        private void Changed(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (!_owner.TryGetTarget(out LayoutAnchorSidePresenter? owner)) { Dispose(); return; }
            if (e.Property == _property && sender is DockingManager manager) owner.SetCurrentValue(ContentProperty, manager.GetValue(_property));
        }
        public void Dispose()
        {
            if (_manager.TryGetTarget(out DockingManager? manager)) manager.PropertyChanged -= Changed;
        }
    }
}
