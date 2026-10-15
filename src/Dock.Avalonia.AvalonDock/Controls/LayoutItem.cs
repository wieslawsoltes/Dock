using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;
using DockCapability = Dock.Model.Core.DockCapability;

namespace AvalonDock.Controls;

/// <summary>A bindable container for a document or tool in a docking manager.</summary>
/// <remarks>Commands and metadata operate on the existing native Dock model.</remarks>
public abstract class LayoutItem : Control
{
    /// <summary>Identifies the attached layout content.</summary>
    public static readonly DirectProperty<LayoutItem, LayoutContent?> LayoutElementProperty =
        AvaloniaProperty.RegisterDirect<LayoutItem, LayoutContent?>(nameof(LayoutElement), item => item.LayoutElement);
    /// <summary>Identifies the application content.</summary>
    public static readonly DirectProperty<LayoutItem, object?> ModelProperty =
        AvaloniaProperty.RegisterDirect<LayoutItem, object?>(nameof(Model), item => item.Model);
    /// <summary>Identifies the tab title property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<LayoutItem, string?>(name: nameof(Title), defaultValue: null);
    /// <summary>Identifies the persistence identifier property.</summary>
    public static readonly StyledProperty<string?> ContentIdProperty =
        AvaloniaProperty.Register<LayoutItem, string?>(name: nameof(ContentId), defaultValue: null);
    /// <summary>Identifies the tab icon property.</summary>
    public static readonly StyledProperty<IImage?> IconSourceProperty =
        AvaloniaProperty.Register<LayoutItem, IImage?>(name: nameof(IconSource), defaultValue: null);
    /// <summary>Identifies the pane selection state property.</summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<LayoutItem, bool>(name: nameof(IsSelected), defaultValue: false);
    /// <summary>Identifies the active content state property.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<LayoutItem, bool>(name: nameof(IsActive), defaultValue: false);
    /// <summary>Identifies the close capability property.</summary>
    public static readonly StyledProperty<bool> CanCloseProperty =
        AvaloniaProperty.Register<LayoutItem, bool>(name: nameof(CanClose), defaultValue: true);
    /// <summary>Identifies the floating capability property.</summary>
    public static readonly StyledProperty<bool> CanFloatProperty =
        AvaloniaProperty.Register<LayoutItem, bool>(name: nameof(CanFloat), defaultValue: true);
    /// <summary>Identifies the command for closing this content.</summary>
    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(CloseCommand));
    /// <summary>Identifies the command for floating this content.</summary>
    public static readonly StyledProperty<ICommand?> FloatCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(FloatCommand));
    /// <summary>Identifies the command for docking this content as a document.</summary>
    public static readonly StyledProperty<ICommand?> DockAsDocumentCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(DockAsDocumentCommand));
    /// <summary>Identifies the command for closing the other documents.</summary>
    public static readonly StyledProperty<ICommand?> CloseAllButThisCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(CloseAllButThisCommand));
    /// <summary>Identifies the command for closing all documents.</summary>
    public static readonly StyledProperty<ICommand?> CloseAllCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(CloseAllCommand));
    /// <summary>Identifies the command for activating this content.</summary>
    public static readonly StyledProperty<ICommand?> ActivateCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(ActivateCommand));
    /// <summary>Identifies the command for splitting documents into columns.</summary>
    public static readonly StyledProperty<ICommand?> NewVerticalTabGroupCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(NewVerticalTabGroupCommand));
    /// <summary>Identifies the command for splitting documents into rows.</summary>
    public static readonly StyledProperty<ICommand?> NewHorizontalTabGroupCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(NewHorizontalTabGroupCommand));
    /// <summary>Identifies the command for moving to the next document pane.</summary>
    public static readonly StyledProperty<ICommand?> MoveToNextTabGroupCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(MoveToNextTabGroupCommand));
    /// <summary>Identifies the command for moving to the previous document pane.</summary>
    public static readonly StyledProperty<ICommand?> MoveToPreviousTabGroupCommandProperty =
        AvaloniaProperty.Register<LayoutItem, ICommand?>(name: nameof(MoveToPreviousTabGroupCommand));

    private LayoutContent? _layoutElement;
    private object? _model;
    private LayoutItemPresenter? _view;
    private LayoutHeaderViews? _headers;
    private bool _synchronizing;
    private bool _updatingModel;
    private bool _applyingStyle;
    private bool _styleInitialized;
    private ControlTheme? _containerTheme;
    private LayoutItemCommands? _commands;

    /// <summary>Creates an unattached layout container.</summary>
    protected LayoutItem() { }

    /// <summary>Gets the layout content, or null after the container is detached.</summary>
    public LayoutContent? LayoutElement => _layoutElement;
    /// <summary>Gets the application content exposed by the layout model.</summary>
    public object? Model => _model;
    /// <summary>Gets the compiled presenter used by native Dock controls for this content.</summary>
    public ContentPresenter View
    {
        get
        {
            if (_view is null)
            {
                _view = new LayoutItemPresenter { Item = this };
                RefreshViewTemplate();
                LayoutElement?.Root?.Manager?.InternalAddLogicalChild(_view);
            }
            return _view;
        }
    }
    /// <summary>Gets or sets the tab title.</summary>
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    /// <summary>Gets or sets the persistence identifier forwarded to the attached layout content.</summary>
    /// <remarks>The default is null. Model changes do not replace this value; applications can bind it explicitly.</remarks>
    public string? ContentId { get => GetValue(ContentIdProperty); set => SetValue(ContentIdProperty, value); }
    /// <summary>Gets or sets the tab icon.</summary>
    public IImage? IconSource { get => GetValue(IconSourceProperty); set => SetValue(IconSourceProperty, value); }
    /// <summary>Gets or sets the pane selection state.</summary>
    public bool IsSelected { get => GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }
    /// <summary>Gets or sets the active content state.</summary>
    public bool IsActive { get => GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    /// <summary>Gets or sets the close capability.</summary>
    public bool CanClose { get => GetValue(CanCloseProperty); set => SetValue(CanCloseProperty, value); }
    /// <summary>Gets or sets the floating capability.</summary>
    public bool CanFloat { get => GetValue(CanFloatProperty); set => SetValue(CanFloatProperty, value); }
    /// <summary>Gets or sets the command for closing this content.</summary>
    public ICommand? CloseCommand { get => GetValue(CloseCommandProperty); set => SetValue(CloseCommandProperty, value); }
    /// <summary>Gets or sets the command for floating this content.</summary>
    public ICommand? FloatCommand { get => GetValue(FloatCommandProperty); set => SetValue(FloatCommandProperty, value); }
    /// <summary>Gets or sets the command for docking this content as a document.</summary>
    public ICommand? DockAsDocumentCommand { get => GetValue(DockAsDocumentCommandProperty); set => SetValue(DockAsDocumentCommandProperty, value); }
    /// <summary>Gets or sets the command for closing the other documents.</summary>
    public ICommand? CloseAllButThisCommand { get => GetValue(CloseAllButThisCommandProperty); set => SetValue(CloseAllButThisCommandProperty, value); }
    /// <summary>Gets or sets the command for closing all documents.</summary>
    public ICommand? CloseAllCommand { get => GetValue(CloseAllCommandProperty); set => SetValue(CloseAllCommandProperty, value); }
    /// <summary>Gets or sets the command for activating this content.</summary>
    public ICommand? ActivateCommand { get => GetValue(ActivateCommandProperty); set => SetValue(ActivateCommandProperty, value); }
    /// <summary>Gets or sets the command for splitting documents into columns.</summary>
    public ICommand? NewVerticalTabGroupCommand { get => GetValue(NewVerticalTabGroupCommandProperty); set => SetValue(NewVerticalTabGroupCommandProperty, value); }
    /// <summary>Gets or sets the command for splitting documents into rows.</summary>
    public ICommand? NewHorizontalTabGroupCommand { get => GetValue(NewHorizontalTabGroupCommandProperty); set => SetValue(NewHorizontalTabGroupCommandProperty, value); }
    /// <summary>Gets or sets the command for moving to the next document pane.</summary>
    public ICommand? MoveToNextTabGroupCommand { get => GetValue(MoveToNextTabGroupCommandProperty); set => SetValue(MoveToNextTabGroupCommandProperty, value); }
    /// <summary>Gets or sets the command for moving to the previous document pane.</summary>
    public ICommand? MoveToPreviousTabGroupCommand { get => GetValue(MoveToPreviousTabGroupCommandProperty); set => SetValue(MoveToPreviousTabGroupCommandProperty, value); }

    internal DockingManager? Manager { get; private set; }
    internal bool IsAttached => Manager is not null && ReferenceEquals(LayoutElement?.Root, Manager.Layout);
    internal bool IsSynchronizing => _synchronizing;
    internal LayoutItemCommands DefaultCommands => _commands ??= new(this,
        CloseCommandProperty, FloatCommandProperty, DockAsDocumentCommandProperty,
        CloseAllButThisCommandProperty, CloseAllCommandProperty, ActivateCommandProperty,
        NewVerticalTabGroupCommandProperty, NewHorizontalTabGroupCommandProperty,
        MoveToNextTabGroupCommandProperty, MoveToPreviousTabGroupCommandProperty);

    internal void Attach(DockingManager manager, LayoutContent content)
    {
        Manager = manager;
        SetAndRaise(LayoutElementProperty, ref _layoutElement, content);
        DataContext = this;
        SynchronizeModel();
        InitDefaultCommands();
        manager.AttachLayoutItem(this);
        _styleInitialized = true;
        ApplyContainerStyle();
    }

    internal void Detach()
    {
        ClearDefaultBindings();
        ClearDefaultCommands();
        DisposeCommands();
        _styleInitialized = false;
        DockingManager? manager = Manager;
        Manager = null;
        ClearContainerStyle();
        manager?.DetachLayoutItem(this);
        SetAndRaise(LayoutElementProperty, ref _layoutElement, null);
        SetAndRaise(ModelProperty, ref _model, null);
        DataContext = null;
        _headers?.Detach();
        _headers = null;
        if (_view is not null)
        {
            manager?.InternalRemoveLogicalChild(_view);
            _view.Item = null;
            _view.ContentTemplate = null;
            _view.DataTemplates.Clear();
            _view.UpdateChild();
        }
    }

    internal virtual void RefreshCommands() => _commands?.Refresh();
    internal virtual void DisposeCommands() => _commands?.Dispose();

    internal void RequestContextMenu(Control target, ContextRequestedEventArgs request)
    {
        using EventRoute route = BuildEventRoute(Control.ContextRequestedEvent);
        route.RaiseEvent(target, request);
    }

    internal Control BuildHeader(LayoutHeaderKind kind, IDataTemplate? fallback, Control? existing) =>
        (_headers ??= new LayoutHeaderViews(this)).Build(kind, fallback, existing);

    internal void RefreshHeaders(LayoutHeaderKind kind, bool selectorChanged) => _headers?.Refresh(kind, selectorChanged);
    internal void AttachHeader(LayoutHeaderPresenter view) => (_headers ??= new LayoutHeaderViews(this)).Attach(view);

    internal void ApplyContainerStyle()
    {
        if (Manager is not { } manager) return;
        bool applyingStyle = _applyingStyle;
        _applyingStyle = true;
        try
        {
            ClearDefaultBindings();
            if (manager.LayoutItemContainerStyle is { } theme)
                ApplyContainerTheme(theme);
            else if (manager.LayoutItemContainerStyleSelector is { } selector)
                ApplyContainerTheme(selector.SelectStyle(Model, this));
            // With neither a style nor a selector, the existing container style
            // remains applied. A selector returning null explicitly clears it.
        }
        finally { _applyingStyle = applyingStyle; }
        SetDefaultBindings();
    }

    private void ApplyContainerTheme(ControlTheme? theme)
    {
        if (ReferenceEquals(theme, _containerTheme) && ReferenceEquals(theme, Theme)) return;
        if (theme is null)
        {
            ClearContainerStyle();
            return;
        }
        // Let the replacement theme publish its effective values to the model.
        // Clearing under the synchronization guard first can reset a property
        // to the replacement's same value without ever updating the model.
        _containerTheme = theme;
        SetCurrentValue(ThemeProperty, theme);
        ApplyStyling();
    }

    private void ClearContainerStyle()
    {
        if (_containerTheme is null) return;
        bool synchronizing = _synchronizing;
        _synchronizing = true;
        try
        {
            SetCurrentValue(ThemeProperty, null);
            _containerTheme = null;
        }
        finally { _synchronizing = synchronizing; }
    }

    internal void RefreshViewTemplate(bool? selectorChanged = null)
    {
        if (_view is not null && Manager is { } manager && LayoutElement is { } content)
        {
            LayoutTemplateSettings settings = LayoutTemplateSettings.Content(manager);
            if (_view.ContentTemplate is LayoutItemContentTemplate previous)
            {
                settings = previous.Settings.Update(settings, selectorChanged);
                if (previous.Settings.Matches(settings)) return;
            }
            _view.SetCurrentValue(ContentPresenter.ContentTemplateProperty, new LayoutItemContentTemplate(content, manager, _view, settings));
        }
    }

    internal void SynchronizeModel(string? propertyName = null)
    {
        if (_synchronizing || _updatingModel || _applyingStyle || LayoutElement is not { } content) return;
        if (!ReferenceEquals(_model, content.Content))
        {
            _updatingModel = true;
            try
            {
                // Detach bindings for the old content type before publishing
                // the new Model and selecting its container theme.
                ControlTheme? retainedTheme = _containerTheme;
                ClearContainerStyle();
                SetAndRaise(ModelProperty, ref _model, content.Content);
                if (_styleInitialized)
                {
                    if (Manager is { LayoutItemContainerStyle: null, LayoutItemContainerStyleSelector: null })
                        ApplyContainerTheme(retainedTheme);
                    ApplyContainerStyle();
                }
            }
            finally { _updatingModel = false; }
            propertyName = null;
        }
        _synchronizing = true;
        try { UpdateModelProperties(propertyName); }
        finally { _synchronizing = false; }
    }

    internal virtual void UpdateModelProperties(string? propertyName)
    {
        if (LayoutElement is not { } content) return;
        if (propertyName is null or nameof(Title)) SetCurrentValue(TitleProperty, content.Title);
        if (propertyName is null or nameof(IconSource)) SetCurrentValue(IconSourceProperty, content.IconSource);
        if (propertyName is null or nameof(IsSelected)) SetCurrentValue(IsSelectedProperty, content.IsSelected);
        if (propertyName is null or nameof(IsActive)) SetCurrentValue(IsActiveProperty, content.IsActive);
        if (propertyName is null or nameof(CanClose)) SetCurrentValue(CanCloseProperty, content.CanClose);
        if (propertyName is null or nameof(CanFloat)) SetCurrentValue(CanFloatProperty, content.CanFloat);
        if (propertyName is null or nameof(ToolTip)) SetCurrentValue(ToolTip.TipProperty, content.ToolTip);
    }

    /// <summary>Closes the attached content through the docking implementation.</summary>
    protected abstract void Close();
    /// <summary>Provides the upstream no-op floating hook; the default float command does not invoke it.</summary>
    protected virtual void Float() { }
    /// <summary>Determines whether the content can enter a document pane.</summary>
    protected virtual bool CanExecuteDockAsDocumentCommand() =>
        IsAttached && LayoutElement is { } content && content.Parent is not LayoutDocumentPane &&
        LayoutCommandCapabilities.IsEnabled(content, DockCapability.DockAsDocument);

    /// <summary>Creates default command definitions without assigning them or evaluating their predicates.</summary>
    protected virtual void InitDefaultCommands()
    {
        DefaultCommands.Add(CloseCommandProperty, Close, () => IsAttached && LayoutCommandCapabilities.CanClose(LayoutElement!));
        DefaultCommands.Add(FloatCommandProperty, () => LayoutItemOperations.RequireManagedContent(this).Float(), () => IsAttached && LayoutElement is { CanFloat: true } content &&
            content.FindParent<LayoutFloatingWindow>() is null &&
            LayoutCommandCapabilities.IsEnabled(content, DockCapability.Float));
        DefaultCommands.Add(DockAsDocumentCommandProperty, () => LayoutItemOperations.RequireManagedContent(this).DockAsDocument(), CanExecuteDockAsDocumentCommand);
        DefaultCommands.Add(CloseAllButThisCommandProperty, () => LayoutItemOperations.CloseDocuments(this, true),
            () => LayoutItemOperations.HasDocuments(this, true));
        DefaultCommands.Add(CloseAllCommandProperty, () => LayoutItemOperations.CloseDocuments(this, false),
            () => LayoutItemOperations.HasDocuments(this, false));
        DefaultCommands.Add(ActivateCommandProperty, () => LayoutItemOperations.RequireManagedContent(this).IsActive = true, () => IsAttached);
        DefaultCommands.Add(NewVerticalTabGroupCommandProperty, () => LayoutItemOperations.Split(this, Orientation.Horizontal),
            () => LayoutItemOperations.CanSplit(this, Orientation.Horizontal));
        DefaultCommands.Add(NewHorizontalTabGroupCommandProperty, () => LayoutItemOperations.Split(this, Orientation.Vertical),
            () => LayoutItemOperations.CanSplit(this, Orientation.Vertical));
        DefaultCommands.Add(MoveToNextTabGroupCommandProperty, () => LayoutItemOperations.Move(this, 1),
            () => LayoutItemOperations.CanMove(this, 1));
        DefaultCommands.Add(MoveToPreviousTabGroupCommandProperty, () => LayoutItemOperations.Move(this, -1),
            () => LayoutItemOperations.CanMove(this, -1));
    }

    /// <summary>Clears the default command definitions without changing assigned commands.</summary>
    protected virtual void ClearDefaultCommands() => _commands?.ClearDefinitions();
    /// <summary>Removes default commands while preserving application command values.</summary>
    protected virtual void ClearDefaultBindings() => DefaultCommands.Clear();
    /// <summary>Applies default commands and synchronizes model state.</summary>
    protected virtual void SetDefaultBindings()
    {
        DefaultCommands.Apply();
        SynchronizeProperty(IsSelectedProperty, LayoutElement!.IsSelected);
        SynchronizeProperty(IsActiveProperty, LayoutElement!.IsActive);
        SynchronizeProperty(CanCloseProperty, LayoutElement!.CanClose);
        // Native metadata and compiled container themes also synchronize after
        // the upstream command/selection phase has completed.
        SynchronizeModel();
    }

    internal void SynchronizeProperty<T>(StyledProperty<T> property, T value)
    {
        bool synchronizing = _synchronizing;
        _synchronizing = true;
        try { SetCurrentValue(property, value); }
        finally { _synchronizing = synchronizing; }
    }
    /// <summary>Applies a change to the Avalonia visibility property.</summary>
    protected virtual void OnVisibilityChanged()
    {
        if (!_synchronizing && LayoutElement is not null && !IsVisible) LayoutElement.Close();
    }
    /// <summary>Forwards the supplied title value, including null, to the attached content.</summary>
    protected virtual void OnTitleChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_synchronizing && LayoutElement is { } content) content.Title = (string)e.NewValue!;
    }
    /// <summary>Forwards a change to the persistence identifier.</summary>
    protected virtual void OnContentIdChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_synchronizing && LayoutElement is { } content) content.ContentId = (string?)e.NewValue;
    }
    /// <summary>Forwards a change to the tab icon.</summary>
    protected virtual void OnIconSourceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_synchronizing && LayoutElement is { } content) content.IconSource = IconSource;
    }
    /// <summary>Forwards a change to the pane selection state.</summary>
    protected virtual void OnIsSelectedChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_synchronizing && LayoutElement is { } content) content.IsSelected = (bool)e.NewValue!;
    }
    /// <summary>Forwards a change to the active content state.</summary>
    protected virtual void OnIsActiveChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_synchronizing && LayoutElement is { } content) content.IsActive = (bool)e.NewValue!;
    }
    /// <summary>Forwards a change to the close capability.</summary>
    protected virtual void OnCanCloseChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_synchronizing && LayoutElement is { } content) content.CanClose = (bool)e.NewValue!;
    }
    /// <summary>Forwards a change to the floating capability.</summary>
    protected virtual void OnCanFloatChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_synchronizing && LayoutElement is { } content) content.CanFloat = (bool)e.NewValue!;
    }
    /// <summary>Called when the command for closing this content changes.</summary>
    protected virtual void OnCloseCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for floating this content changes.</summary>
    protected virtual void OnFloatCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for docking this content as a document changes.</summary>
    protected virtual void OnDockAsDocumentCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for closing the other documents changes.</summary>
    protected virtual void OnCloseAllButThisCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for closing all documents changes.</summary>
    protected virtual void OnCloseAllCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for activating this content changes.</summary>
    protected virtual void OnActivateCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for splitting documents into columns changes.</summary>
    protected virtual void OnNewVerticalTabGroupCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for splitting documents into rows changes.</summary>
    protected virtual void OnNewHorizontalTabGroupCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for moving to the next document pane changes.</summary>
    protected virtual void OnMoveToNextTabGroupCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the command for moving to the previous document pane changes.</summary>
    protected virtual void OnMoveToPreviousTabGroupCommandChanged(AvaloniaPropertyChangedEventArgs e) { }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty) OnTitleChanged(change);
        else if (change.Property == ContentIdProperty) OnContentIdChanged(change);
        else if (change.Property == IconSourceProperty) OnIconSourceChanged(change);
        else if (change.Property == IsSelectedProperty) OnIsSelectedChanged(change);
        else if (change.Property == IsActiveProperty) OnIsActiveChanged(change);
        else if (change.Property == CanCloseProperty) OnCanCloseChanged(change);
        else if (change.Property == CanFloatProperty) OnCanFloatChanged(change);
        else if (change.Property == CloseCommandProperty) OnCloseCommandChanged(change);
        else if (change.Property == FloatCommandProperty) OnFloatCommandChanged(change);
        else if (change.Property == DockAsDocumentCommandProperty) OnDockAsDocumentCommandChanged(change);
        else if (change.Property == CloseAllButThisCommandProperty) OnCloseAllButThisCommandChanged(change);
        else if (change.Property == CloseAllCommandProperty) OnCloseAllCommandChanged(change);
        else if (change.Property == ActivateCommandProperty) OnActivateCommandChanged(change);
        else if (change.Property == NewVerticalTabGroupCommandProperty) OnNewVerticalTabGroupCommandChanged(change);
        else if (change.Property == NewHorizontalTabGroupCommandProperty) OnNewHorizontalTabGroupCommandChanged(change);
        else if (change.Property == MoveToNextTabGroupCommandProperty) OnMoveToNextTabGroupCommandChanged(change);
        else if (change.Property == MoveToPreviousTabGroupCommandProperty) OnMoveToPreviousTabGroupCommandChanged(change);
        else if (change.Property == IsVisibleProperty) OnVisibilityChanged();
        else if (change.Property == ToolTip.TipProperty && !_synchronizing && LayoutElement is { } content)
            content.ToolTip = GetValue(ToolTip.TipProperty);
    }
}
