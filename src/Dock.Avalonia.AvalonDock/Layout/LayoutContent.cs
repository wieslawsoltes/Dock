using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>A document or tool whose state is owned by Dock.</summary>
public abstract class LayoutContent : LayoutElement, IComparable<LayoutContent>
{
    /// <summary>Identifies the title property.</summary>
    public static readonly DirectProperty<LayoutContent, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<LayoutContent, string>(nameof(Title), x => x.Title, (x, v) => x.Title = v);
    /// <summary>Identifies the content identifier property.</summary>
    public static readonly DirectProperty<LayoutContent, string> ContentIdProperty =
        AvaloniaProperty.RegisterDirect<LayoutContent, string>(nameof(ContentId), x => x.ContentId, (x, v) => x.ContentId = v);
    /// <summary>Identifies the content property.</summary>
    public static readonly DirectProperty<LayoutContent, object?> ContentProperty =
        AvaloniaProperty.RegisterDirect<LayoutContent, object?>(nameof(Content), x => x.Content, (x, v) => x.Content = v);

    private double _floatingWidth = 300;
    private double _floatingHeight = 400;
    private double _floatingLeft;
    private double _floatingTop;
    private bool _isMaximized;
    private bool _isEnabled = true;
    private object? _toolTip;
    private IImage? _iconSource;
    private ILayoutContainer? _previousContainer;
    private string? _previousContainerId;
    private int _previousContainerIndex = -1;
    private DateTime? _lastActivationTimeStamp;

    internal LayoutContent(DockableBase model, LayoutModelContext? context = null) : base(model, context) { }
    private IDockingWindowState State => (IDockingWindowState)DockModel;

    /// <summary>Gets or sets the tab title.</summary>
    public string Title { get => DockModel.Title; set { RaisePropertyChanging(); DockModel.Title = value; } }
    /// <summary>Gets or sets the identifier used for persistence.</summary>
    public string ContentId { get => DockModel.Id; set { RaisePropertyChanging(); DockModel.Id = value; } }
    /// <summary>Gets or sets the document or tool content.</summary>
    [Content]
    public object? Content
    {
        get => DockModel is Document document ? document.Content : ((Tool)DockModel).Content;
        set
        {
            RaisePropertyChanging();
            if (DockModel is Document document) document.Content = value;
            else ((Tool)DockModel).Content = value;
        }
    }
    /// <summary>Gets or sets whether this item is selected in its pane.</summary>
    public bool IsSelected { get => State.IsSelected; set => State.IsSelected = value; }
    /// <summary>Gets or sets whether this item is active.</summary>
    public bool IsActive { get => State.IsActive; set => State.IsActive = value; }
    /// <summary>Gets whether this is the last focused document.</summary>
    public bool IsLastFocusedDocument => ReferenceEquals((Root as LayoutRoot)?.LastFocusedDocument, this);
    /// <summary>Gets or sets the previous container.</summary>
    public ILayoutContainer? PreviousContainer { get => _previousContainer; set => SetField(ref _previousContainer, value); }
    /// <summary>Gets or sets the previous container's persistence identifier.</summary>
    public string? PreviousContainerId { get => _previousContainerId; set => SetField(ref _previousContainerId, value); }
    /// <summary>Gets or sets the previous index in a container.</summary>
    public int PreviousContainerIndex { get => _previousContainerIndex; set => SetField(ref _previousContainerIndex, value); }
    /// <summary>Gets or sets the last activation time.</summary>
    public DateTime? LastActivationTimeStamp { get => _lastActivationTimeStamp; set => SetField(ref _lastActivationTimeStamp, value); }
    /// <summary>Gets or sets the floating width.</summary>
    public double FloatingWidth { get => _floatingWidth; set => SetField(ref _floatingWidth, value); }
    /// <summary>Gets or sets the floating height.</summary>
    public double FloatingHeight { get => _floatingHeight; set => SetField(ref _floatingHeight, value); }
    /// <summary>Gets or sets the floating horizontal position.</summary>
    public double FloatingLeft { get => _floatingLeft; set => SetField(ref _floatingLeft, value); }
    /// <summary>Gets or sets the floating vertical position.</summary>
    public double FloatingTop { get => _floatingTop; set => SetField(ref _floatingTop, value); }
    /// <summary>Gets or sets the floating maximized state.</summary>
    public bool IsMaximized { get => _isMaximized; set => SetField(ref _isMaximized, value); }
    /// <summary>Gets or sets tooltip content.</summary>
    public object? ToolTip { get => _toolTip; set => SetField(ref _toolTip, value); }
    /// <summary>Gets or sets the tab icon.</summary>
    public IImage? IconSource { get => _iconSource; set => SetField(ref _iconSource, value); }
    /// <summary>Gets whether this item is in a floating window.</summary>
    public bool IsFloating => (DockModel.DockingState & DockingWindowState.Floating) != 0;
    /// <summary>Gets or sets whether this item can be closed.</summary>
    public bool CanClose { get => DockModel.CanClose; set => DockModel.CanClose = value; }
    /// <summary>Gets or sets whether this item can be floated.</summary>
    public bool CanFloat { get => DockModel.CanFloat; set => DockModel.CanFloat = value; }
    /// <summary>Gets or sets whether the content is enabled.</summary>
    public bool IsEnabled { get => _isEnabled; set => SetField(ref _isEnabled, value); }
    /// <summary>Closes the item, honoring capabilities and cancellation.</summary>
    public abstract void Close();
    /// <summary>Compares titles using the current culture.</summary>
    public int CompareTo(LayoutContent? other) => other is null ? 1 : string.Compare(Title, other.Title, StringComparison.CurrentCulture);
    /// <summary>Floats this item through Dock's window implementation.</summary>
    public void Float()
    {
        if (!CanFloat || IsFloating) return;
        PreviousContainer = Parent;
        PreviousContainerIndex = Parent is ILayoutGroup group ? group.IndexOfChild(this) : -1;
        DockModel.SetVisibleBounds(FloatingLeft, FloatingTop, FloatingWidth, FloatingHeight);
        DockModel.Factory?.FloatDockable(DockModel);
    }
    /// <summary>Docks this item in a document pane.</summary>
    public void DockAsDocument() => DockModel.Factory?.DockAsDocument(DockModel);
    /// <summary>Returns the item to its previous container.</summary>
    public void Dock()
    {
        if (PreviousContainer is LayoutElement previous && previous.DockModel is IDock target && DockModel.Owner is IDock source && DockModel.Factory is { } factory)
            factory.MoveDockable(source, target, DockModel, null);
    }
    /// <summary>Occurs after the item is closed.</summary>
    public event EventHandler? Closed;
    /// <summary>Occurs before closing and permits cancellation.</summary>
    public event EventHandler<CancelEventArgs>? Closing;
    /// <summary>Occurs when pane selection changes.</summary>
    public event EventHandler? IsSelectedChanged;
    /// <summary>Occurs when active state changes.</summary>
    public event EventHandler? IsActiveChanged;

    internal void NotifyClosing(CancelEventArgs args) => Closing?.Invoke(this, args);
    internal void NotifyClosed() => Closed?.Invoke(this, EventArgs.Empty);
    internal void RememberDockedLocation()
    {
        if (Parent is not LayoutElement parent || parent is LayoutAnchorGroup or LayoutAnchorSide
            || parent is not ILayoutGroup group) return;
        PreviousContainer = group;
        PreviousContainerIndex = group.IndexOfChild(this);
        PreviousContainerId = parent.DockModel.Id;
        DockModel.OriginalOwner = parent.DockModel;
    }
    internal void CloseCore()
    {
        if (!CanClose) return;
        if (DockModel.Factory is { } factory) factory.CloseDockable(DockModel);
        else
        {
            CancelEventArgs args = new();
            NotifyClosing(args);
            if (args.Cancel) return;
            Parent?.RemoveChild(this);
            NotifyClosed();
        }
    }

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockableBase.TitleProperty)
            RaisePropertyChanged(TitleProperty, (string)e.OldValue!, (string)e.NewValue!);
        else if (e.Property == DockableBase.IdProperty)
        {
            RaisePropertyChanged(ContentIdProperty, (string)e.OldValue!, (string)e.NewValue!);
            RaisePropertyChanged(nameof(ContentId));
        }
        else if (e.Property == Document.ContentProperty || e.Property == Tool.ContentProperty)
            RaisePropertyChanged(ContentProperty, e.OldValue, e.NewValue);
        else if (e.Property.Name == nameof(IsSelected)) IsSelectedChanged?.Invoke(this, EventArgs.Empty);
        else if (e.Property.Name == nameof(IsActive))
        {
            if (IsActive) LastActivationTimeStamp = DateTime.Now;
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (e.Property == DockableBase.DockingStateProperty) RaisePropertyChanged(nameof(IsFloating));
    }
}
