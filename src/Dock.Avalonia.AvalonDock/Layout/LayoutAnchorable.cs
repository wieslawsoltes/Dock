using System;
using System.ComponentModel;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>An AvalonDock tool backed by a Dock Avalonia tool.</summary>
public class LayoutAnchorable : LayoutContent
{
    /// <summary>Identifies the tool's visible state for Avalonia bindings.</summary>
    public static readonly DirectProperty<LayoutAnchorable, bool> IsVisibleProperty =
        AvaloniaProperty.RegisterDirect<LayoutAnchorable, bool>(nameof(IsVisible), x => x.IsVisible, (x, value) => x.IsVisible = value);

    private bool _canHide = true;
    private double _autoHideMinWidth = 100;
    private double _autoHideMinHeight = 100;
    private bool _lastVisible;
    private bool _lastHidden;
    private bool _lastAutoHidden;
    /// <summary>Creates an anchorable tool.</summary>
    public LayoutAnchorable() : this(new Tool { CanClose = false }) { }
    internal LayoutAnchorable(Tool model, LayoutModelContext? context = null) : base(model, context)
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Parent)) NotifyVisibility();
        };
    }
    /// <summary>Gets or sets the auto-hide popup width.</summary>
    public double AutoHideWidth
    {
        get => DockModel.PinnedBounds?.Width ?? 0;
        set { DockModel.PinnedBounds = new DockRect(0, 0, value, AutoHideHeight); RaisePropertyChanged(); }
    }
    /// <summary>Gets or sets the auto-hide popup height.</summary>
    public double AutoHideHeight
    {
        get => DockModel.PinnedBounds?.Height ?? 0;
        set { DockModel.PinnedBounds = new DockRect(0, 0, AutoHideWidth, value); RaisePropertyChanged(); }
    }
    /// <summary>Gets or sets the minimum auto-hide width.</summary>
    public double AutoHideMinWidth { get => _autoHideMinWidth; set => SetField(ref _autoHideMinWidth, value); }
    /// <summary>Gets or sets the minimum auto-hide height.</summary>
    public double AutoHideMinHeight { get => _autoHideMinHeight; set => SetField(ref _autoHideMinHeight, value); }
    /// <summary>Gets or sets whether the tool can be hidden.</summary>
    public bool CanHide { get => _canHide; set => SetField(ref _canHide, value); }
    /// <summary>Gets or sets whether the tool can be auto-hidden.</summary>
    public bool CanAutoHide { get => DockModel.CanPin; set => DockModel.CanPin = value; }
    /// <summary>Gets or sets whether the tool can become a document tab.</summary>
    public bool CanDockAsTabbedDocument { get => DockModel.CanDockAsDocument; set => DockModel.CanDockAsDocument = value; }
    /// <summary>Gets whether this tool is in an auto-hide strip.</summary>
    public bool IsAutoHidden => Parent is LayoutAnchorGroup;
    /// <summary>Gets whether this tool is hidden.</summary>
    public bool IsHidden => Parent is LayoutRoot;
    /// <summary>Gets whether this tool is visible in a layout.</summary>
    public bool IsVisible
    {
        get => Parent is not null && !IsHidden;
        set { if (value) Show(); else Hide(); }
    }
    /// <summary>Occurs when visibility changes.</summary>
    public event EventHandler? IsVisibleChanged;
    /// <summary>Occurs before hiding and permits cancellation.</summary>
    public event EventHandler<CancelEventArgs>? Hiding;
    /// <summary>Occurs after hiding.</summary>
    public event EventHandler? Hidden;
    /// <inheritdoc/>
    public override void Close() => CloseCore();
    /// <inheritdoc/>
    protected override void InternalDock()
    {
        if (Root is LayoutRoot root) LayoutSourcePlacement.DockAnchorable(root, this, root.Manager?.LayoutUpdateStrategy);
        base.InternalDock();
    }
    /// <summary>Hides this tool without destroying its content.</summary>
    public void Hide(bool cancelable = true)
    {
        if (!IsVisible) { IsSelected = true; IsActive = true; return; }
        CancelEventArgs args = new();
        if (cancelable) OnHiding(args);
        if (args.Cancel) return;
        InitialContainer = PreviousContainer;
        InitialContainerIndex = PreviousContainerIndex;
        InitialContainerId = PreviousContainerId;
        ILayoutContainer? previous = Parent;
        LayoutAnchorSide? side = (previous as LayoutAnchorGroup)?.Parent as LayoutAnchorSide;
        int index = (previous as ILayoutGroup)?.IndexOfChild(this) ?? -1;
        DockModel.Factory?.HideDockable(DockModel);
        if (!IsHidden) return;
        PreviousContainer = previous;
        PreviousContainerIndex = index;
        PreviousContainerId = (previous as LayoutElement)?.DockModel.Id;
        // Dock unpins before hiding. Keep the AvalonDock group as the return
        // destination even when it no longer has a visible tool.
        if (previous is LayoutAnchorGroup group && side is not null && group.Parent is null) side.Children.Add(group);
        OnHidden();
    }
    /// <summary>Restores this tool to its previous pane.</summary>
    public void Show()
    {
        if (IsVisible) return;
        if (!IsHidden) throw new InvalidOperationException("The tool must belong to a layout before it can be shown.");
        LayoutRoot root = (LayoutRoot)Root!;
        ILayoutGroup? previous = PreviousContainer as ILayoutGroup;
        if (previous is null && DockModel.OriginalOwner is { } owner) previous = Context.Get(owner) as ILayoutGroup;
        ILayoutUpdateStrategy? strategy = root.Manager?.LayoutUpdateStrategy;
        bool inserted = strategy?.BeforeInsertAnchorable(root, this, previous) == true;
        if (!inserted && previous is not null)
        {
            root.DockFactory.RestoreDockable(DockModel);
            int index = PreviousContainerIndex < 0 ? previous.ChildrenCount : Math.Min(PreviousContainerIndex, previous.ChildrenCount);
            if (ReferenceEquals(Parent, previous))
            {
                int current = previous.IndexOfChild(this);
                int target = Math.Min(index, previous.ChildrenCount - 1);
                if (current != target && previous is ILayoutPane pane) pane.MoveChild(current, target);
            }
            else previous.InsertChildAt(index, this);
            IsSelected = true;
            IsActive = true;
        }
        strategy?.AfterInsertAnchorable(root, this);
        PreviousContainer = InitialContainer;
        PreviousContainerIndex = InitialContainerIndex;
        PreviousContainerId = InitialContainerId;
        InitialContainer = null;
        InitialContainerIndex = -1;
        InitialContainerId = null;
    }
    /// <summary>Adds a detached tool to the requested edge of a manager's layout.</summary>
    public void AddToLayout(DockingManager manager, AnchorableShowStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(manager);
        if (IsVisible || IsHidden) throw new InvalidOperationException("The tool already belongs to a layout.");
        LayoutAnchorablePlacement.Add(manager.Layout, this, strategy);
    }
    /// <summary>Raises the cancelable hiding event.</summary>
    protected virtual void OnHiding(CancelEventArgs args) => Hiding?.Invoke(this, args);
    /// <summary>Raises the hidden event.</summary>
    protected virtual void OnHidden() => Hidden?.Invoke(this, EventArgs.Empty);
    /// <summary>Toggles Dock's auto-hide state.</summary>
    public void ToggleAutoHide()
    {
        if (!CanAutoHide) return;
        if (IsAutoHidden) DockModel.Factory?.UnpinDockable(DockModel);
        else
        {
            RememberDockedLocation();
            if (DockModel.Owner is ToolDock pane)
            {
                pane.Alignment = (Parent?.GetSide() ?? AnchorSide.Right) switch
                {
                    AnchorSide.Left => Alignment.Left,
                    AnchorSide.Top => Alignment.Top,
                    AnchorSide.Right => Alignment.Right,
                    _ => Alignment.Bottom
                };
            }
            DockModel.Factory?.PinDockable(DockModel);
        }
    }
    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockableBase.DockingStateProperty)
            NotifyVisibility();
    }

    private void NotifyVisibility()
    {
        bool visible = IsVisible;
        bool hidden = IsHidden;
        bool autoHidden = IsAutoHidden;
        if (_lastHidden != hidden) { _lastHidden = hidden; RaisePropertyChanged(nameof(IsHidden)); }
        if (_lastAutoHidden != autoHidden) { _lastAutoHidden = autoHidden; RaisePropertyChanged(nameof(IsAutoHidden)); }
        if (_lastVisible == visible) return;
        bool old = _lastVisible;
        _lastVisible = visible;
        RaisePropertyChanged(IsVisibleProperty, old, visible);
        RaisePropertyChanged(nameof(IsVisible));
        IsVisibleChanged?.Invoke(this, EventArgs.Empty);
    }
}
