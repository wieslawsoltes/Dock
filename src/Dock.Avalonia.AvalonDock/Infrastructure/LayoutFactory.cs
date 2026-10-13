using System;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// AvalonDock closes compatible hosts independently of their layout contents.
// Application-provided plain Dock hosts retain the native close/hide adapter.
internal sealed class LayoutFactory(Func<IDockWindow, bool> closeWindow, ILayoutFloatingPlacement floatingPlacement, IDockDropPolicy dropPolicy,
    ILayoutOperationEvents operations, Func<IDockable, LayoutItem?> getLayoutItem,
    Func<IDockable, LayoutElement> getLayoutElement) : Factory, IDockOperationObserver, IDockWindowHostFactory, IDockWindowClosePolicy, IDockItemDragPolicy, IDockDragPreviewPolicy, IDockDropPolicy, IDockSplitPolicy, IDockOuterSplitPolicy
{
    private IDockWindow? _evaluatingClose;
    private IDock? _selectionOwner;
    private IDockable? _selectionTarget;
    private IDockable? _writingActiveFlag;
    private IDockable? _writingSelectedFlag;
    private IDock? _projectingPaneSelection;
    private bool _passivePreview;
    internal IDockWindow? RequestedHostWindow { get; private set; }
    internal event Action? SettingFocus;
    internal event Action<IDock>? FocusSet;

    internal void RestoreOriginalOwner(IDockable dockable, IDockable? originalOwner)
    {
        dockable.OriginalOwner = originalOwner;
        // Pinned/hidden XML contents are initialized before their return IDs
        // are resolved. Recompute native state before layout collection can
        // discard a floating return pane that is still needed by the content.
        UpdateDockingWindowState(dockable);
    }

    public bool PreserveLayoutOnHostClose(IDockWindow window) => window.Host is LayoutFloatingWindowControl;

    bool IDockDragPreviewPolicy.ShowDockablePreviewOnDrag(IDockable dockable) =>
        getLayoutElement(dockable).Root?.Manager?.ShowDockablePreviewOnDrag ?? true;

    bool IDockDropPolicy.CanDrop(IDockable source, IDockable target, DockOperation operation) =>
        dropPolicy.CanDrop(source, target, operation);

    bool IDockSplitPolicy.UseDocumentDock(IDockable source, IDock target, DockOperation operation) =>
        target is IDocumentDock;

    IDock? IDockOuterSplitPolicy.GetOuterSplitTarget(IDockable source, IDockable target, DockOperation operation) =>
        LayoutOuterSplitPolicy.Resolve(source, getLayoutElement(target), operation);

    internal void SetPaneSelection(IDock owner, IDockable? selected)
    {
        IDock? previousOwner = _selectionOwner;
        IDockable? previousTarget = _selectionTarget;
        IDock? previousProjection = _projectingPaneSelection;
        _selectionOwner = owner;
        _selectionTarget = selected;
        _projectingPaneSelection = owner;
        try { owner.ActiveDockable = selected; }
        finally
        {
            _selectionOwner = previousOwner;
            _selectionTarget = previousTarget;
            _projectingPaneSelection = previousProjection;
        }
    }

    public override void InitActiveDockable(IDockable? dockable, IDock owner)
    {
        if (!ReferenceEquals(owner, _projectingPaneSelection) &&
            getLayoutElement(owner) is not ILayoutPaneSelection { IsMovingChildren: true })
        {
            base.InitActiveDockable(dockable, owner);
            return;
        }
        // Public selection has already updated each affected flag, including
        // nested callbacks. Project the chosen item without a second native
        // normalization pass changing those observable intermediate states.
        OnActiveDockableChanged(dockable);
        dockable?.OnSelected();
    }

    protected override bool ShouldFocusActiveDockable(IDockable dockable, IDock owner) =>
        !_passivePreview &&
        (!ReferenceEquals(owner, _selectionOwner) || !ReferenceEquals(dockable, _selectionTarget)) &&
        base.ShouldFocusActiveDockable(dockable, owner);

    protected override bool ShouldSynchronizeDockingWindowSelection(IDockable dockable) =>
        (_projectingPaneSelection is null || !ReferenceEquals(dockable.Owner, _projectingPaneSelection)) &&
        (dockable.Owner is not IDock owner || getLayoutElement(owner) is not ILayoutPaneSelection { IsMovingChildren: true }) &&
        base.ShouldSynchronizeDockingWindowSelection(dockable);

    protected override void OnDockingWindowStatePropertyChanged(IDockable dockable, DockingWindowStateProperty property)
    {
        if (property == DockingWindowStateProperty.IsActive && ReferenceEquals(dockable, _writingActiveFlag)) return;
        if (property == DockingWindowStateProperty.IsSelected && ReferenceEquals(dockable, _writingSelectedFlag)) return;
        // An auto-hide selection is a preview request. Its public flag is reset
        // by the anchor, without clearing Dock's active preview content or
        // selecting the pinned tool as the root's main layout.
        if (property == DockingWindowStateProperty.IsSelected && IsDockablePinned(dockable)
            && getLayoutElement(dockable) is LayoutAnchorable { IsAutoHidden: true }) return;
        base.OnDockingWindowStatePropertyChanged(dockable, property);
    }

    internal void SetContentActiveFlag(IDockable model, bool active)
    {
        IDockable? previous = _writingActiveFlag;
        _writingActiveFlag = model;
        try { ((IDockingWindowState)model).IsActive = active; }
        finally { _writingActiveFlag = previous; }
    }

    internal void SetContentSelectedFlag(IDockable model, bool selected)
    {
        IDockable? previous = _writingSelectedFlag;
        _writingSelectedFlag = model;
        try { ((IDockingWindowState)model).IsSelected = selected; }
        finally { _writingSelectedFlag = previous; }
    }

    internal void PreviewAnchor(IDockable model, bool activate)
    {
        bool previous = _passivePreview;
        _passivePreview = !activate;
        try { PreviewPinnedDockable(model); }
        finally { _passivePreview = previous; }
    }

    public override void MovePinnedDockable(IDockable sourceDockable, IDockable targetDockable)
    {
        if (getLayoutElement(sourceDockable).FindParent<LayoutAnchorSide>() is { } side)
            side.MovePinned(() => base.MovePinnedDockable(sourceDockable, targetDockable));
        else base.MovePinnedDockable(sourceDockable, targetDockable);
    }

    protected override IDockable? GetActiveDockableAfterRemoval(IDock dock, IDockable removedDockable, int removedIndex)
    {
        if (dock is IDocumentDock or IToolDock && getLayoutElement(dock) is ILayoutContentSelector selector)
        {
            (selector as ILayoutPaneSelection)?.ReconcileSelection();
            return selector.SelectedContent?.DockModel;
        }
        return base.GetActiveDockableAfterRemoval(dock, removedDockable, removedIndex);
    }

    public override void SetFocusedDockable(IDock dock, IDockable? dockable)
    {
        // Native focus notifications precede activation/selection synchronization.
        // Let the public layout report the completed operation to binding observers.
        SettingFocus?.Invoke();
        try { base.SetFocusedDockable(dock, dockable); }
        finally { FocusSet?.Invoke(dock); }
    }

    public IHostWindow? CreateHostWindow(IDockWindow window)
    {
        // Preserve native ID-based and application-provided locators. The
        // manager's locator can additionally choose a host for this model.
        IDockWindow? previous = RequestedHostWindow;
        RequestedHostWindow = window;
        try { return GetHostWindow(window.Id); }
        finally { RequestedHostWindow = previous; }
    }

    internal LayoutItem? GetLayoutItem(IDockable model) => getLayoutItem(model);
    internal LayoutContent? GetLayoutContent(IDockable model) => getLayoutElement(model) as LayoutContent;
    internal ILayoutContainer? GetLayoutContainer(IDockable model) => getLayoutElement(model) as ILayoutContainer;

    // AvalonDock's explicit model operations are independent of UI capability
    // flags. Keep their native lifecycle without changing policy for other calls.
    internal void CloseContent(IDockable model) => CloseDockableCore(model);
    internal void ToggleAutoHideContent(IDockable model, IRootDock root, IToolDock? destination)
    {
        IDock? previousOwner = _selectionOwner;
        IDockable? previousTarget = _selectionTarget;
        _selectionOwner = destination;
        _selectionTarget = model;
        try { PinDockableCore(model, root, destination); }
        finally { _selectionOwner = previousOwner; _selectionTarget = previousTarget; }
    }

    public override void RemoveDockable(IDockable dockable, bool collapse)
    {
        if (dockable is Tool && getLayoutElement(dockable) is LayoutAnchorable { IsAutoHidden: true } tool)
            LayoutAutoHideOperations.Toggle(tool);
        base.RemoveDockable(dockable, collapse);
    }

    public override IProportionalDockSplitter CreateProportionalDockSplitter()
    {
        IProportionalDockSplitter splitter = base.CreateProportionalDockSplitter();
        splitter.ResizePreview = true;
        return splitter;
    }

    bool IDockItemDragPolicy.CanDragItem(IDockable dockable) =>
        getLayoutItem(dockable)?.LayoutElement is { IsEnabled: true } &&
        DockCapabilityResolver.IsEnabled(dockable, DockCapability.Drag, DockCapabilityResolver.ResolveOperationDock(dockable));

    bool IDockItemDragPolicy.CanReorderItem(IDockable dockable)
    {
        ILayoutContainer? parent = getLayoutItem(dockable)?.LayoutElement?.Parent;
        return parent is not ILayoutPositionableGroup { CanRepositionItems: false } &&
            parent?.Parent is not ILayoutPositionableGroup { CanRepositionItems: false };
    }

    public override bool OnWindowClosing(IDockWindow? window)
    {
        // DockWindow.OnClose normally calls CloseWindow before the cancelable
        // factory event. Evaluate that event without changing the layout.
        IDockWindow? previous = _evaluatingClose;
        _evaluatingClose = window;
        bool allowed;
        try { allowed = base.OnWindowClosing(window); }
        finally { _evaluatingClose = previous; }
        return allowed && (window is null || PreserveLayoutOnHostClose(window) || closeWindow(window));
    }

    public override void CloseWindow(IDockWindow window)
    {
        if (!ReferenceEquals(_evaluatingClose, window)) base.CloseWindow(window);
    }

    public override void RemoveWindow(IDockWindow window)
    {
        if (window.Host is LayoutFloatingWindowControl { HasClosed: false, Model: LayoutAnchorableFloatingWindow { IsValid: false } model } &&
            model.Root is { } root)
        {
            root.FloatingWindows.Remove(model);
            return;
        }
        base.RemoveWindow(window);
    }

    public IDockOperationScope? BeginDockOperation(IDockable source, IDockable target, DockOperation operation)
    {
        LayoutOperation notification = operation == DockOperation.Window ? LayoutOperation.Float : LayoutOperation.Dock;
        if (operation == DockOperation.Fill && source is not IDock &&
            (ReferenceEquals(source.Owner, target) || ReferenceEquals(source.Owner, target.Owner)))
            notification = LayoutOperation.Silent;
        IDockOperationScope? scope = operations.Begin(source, notification);
        if (operation is DockOperation.OuterLeft or DockOperation.OuterTop or DockOperation.OuterRight or DockOperation.OuterBottom)
        {
            LayoutElement targetElement = getLayoutElement(target);
            if ((targetElement as LayoutRoot ?? targetElement.Root as LayoutRoot) is { } root)
                return new LayoutOuterSplitScope(root, scope);
        }
        return scope;
    }

    public override void FloatAllDockables(IDockable dockable, DockWindowOptions? options)
    {
        using IDockOperationScope? scope = operations.Begin(dockable.Owner ?? dockable, LayoutOperation.Float);
        floatingPlacement.FloatAll(dockable, () => base.FloatAllDockables(dockable, options));
        scope?.Complete();
    }

    public override void FloatDockable(IDockable dockable, DockWindowOptions? options)
    {
        if (dockable.Owner is not IDock || !DockCapabilityResolver.IsEnabled(dockable,
                DockCapability.Float, DockCapabilityResolver.ResolveOperationDock(dockable))) return;
        // Native floating first unpins auto-hidden content. Observe that change
        // within the same scope as the later window creation.
        using IDockOperationScope? scope = operations.Begin(dockable, LayoutOperation.Float);
        base.FloatDockable(dockable, options);
        scope?.Complete();
    }

    public override void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height, DockWindowOptions? options)
    {
        using IDockOperationScope? scope = operations.Begin(dockable, LayoutOperation.Float);
        floatingPlacement.FloatContainer(dockable, () => base.SplitToWindow(dock, dockable, x, y, width, height, options));
        scope?.Complete();
    }

    public override void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
    {
        using IDockOperationScope? scope = ReferenceEquals(sourceDock, targetDock)
            ? null : operations.Begin(sourceDockable, LayoutOperation.Dock);
        base.MoveDockable(sourceDock, targetDock, sourceDockable, targetDockable);
        scope?.Complete();
    }

    public override void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        using IDockOperationScope? scope = operations.Begin(dockable, LayoutOperation.Dock);
        if (dock is IDocumentDock && dockable is IDocumentDock && getLayoutElement(dock) is LayoutDocumentPane pane)
            LayoutDocumentSplit.Prepare(pane, operation);
        base.SplitToDock(dock, dockable, operation);
        scope?.Complete();
    }
}
