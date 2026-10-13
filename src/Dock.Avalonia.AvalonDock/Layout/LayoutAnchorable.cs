using System;
using System.ComponentModel;
using System.Xml;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

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
    public LayoutAnchorable() : this(new Tool { Id = null!, Title = null!, CanClose = false }) { }
    internal LayoutAnchorable(Tool model, LayoutModelContext? context = null) : base(model, context) { }
    /// <summary>Gets or sets the auto-hide popup width, clamped to <see cref="AutoHideMinWidth"/> when assigned.</summary>
    public double AutoHideWidth
    {
        get => GetAutoHideSize().Width;
        set
        {
            if (AutoHideWidth == value) return;
            RaisePropertyChanging();
            DockModel.PinnedBounds = new DockRect(0, 0, Math.Max(value, AutoHideMinWidth), AutoHideHeight);
            RaisePropertyChanged();
        }
    }
    /// <summary>Gets or sets the auto-hide popup height, clamped to <see cref="AutoHideMinHeight"/> when assigned.</summary>
    public double AutoHideHeight
    {
        get => GetAutoHideSize().Height;
        set
        {
            if (AutoHideHeight == value) return;
            RaisePropertyChanging();
            DockModel.PinnedBounds = new DockRect(0, 0, AutoHideWidth, Math.Max(value, AutoHideMinHeight));
            RaisePropertyChanged();
        }
    }
    /// <summary>Gets or sets the minimum auto-hide width. Defaults to 100; negative values are rejected.</summary>
    public double AutoHideMinWidth
    {
        get => _autoHideMinWidth;
        set
        {
            if (_autoHideMinWidth == value) return;
            RaisePropertyChanging();
            if (value < 0) throw new ArgumentException("value");
            _autoHideMinWidth = value;
            RaisePropertyChanged();
        }
    }
    /// <summary>Gets or sets the minimum auto-hide height. Defaults to 100; negative values are rejected.</summary>
    public double AutoHideMinHeight
    {
        get => _autoHideMinHeight;
        set
        {
            if (_autoHideMinHeight == value) return;
            RaisePropertyChanging();
            if (value < 0) throw new ArgumentException("value");
            _autoHideMinHeight = value;
            RaisePropertyChanged();
        }
    }

    private (double Width, double Height) GetAutoHideSize()
    {
        DockModel.GetPinnedBounds(out double x, out double y, out double width, out double height);
        // Dock represents an unset rectangle with four NaNs. Read its raw
        // dimensions so an explicitly assigned NaN or infinity is not lost
        // through the native PinnedBounds serialization filter.
        return double.IsNaN(x) && double.IsNaN(y) && double.IsNaN(width) && double.IsNaN(height)
            ? (0, 0) : (width, height);
    }
    /// <summary>Gets or sets whether the tool can be hidden.</summary>
    public bool CanHide { get => _canHide; set => SetFieldChanged(ref _canHide, value); }
    /// <summary>Gets or sets whether the tool can be auto-hidden.</summary>
    public bool CanAutoHide { get => DockModel.CanPin; set => DockModel.CanPin = value; }
    /// <summary>Gets or sets whether the tool can become a document tab.</summary>
    public bool CanDockAsTabbedDocument { get => DockModel.CanDockAsDocument; set => DockModel.CanDockAsDocument = value; }
    /// <summary>Reads tool-specific metadata before the shared content metadata.</summary>
    public override void ReadXml(XmlReader reader)
    {
        LayoutContentXml.ReadAnchorable(this, reader);
        base.ReadXml(reader);
    }
    /// <summary>Writes tool metadata followed by common content attributes using AvalonDock's omission rules.</summary>
    public override void WriteXml(XmlWriter writer)
    {
        writer = AvalonDockXml.PrepareWriter(this, writer);
        LayoutXmlAttributes.Anchorable(this, writer);
        base.WriteXml(writer);
    }
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
    /// <summary>Refreshes pane visibility and reports tool visibility before the parent notification.</summary>
    protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        (newValue as ILayoutElementWithVisibility)?.ComputeVisibility();
        NotifyVisibility(true);
        base.OnParentChanged(oldValue, newValue);
    }
    /// <inheritdoc/>
    protected override void InternalDock()
    {
        if (Root is LayoutRoot root) LayoutSourcePlacement.DockAnchorable(root, this);
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
        int groupIndex = previous is LayoutAnchorGroup previousGroup && side is not null ? side.Children.IndexOf(previousGroup) : -1;
        int index = (previous as ILayoutGroup)?.IndexOfChild(this) ?? -1;
        DockModel.Factory?.HideDockable(DockModel);
        if (!IsHidden) return;
        PreviousContainer = previous;
        PreviousContainerIndex = index;
        PreviousContainerId = (previous as LayoutElement)?.DockModel.Id;
        // Dock unpins before hiding. Keep the AvalonDock group as the return
        // destination even when it no longer has a visible tool.
        if (previous is LayoutAnchorGroup group && side is not null && group.Parent is null)
            side.Children.Insert(groupIndex < 0 ? side.Children.Count : Math.Min(groupIndex, side.Children.Count), group);
        OnHidden();
    }
    /// <summary>Restores this tool to its previous pane.</summary>
    /// <remarks>
    /// The update strategy may change <see cref="LayoutContent.PreviousContainer"/>
    /// and <see cref="LayoutContent.PreviousContainerIndex"/> before default
    /// restoration. Clearing the return container leaves the tool hidden.
    /// </remarks>
    public void Show()
    {
        if (IsVisible) return;
        if (!IsHidden) throw new InvalidOperationException("The tool must belong to a layout before it can be shown.");
        LayoutRoot root = (LayoutRoot)Root!;
        ILayoutContainer? previousBeforeCallback = PreviousContainer;
        ILayoutGroup? previous = PreviousContainer as ILayoutGroup;
        if (previous is null && DockModel.OriginalOwner is { } owner) previous = Context.Get(owner) as ILayoutGroup;
        bool inserted = root.Manager?.LayoutUpdateStrategy?.BeforeInsertAnchorable(root, this, previous) == true;
        if (!ReferenceEquals(previousBeforeCallback, PreviousContainer))
            previous = PreviousContainer as ILayoutGroup;
        if (!inserted && previous is not null)
        {
            // Anchor groups project Dock's pinned lists rather than own native
            // content. Other destinations can be restored directly, avoiding
            // a transient insertion into the tool's old pane.
            if (previous is LayoutElement returnPane and not LayoutAnchorGroup)
                DockModel.OriginalOwner = returnPane.DockModel;
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
        root.Manager?.LayoutUpdateStrategy?.AfterInsertAnchorable(root, this);
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
    /// <summary>Toggles Dock's auto-hide state independently of <see cref="CanAutoHide"/>.</summary>
    /// <remarks>
    /// Each pin creates an anchor group in the manager's root, including for a
    /// floating tool. Restoring uses its previous pane or creates one on that
    /// edge, redirects hidden return locations, and selects the restored tool.
    /// </remarks>
    public void ToggleAutoHide() => LayoutAutoHideOperations.Toggle(this);
    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == DockableBase.CanPinProperty)
        {
            RaisePropertyChanged(nameof(CanAutoHide));
            return;
        }
        if (e.Property == DockableBase.CanDockAsDocumentProperty)
        {
            RaisePropertyChanged(nameof(CanDockAsTabbedDocument));
            return;
        }
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockableBase.DockingStateProperty)
            NotifyVisibility();
    }

    private void NotifyVisibility(bool parentChanged = false)
    {
        bool visible = IsVisible;
        bool hidden = IsHidden;
        bool autoHidden = IsAutoHidden;
        bool visibleChanged = _lastVisible != visible;
        bool hiddenChanged = _lastHidden != hidden;
        bool autoHiddenChanged = _lastAutoHidden != autoHidden;
        bool previousVisible = _lastVisible;
        _lastVisible = visible;
        _lastHidden = hidden;
        _lastAutoHidden = autoHidden;
        if (visibleChanged) RaisePropertyChanged(IsVisibleProperty, previousVisible, visible);
        if (visibleChanged || parentChanged)
        {
            RaisePropertyChanged(nameof(IsVisible));
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
        }
        if (hiddenChanged || parentChanged) RaisePropertyChanged(nameof(IsHidden));
        if (autoHiddenChanged || parentChanged) RaisePropertyChanged(nameof(IsAutoHidden));
    }
}
