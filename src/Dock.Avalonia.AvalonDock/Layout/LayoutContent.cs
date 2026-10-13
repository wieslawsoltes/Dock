using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A document or tool whose state is owned by Dock.</summary>
public abstract class LayoutContent : LayoutElement, IComparable<LayoutContent>, ILayoutElementForFloatingWindow, IXmlSerializable
{
    internal event EventHandler? LayoutItemChanged;
    internal void NotifyLayoutItemChanged() => LayoutItemChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>Identifies the title property.</summary>
    public static readonly DirectProperty<LayoutContent, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<LayoutContent, string>(nameof(Title), x => x.Title, (x, v) => x.Title = v);
    /// <summary>Identifies the content identifier property.</summary>
    public static readonly DirectProperty<LayoutContent, string?> ContentIdProperty =
        AvaloniaProperty.RegisterDirect<LayoutContent, string?>(nameof(ContentId), x => x.ContentId, (x, v) => x.ContentId = v);
    /// <summary>Identifies the content property.</summary>
    public static readonly DirectProperty<LayoutContent, object?> ContentProperty =
        AvaloniaProperty.RegisterDirect<LayoutContent, object?>(nameof(Content), x => x.Content, (x, v) => x.Content = v);

    private double _floatingWidth;
    private double _floatingHeight;
    private double _floatingLeft;
    private double _floatingTop;
    private bool _isMaximized;
    private bool _isEnabled = true;
    private object? _toolTip;
    private IImage? _iconSource;
    private ILayoutContainer? _previousContainer;
    private string? _previousContainerId;
    private int _previousContainerIndex = -1;
    private ILayoutContainer? _initialContainer;
    private int _initialContainerIndex = -1;
    private DateTime? _lastActivationTimeStamp;
    private bool _synchronizingActive;
    private int _writingActive;
    private bool _isLastFocusedDocument;
    private bool _isFloating;
    private int _contentRevision;

    internal LayoutContent(DockableBase model, LayoutModelContext? context = null) : base(model, context)
    {
        DockingHistory = new(this);
        _isFloating = (model.DockingState & DockingWindowState.Floating) != 0;
    }
    internal LayoutDockingHistory DockingHistory { get; }
    private IDockingWindowState State => (IDockingWindowState)DockModel;

    /// <summary>Gets or sets the tab title; initially null.</summary>
    public string Title
    {
        get => DockModel.Title;
        set
        {
            VerifyAccess();
            if (DockModel.Title == value) return;
            RaisePropertyChanging();
            DockModel.Title = value;
        }
    }
    /// <summary>Gets or sets the identifier used for persistence and active-content lookup; initially null.</summary>
    /// <remarks>A named control assigned to <see cref="Content"/> supplies an identifier when this value is null.</remarks>
    public string? ContentId { get => DockModel.Id; set => DockModel.Id = value!; }
    /// <summary>Gets or sets the document or tool content.</summary>
    [Content]
    public object? Content
    {
        get => DockModel is Document document ? document.Content : ((Tool)DockModel).Content;
        set
        {
            if (ReferenceEquals(Content, value)) return;
            RaisePropertyChanging();
            object? previous = Content;
            int revision = _contentRevision;
            if (DockModel is Document document) document.Content = value;
            else ((Tool)DockModel).Content = value;
            // Avalonia stores an equal replacement without raising its styled
            // property notification. AvalonDock's Content contract uses identity.
            if (revision == _contentRevision) NotifyContentChanged(previous, Content);
        }
    }
    /// <summary>Gets or sets whether this item is selected in its pane.</summary>
    public bool IsSelected
    {
        get => State.IsSelected;
        set
        {
            if (Parent is ILayoutContentSelector selector)
            {
                int index = selector.IndexOf(this);
                if (value && index >= 0) { selector.SelectedContentIndex = index; return; }
                if (!value && ReferenceEquals(selector.SelectedContent, this)) { selector.SelectedContentIndex = -1; return; }
            }
            SetSelectedFromPane(value);
        }
    }
    /// <summary>Gets or sets whether this item is active.</summary>
    /// <remarks>Clearing the flag retains the root's active-content reference and pane selection.</remarks>
    public bool IsActive
    {
        get => State.IsActive;
        set
        {
            VerifyAccess();
            if (State.IsActive == value) return;
            RaisePropertyChanging();
            bool previous = State.IsActive;
            _writingActive++;
            try
            {
                if (DockModel.Factory is LayoutFactory factory) factory.SetContentActiveFlag(DockModel, value);
                else State.IsActive = value;
            }
            finally { _writingActive--; }
            if (State.IsActive)
            {
                if (Root is LayoutRoot root)
                {
                    root.ActiveContent = this;
                    if (State.IsActive) root.Focus.ProjectNativeFocus(this);
                }
                if (State.IsActive) IsSelected = true;
            }
            OnIsActiveChanged(previous, value);
            RaisePropertyChanged(nameof(IsActive));
        }
    }
    /// <summary>Gets whether this is the last focused document.</summary>
    public bool IsLastFocusedDocument => _isLastFocusedDocument;
    /// <summary>Gets or sets the previous container, initializing its persistence identifier after change notification.</summary>
    public ILayoutContainer? PreviousContainer { get => _previousContainer; set => SetContainer(ref _previousContainer, value, nameof(PreviousContainer)); }
    /// <summary>Gets or sets the previous container's persistence identifier.</summary>
    /// <remarks>This serialization metadata does not raise property notifications or change <see cref="PreviousContainer"/>.</remarks>
    public string? PreviousContainerId { get => _previousContainerId; set { VerifyAccess(); _previousContainerId = value; } }
    /// <summary>Gets or sets the previous index in a container.</summary>
    public int PreviousContainerIndex { get => _previousContainerIndex; set => SetFieldChanged(ref _previousContainerIndex, value); }
    internal ILayoutContainer? InitialContainer { get => _initialContainer; set => SetContainer(ref _initialContainer, value, nameof(InitialContainer)); }
    internal string? InitialContainerId { get; set; }
    internal int InitialContainerIndex { get => _initialContainerIndex; set => SetFieldChanged(ref _initialContainerIndex, value); }

    private void SetContainer(ref ILayoutContainer? field, ILayoutContainer? value, string name)
    {
        VerifyAccess();
        if (ReferenceEquals(field, value)) return;
        field = value;
        RaisePropertyChanged(name);
        // Application callbacks may redirect the reference before its pane ID
        // is initialized. Always use the reference that remains after them.
        LayoutContainerIdentity.EnsureReferencedId(field);
    }

    /// <summary>Gets or sets the last activation time.</summary>
    public DateTime? LastActivationTimeStamp { get => _lastActivationTimeStamp; set => SetFieldChanged(ref _lastActivationTimeStamp, value); }
    /// <summary>Gets or sets the floating width; zero requests the native fallback size.</summary>
    public double FloatingWidth { get => _floatingWidth; set => SetField(ref _floatingWidth, value); }
    /// <summary>Gets or sets the floating height; zero requests the native fallback size.</summary>
    public double FloatingHeight { get => _floatingHeight; set => SetField(ref _floatingHeight, value); }
    /// <summary>Gets or sets the floating horizontal position.</summary>
    public double FloatingLeft { get => _floatingLeft; set => SetField(ref _floatingLeft, value); }
    /// <summary>Gets or sets the floating vertical position.</summary>
    public double FloatingTop { get => _floatingTop; set => SetField(ref _floatingTop, value); }
    /// <summary>Gets or sets the floating maximized state.</summary>
    public bool IsMaximized { get => _isMaximized; set => SetField(ref _isMaximized, value); }
    /// <summary>Gets or sets tooltip content.</summary>
    public object? ToolTip { get => _toolTip; set => SetReferenceChanged(ref _toolTip, value); }
    /// <summary>Gets or sets the tab icon.</summary>
    public IImage? IconSource { get => _iconSource; set => SetReferenceChanged(ref _iconSource, value); }
    /// <summary>Gets the item's floating state, restored from XML or updated by native docking operations.</summary>
    /// <remarks>Reading this flag from XML records metadata without moving the item or creating a window.</remarks>
    public bool IsFloating => _isFloating;
    /// <summary>Gets or sets whether UI commands can close this item.</summary>
    public bool CanClose { get => DockModel.CanClose; set => DockModel.CanClose = value; }
    /// <summary>Gets or sets whether this item can be floated.</summary>
    public bool CanFloat { get => DockModel.CanFloat; set => DockModel.CanFloat = value; }
    /// <summary>Gets or sets whether the content is enabled.</summary>
    public bool IsEnabled { get => _isEnabled; set => SetFieldChanged(ref _isEnabled, value); }
    /// <summary>Closes the item, honoring cancellation independently of <see cref="CanClose"/>.</summary>
    public abstract void Close();
    /// <summary>Returns no custom XML schema.</summary>
    public XmlSchema? GetSchema() => null;
    /// <summary>Reads content metadata, retaining its application content and native model.</summary>
    /// <remarks>
    /// Applies setters in AvalonDock's metadata order and advances the reader once.
    /// Earlier assignments remain if a later conversion or application callback fails.
    /// Persisted focus and floating flags do not activate content or create windows.
    /// </remarks>
    public virtual void ReadXml(XmlReader reader) => LayoutContentXml.Read(this, reader);
    /// <summary>Writes content metadata inside the caller's XML element.</summary>
    public virtual void WriteXml(XmlWriter writer) => LayoutXmlAttributes.Content(this, AvalonDockXml.PrepareWriter(this, writer));
    /// <summary>Compares application content when it implements <see cref="IComparable"/>, otherwise titles using the current culture.</summary>
    /// <exception cref="NullReferenceException">The other layout content is null.</exception>
    public int CompareTo(LayoutContent? other) => Content is IComparable comparable
        ? comparable.CompareTo(other!.Content)
        : string.Compare(Title, other!.Title, StringComparison.CurrentCulture);
    /// <summary>Floats this item through Dock's window implementation.</summary>
    /// <remarks>
    /// Restoring a previous floating pane is independent of <see cref="CanFloat"/>.
    /// Creating a new window uses the native factory's capability rules. The item
    /// is selected and activated even when new window creation is disabled.
    /// </remarks>
    /// <exception cref="NullReferenceException">New window creation has no layout root or manager.</exception>
    public void Float()
    {
        if (!DockingHistory.RestorePrevious(floatingOnly: true))
        {
            LayoutOperationContext.RequireManager(this);
            DockModel.SetVisibleBounds(FloatingLeft, FloatingTop, FloatingWidth, FloatingHeight);
            DockModel.Factory?.FloatDockable(DockModel);
        }
        LayoutOperationContext.SelectAndActivate(this);
        Root!.CollectGarbage();
    }
    /// <summary>Docks this item in a document pane.</summary>
    /// <remarks>This explicit operation does not consult an anchorable's tabbed-document capability.</remarks>
    /// <exception cref="InvalidOperationException">The content does not belong to a layout.</exception>
    /// <exception cref="NullReferenceException">A move is required but the layout has no manager.</exception>
    public void DockAsDocument() => LayoutDocumentPlacement.Dock(this);
    /// <summary>Returns the item to its previous container.</summary>
    /// <exception cref="NullReferenceException">The content has no layout root or manager.</exception>
    public void Dock()
    {
        LayoutOperationContext.RequireManager(this).NotifyLayoutOperation(this, floating: false, preview: true);
        if (DockingHistory.RestorePrevious())
        {
            LayoutOperationContext.SelectAndActivate(this);
        }
        else InternalDock();
        LayoutOperationContext.RequireManager(this).NotifyLayoutOperation(this, floating: false, preview: false);
        SetFloating(false);
        Root?.CollectGarbage();
    }
    /// <summary>Chooses a docking destination when no previous container is available.</summary>
    protected virtual void InternalDock() { }

    /// <summary>Clears pane selection before leaving the previous parent.</summary>
    protected override void OnParentChanging(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        if (oldValue is not null) IsSelected = false;
        base.OnParentChanging(oldValue, newValue);
    }

    /// <summary>Applies an initially selected item's state to its new pane.</summary>
    protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        if (IsSelected && newValue is ILayoutContentSelector selector)
            selector.SelectedContentIndex = selector.IndexOf(this);
        base.OnParentChanged(oldValue, newValue);
    }

    internal void SetSelectedFromPane(bool selected)
    {
        VerifyAccess();
        if (State.IsSelected == selected) return;
        RaisePropertyChanging(nameof(IsSelected));
        State.IsSelected = selected;
    }
    /// <summary>Occurs after the item is closed.</summary>
    public event EventHandler? Closed;
    /// <summary>Occurs before closing and permits cancellation.</summary>
    public event EventHandler<CancelEventArgs>? Closing;
    /// <summary>Occurs when pane selection changes.</summary>
    public event EventHandler? IsSelectedChanged;
    /// <summary>Occurs when active state changes.</summary>
    public event EventHandler? IsActiveChanged;

    /// <summary>Raises the selection event after the item's selection changes.</summary>
    /// <param name="oldValue">The previous selection state.</param>
    /// <param name="newValue">The new selection state.</param>
    protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue) => IsSelectedChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>Records activation time and raises the active-state event.</summary>
    /// <param name="oldValue">The previous active state.</param>
    /// <param name="newValue">The requested active state, even if a callback has redirected activation.</param>
    protected virtual void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        if (newValue) LastActivationTimeStamp = DateTime.Now;
        IsActiveChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises the cancelable closing event.</summary>
    /// <param name="args">The cancellation shared with the close operation.</param>
    protected virtual void OnClosing(CancelEventArgs args) => Closing?.Invoke(this, args);

    /// <summary>Raises the closed event after the item has left its layout.</summary>
    protected virtual void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);

    internal void NotifyClosing(CancelEventArgs args) => OnClosing(args);
    internal void NotifyClosed() => OnClosed();
    private void NotifyContentChanged(object? previous, object? current)
    {
        _contentRevision++;
        RaisePropertyChanged(nameof(Content));
        RaisePropertyChanged(ContentProperty, previous, current);
        if (ContentId is null && Content is Control { Name: { } name } && !string.IsNullOrWhiteSpace(name))
            SetCurrentValue(ContentIdProperty, name);
    }

    internal void CompleteActivationNotification(bool previous)
    {
        bool requested = State.IsActive;
        if (previous == requested) return;
        _synchronizingActive = true;
        try
        {
            if (State.IsActive)
            {
                if (Root is LayoutRoot root) root.ActiveContent = this;
                if (State.IsActive) IsSelected = true;
            }
        }
        finally { _synchronizingActive = false; }
        // Native focus can transiently clear the flag. Preserve the requested
        // transition while explicit nested assignments publish their own hooks.
        OnIsActiveChanged(previous, requested);
        RaisePropertyChanged(nameof(IsActive));
    }
    internal void SetLastFocusedDocument(bool value) => SetField(ref _isLastFocusedDocument, value, nameof(IsLastFocusedDocument));
    internal void SetFloating(bool value) => SetFieldChanged(ref _isFloating, value, nameof(IsFloating));
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
        if (Root is not LayoutRoot { Manager: not null })
        {
            ILayoutRoot? root = Root;
            CancelEventArgs args = new();
            NotifyClosing(args);
            if (args.Cancel) return;
            if (this is LayoutAnchorable { IsAutoHidden: true } tool) tool.ToggleAutoHide();
            Parent?.RemoveChild(this);
            root?.CollectGarbage();
            NotifyClosed();
            return;
        }
        if (DockModel.Factory is LayoutFactory layoutFactory) layoutFactory.CloseContent(DockModel);
        else if (DockModel.Factory is { } factory) factory.CloseDockable(DockModel);
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
        if (e.Property == Document.IsSelectedProperty || e.Property == Tool.IsSelectedProperty)
            OnIsSelectedChanged((bool)e.OldValue!, (bool)e.NewValue!);
        else if (e.Property == Document.IsActiveProperty || e.Property == Tool.IsActiveProperty)
        {
            if (_synchronizingActive || _writingActive != 0) return;
            bool previous = (bool)e.OldValue!;
            if ((Root as LayoutRoot)?.Focus.DeferActivationNotification(this, previous) != true)
                CompleteActivationNotification(previous);
            return;
        }
        if (e.Property == DockableBase.IdProperty)
        {
            RaisePropertyChanged(ContentIdProperty, (string?)e.OldValue, (string?)e.NewValue);
            RaisePropertyChanged(nameof(ContentId));
            return;
        }
        if (e.Property == Document.ContentProperty || e.Property == Tool.ContentProperty)
        {
            NotifyContentChanged(e.OldValue, e.NewValue);
            return;
        }
        if (e.Property == DockableBase.DockingStateProperty)
        {
            // Publish the current native value before observers of DockingState
            // can read the facade, including nested docking or XML callbacks.
            bool previous = _isFloating;
            bool floating = (DockModel.DockingState & DockingWindowState.Floating) != 0;
            _isFloating = floating;
            base.OnModelPropertyChanged(sender, e);
            if (previous != floating) RaisePropertyChanged(nameof(IsFloating));
            return;
        }
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockableBase.TitleProperty)
            RaisePropertyChanged(TitleProperty, (string)e.OldValue!, (string)e.NewValue!);
    }
}
