using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Controls;

namespace AvalonDock.Infrastructure;

// The construction boundary for public grids. Model-bearing constructors stay
// internal, as upstream; all native layout visuals come from a compiled template.
internal sealed class LayoutGridPresenter : ContentControl
{
    private IDisposable? _rootBinding;
    private ILayoutContainer? _model;

    public LayoutGridPresenter()
    {
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;
    }

    protected override Type StyleKeyOverride => typeof(ContentControl);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataContextProperty) SetModel();
        if (change.Property == ContentProperty && change.OldValue is ILayoutGridView previous)
            previous.ReleaseChildren();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetModel();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _rootBinding?.Dispose();
        _rootBinding = null;
        _model = null;
        SetCurrentValue(ContentProperty, null);
        Presenter?.UpdateChild();
        base.OnDetachedFromVisualTree(e);
    }

    private void SetModel()
    {
        ILayoutContainer? model = DataContext is IProportionalDock { Factory: LayoutFactory factory } native
            ? factory.GetLayoutContainer(native) : null;
        if (ReferenceEquals(_model, model) && (_rootBinding is not null || model?.Parent is not LayoutRoot)) return;
        _rootBinding?.Dispose();
        _rootBinding = null;
        _model = model;
        if (model is LayoutPanel panel && panel.Root?.Manager is { } manager && ReferenceEquals(manager.Layout.RootPanel, panel))
        {
            manager.EnsureLayoutRootPanel();
            _rootBinding = new RootPanelBinding(manager, this);
            return;
        }
        SetCurrentValue(ContentProperty, model switch
        {
            LayoutPanel panelModel => new LayoutPanelControl(panelModel),
            LayoutDocumentPaneGroup group => new LayoutDocumentPaneGroupControl(group),
            LayoutAnchorablePaneGroup group => new LayoutAnchorablePaneGroupControl(group),
            _ => null
        });
    }

    private sealed class RootPanelBinding : IDisposable
    {
        private readonly WeakReference<DockingManager> _manager;
        private readonly WeakReference<LayoutGridPresenter> _owner;

        internal RootPanelBinding(DockingManager manager, LayoutGridPresenter owner)
        {
            _manager = new(manager);
            _owner = new(owner);
            manager.PropertyChanged += Changed;
            Update(manager, owner);
        }

        private static void Update(DockingManager manager, LayoutGridPresenter owner) =>
            owner.SetCurrentValue(ContentProperty, ReferenceEquals(owner._model, manager.Layout.RootPanel) ? manager.LayoutRootPanel : null);

        private void Changed(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutGridPresenter? owner)) { Dispose(); return; }
            if (args.Property == DockingManager.LayoutRootPanelProperty && sender is DockingManager manager) Update(manager, owner);
        }

        public void Dispose()
        {
            if (_manager.TryGetTarget(out DockingManager? manager)) manager.PropertyChanged -= Changed;
        }
    }
}
