using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Styling;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Infrastructure;

namespace AvalonDock;

/// <summary>Hosts an AvalonDock-compatible layout using Dock's controls and models.</summary>
public class DockingManager : TemplatedControl
{
    /// <summary>Identifies the bubbling notification before creating a floating window.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> PreviewFloatEvent =
        RoutedEvent.Register<DockingManager, RoutedEventArgs>(nameof(PreviewFloat), RoutingStrategies.Bubble);
    /// <summary>Identifies the bubbling notification after creating a floating window.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> FloatedEvent =
        RoutedEvent.Register<DockingManager, RoutedEventArgs>(nameof(Floated), RoutingStrategies.Bubble);
    /// <summary>Identifies the bubbling notification before docking content.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> PreviewDockEvent =
        RoutedEvent.Register<DockingManager, RoutedEventArgs>(nameof(PreviewDock), RoutingStrategies.Bubble);
    /// <summary>Identifies the bubbling notification after docking content.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> DockedEvent =
        RoutedEvent.Register<DockingManager, RoutedEventArgs>(nameof(Docked), RoutingStrategies.Bubble);
    /// <summary>Identifies the width of splitters between horizontally arranged panes.</summary>
    public static readonly StyledProperty<double> GridSplitterWidthProperty =
        AvaloniaProperty.Register<DockingManager, double>(nameof(GridSplitterWidth), 6);
    /// <summary>Identifies the height of splitters between vertically arranged panes.</summary>
    public static readonly StyledProperty<double> GridSplitterHeightProperty =
        AvaloniaProperty.Register<DockingManager, double>(nameof(GridSplitterHeight), 6);
    /// <summary>Identifies the document context menu.</summary>
    public static readonly StyledProperty<ContextMenu?> DocumentContextMenuProperty =
        AvaloniaProperty.Register<DockingManager, ContextMenu?>(nameof(DocumentContextMenu));
    /// <summary>Identifies the tool context menu.</summary>
    public static readonly StyledProperty<ContextMenu?> AnchorableContextMenuProperty =
        AvaloniaProperty.Register<DockingManager, ContextMenu?>(nameof(AnchorableContextMenu));
    /// <summary>Identifies whether document-group commands may change a populated group's orientation.</summary>
    public static readonly StyledProperty<bool> AllowMixedOrientationProperty =
        AvaloniaProperty.Register<DockingManager, bool>(nameof(AllowMixedOrientation));
    /// <summary>Identifies whether unhandled arrow keys move floating windows.</summary>
    public static readonly StyledProperty<bool> AllowMovingFloatingWindowWithKeyboardProperty =
        AvaloniaProperty.Register<DockingManager, bool>(nameof(AllowMovingFloatingWindowWithKeyboard));
    /// <summary>Identifies the template used to display application content.</summary>
    public static readonly StyledProperty<IDataTemplate?> LayoutItemTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IDataTemplate?>(nameof(LayoutItemTemplate));
    /// <summary>Identifies the application content template selector.</summary>
    public static readonly StyledProperty<DataTemplateSelector?> LayoutItemTemplateSelectorProperty =
        AvaloniaProperty.Register<DockingManager, DataTemplateSelector?>(nameof(LayoutItemTemplateSelector));
    /// <summary>Identifies the document tab header template selector.</summary>
    public static readonly StyledProperty<DataTemplateSelector?> DocumentHeaderTemplateSelectorProperty =
        AvaloniaProperty.Register<DockingManager, DataTemplateSelector?>(nameof(DocumentHeaderTemplateSelector));
    /// <summary>Identifies the template used for document tab headers.</summary>
    public static readonly StyledProperty<IDataTemplate?> DocumentHeaderTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IDataTemplate?>(nameof(DocumentHeaderTemplate),
            coerce: static (owner, value) => owner.GetValue(DocumentHeaderTemplateSelectorProperty) is null ? value : null);
    /// <summary>Identifies the floating document window title template selector.</summary>
    public static readonly StyledProperty<DataTemplateSelector?> DocumentTitleTemplateSelectorProperty =
        AvaloniaProperty.Register<DockingManager, DataTemplateSelector?>(nameof(DocumentTitleTemplateSelector));
    /// <summary>Identifies the floating document window title template.</summary>
    public static readonly StyledProperty<IDataTemplate?> DocumentTitleTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IDataTemplate?>(nameof(DocumentTitleTemplate),
            coerce: static (owner, value) => owner.GetValue(DocumentTitleTemplateSelectorProperty) is null ? value : null);
    /// <summary>Identifies the document list menu header template selector.</summary>
    public static readonly StyledProperty<DataTemplateSelector?> DocumentPaneMenuItemHeaderTemplateSelectorProperty =
        AvaloniaProperty.Register<DockingManager, DataTemplateSelector?>(nameof(DocumentPaneMenuItemHeaderTemplateSelector));
    /// <summary>Identifies the document list menu header template.</summary>
    public static readonly StyledProperty<IDataTemplate?> DocumentPaneMenuItemHeaderTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IDataTemplate?>(nameof(DocumentPaneMenuItemHeaderTemplate),
            coerce: static (owner, value) => owner.GetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty) is null
                ? value ?? owner.GetValue(DocumentHeaderTemplateProperty) : null);
    /// <summary>Identifies the document list menu icon template.</summary>
    public static readonly StyledProperty<IDataTemplate?> IconContentTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IDataTemplate?>(nameof(IconContentTemplate));
    /// <summary>Identifies the document list menu icon template selector.</summary>
    public static readonly StyledProperty<DataTemplateSelector?> IconContentTemplateSelectorProperty =
        AvaloniaProperty.Register<DockingManager, DataTemplateSelector?>(nameof(IconContentTemplateSelector));
    /// <summary>Identifies the tool tab and auto-hide header template selector.</summary>
    public static readonly StyledProperty<DataTemplateSelector?> AnchorableHeaderTemplateSelectorProperty =
        AvaloniaProperty.Register<DockingManager, DataTemplateSelector?>(nameof(AnchorableHeaderTemplateSelector));
    /// <summary>Identifies the template used for tool tab and auto-hide headers.</summary>
    public static readonly StyledProperty<IDataTemplate?> AnchorableHeaderTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IDataTemplate?>(nameof(AnchorableHeaderTemplate),
            coerce: static (owner, value) => owner.GetValue(AnchorableHeaderTemplateSelectorProperty) is null ? value : null);
    /// <summary>Identifies the tool title bar template selector.</summary>
    public static readonly StyledProperty<DataTemplateSelector?> AnchorableTitleTemplateSelectorProperty =
        AvaloniaProperty.Register<DockingManager, DataTemplateSelector?>(nameof(AnchorableTitleTemplateSelector));
    /// <summary>Identifies the template used for the selected tool's title bar.</summary>
    public static readonly StyledProperty<IDataTemplate?> AnchorableTitleTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IDataTemplate?>(nameof(AnchorableTitleTemplate),
            coerce: static (owner, value) => owner.GetValue(AnchorableTitleTemplateSelectorProperty) is null ? value : null);
    /// <summary>Identifies the theme applied to document and tool containers.</summary>
    public static readonly StyledProperty<ControlTheme?> LayoutItemContainerStyleProperty =
        AvaloniaProperty.Register<DockingManager, ControlTheme?>(nameof(LayoutItemContainerStyle));
    /// <summary>Identifies the item container style selector.</summary>
    public static readonly StyledProperty<StyleSelector?> LayoutItemContainerStyleSelectorProperty =
        AvaloniaProperty.Register<DockingManager, StyleSelector?>(nameof(LayoutItemContainerStyleSelector));
    /// <summary>Identifies the layout property.</summary>
    public static readonly DirectProperty<DockingManager, LayoutRoot> LayoutProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, LayoutRoot>(nameof(Layout), x => x.Layout, (x, v) => x.Layout = v);
    /// <summary>Identifies the active content property.</summary>
    public static readonly DirectProperty<DockingManager, object?> ActiveContentProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, object?>(nameof(ActiveContent), x => x.ActiveContent, (x, v) => x.ActiveContent = v, defaultBindingMode: BindingMode.TwoWay);
    /// <summary>Identifies the live root used by the control template.</summary>
    public static readonly DirectProperty<DockingManager, RootDock> DockLayoutProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, RootDock>(nameof(DockLayout), x => x.DockLayout);
    /// <summary>Identifies the factory used by the control template.</summary>
    public static readonly DirectProperty<DockingManager, IFactory> DockFactoryProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, IFactory>(nameof(DockFactory), x => x.DockFactory);
    /// <summary>Identifies the native floating host factory used by the control template.</summary>
    public static readonly DirectProperty<DockingManager, Func<IHostWindow?>> DockHostWindowFactoryProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, Func<IHostWindow?>>(nameof(DockHostWindowFactory), x => x.DockHostWindowFactory);
    /// <summary>Identifies the document content source.</summary>
    public static readonly DirectProperty<DockingManager, IEnumerable?> DocumentsSourceProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, IEnumerable?>(nameof(DocumentsSource), x => x.DocumentsSource, (x, value) => x.DocumentsSource = value);
    /// <summary>Identifies the tool content source.</summary>
    public static readonly DirectProperty<DockingManager, IEnumerable?> AnchorablesSourceProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, IEnumerable?>(nameof(AnchorablesSource), x => x.AnchorablesSource, (x, value) => x.AnchorablesSource = value);
    /// <summary>Identifies the insertion strategy.</summary>
    public static readonly DirectProperty<DockingManager, ILayoutUpdateStrategy?> LayoutUpdateStrategyProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, ILayoutUpdateStrategy?>(nameof(LayoutUpdateStrategy), x => x.LayoutUpdateStrategy, (x, value) => x.LayoutUpdateStrategy = value);

    private LayoutRoot _layout;
    private object? _activeContent;
    internal LayoutSizingViews SizingViews { get; } = new();
    internal LayoutFloatingWindowCreation FloatingWindowCreation { get; }
    private readonly LayoutFloatingWindowPresenter _floatingWindows = new();
    private readonly LayoutFloatingWindowControls _floatingControls;
    private readonly LayoutSourceBinding<LayoutDocument> _documents;
    private readonly LayoutSourceBinding<LayoutAnchorable> _anchorables;
    private readonly LayoutItemRegistry _layoutItems;
    private readonly LayoutContextMenus _contextMenus = new();
    private readonly LayoutNavigatorPresenter _navigator = new();
    private IEnumerable? _documentsSource;
    private IEnumerable? _anchorablesSource;
    private ILayoutUpdateStrategy? _layoutUpdateStrategy;
    private int _sourceBindingSuspensions;

    static DockingManager()
    {
        PreviewFloatEvent.AddClassHandler<DockingManager>(LayoutOperationEventArgs.RestoreSource, handledEventsToo: true);
        FloatedEvent.AddClassHandler<DockingManager>(LayoutOperationEventArgs.RestoreSource, handledEventsToo: true);
        PreviewDockEvent.AddClassHandler<DockingManager>(LayoutOperationEventArgs.RestoreSource, handledEventsToo: true);
        DockedEvent.AddClassHandler<DockingManager>(LayoutOperationEventArgs.RestoreSource, handledEventsToo: true);
        KeyDownEvent.AddClassHandler<DockingManager>(static (manager, args) => manager.OnPreviewKeyDown(args), RoutingStrategies.Tunnel);
    }

    /// <summary>Creates a manager with an empty document layout.</summary>
    public DockingManager()
    {
        LayoutHostWindowFactory hosts = new(this);
        DockHostWindowFactory = hosts.Create;
        FloatingWindowCreation = new(this, hosts);
        _layout = new LayoutRoot { Manager = this };
        _layout.PropertyChanged += LayoutPropertyChanged;
        _layoutItems = new LayoutItemRegistry(this);
        _layoutItems.SetLayout(_layout);
        _floatingControls = new(this);
        _floatingControls.SetLayout(_layout);
        _documents = new(this, static content => new LayoutDocument { Content = content }, LayoutSourcePlacement.InsertDocument);
        _anchorables = new(this, static content => new LayoutAnchorable { Content = content }, LayoutSourcePlacement.InsertAnchorable);
    }

    /// <summary>Preserves the default manager theme for derived controls.</summary>
    protected override Type StyleKeyOverride => typeof(DockingManager);

    /// <summary>Gets or sets the AvalonDock layout.</summary>
    [Content]
    public LayoutRoot Layout
    {
        get => _layout;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_layout, value)) return;
            if (value.Manager is not null && !ReferenceEquals(value.Manager, this))
                throw new InvalidOperationException("A layout can belong to only one DockingManager.");
            LayoutChanging?.Invoke(this, EventArgs.Empty);
            _contextMenus.Close();
            _navigator.Close();
            _floatingWindows.Detach();
            RootDock oldDock = DockLayout;
            LayoutRoot oldLayout = _layout;
            IFactory oldFactory = DockFactory;
            _layout.PropertyChanged -= LayoutPropertyChanged;
            _layout.Manager = null;
            SetAndRaise(LayoutProperty, ref _layout, value);
            _layout.Manager = this;
            _layout.PropertyChanged += LayoutPropertyChanged;
            _layoutItems.SetLayout(_layout);
            _floatingControls.SetLayout(_layout);
            RaisePropertyChanged(DockFactoryProperty, oldFactory, DockFactory);
            RaisePropertyChanged(DockLayoutProperty, oldDock, DockLayout);
            OnLayoutChanged(oldLayout, value);
            SynchronizeActiveContent();
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    /// <summary>Gets the live Dock root for integration and templates.</summary>
    public RootDock DockLayout => (RootDock)Layout.DockModel;
    /// <summary>Gets the factory operating the live layout.</summary>
    public IFactory DockFactory => Layout.DockFactory;
    /// <summary>Gets the native floating-window controls associated with this manager's layout.</summary>
    public IEnumerable<LayoutFloatingWindowControl> FloatingWindows => _floatingControls.Items;
    /// <summary>Creates a navigator over the manager's current document and tool containers.</summary>
    public virtual NavigatorWindow CreateNavigatorWindow() => new(this);

    /// <summary>Presents one navigator, centered over the manager's window when available.</summary>
    protected internal virtual void ShowNavigatorWindow() => _navigator.Show(this);

    internal bool IsNavigatorWindowActive => _navigator.IsActive;

    /// <summary>Opens the navigator on Ctrl+Tab before native child controls process the key.</summary>
    protected virtual void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Tab && (e.KeyModifiers & KeyModifiers.Control) != 0 && !IsNavigatorWindowActive)
        {
            ShowNavigatorWindow();
            e.Handled = true;
        }
    }
    /// <summary>Moves content into a new native floating control without showing the window.</summary>
    /// <param name="contentModel">The document or anchorable to move into the new window.</param>
    /// <param name="isContentImmutable">Whether to prevent replacing the window's loaded content.</param>
    /// <returns>The new control, or null if floating was rejected. Call Show to display it.</returns>
    /// <exception cref="ArgumentNullException">The content model is null.</exception>
    /// <exception cref="InvalidOperationException">The document has no parent pane, or the native locator returns an incompatible host.</exception>
    public LayoutFloatingWindowControl? CreateFloatingWindow(LayoutContent contentModel, bool isContentImmutable) =>
        FloatingWindowCreation.Create(contentModel, isContentImmutable);
    /// <summary>Gets or sets the horizontal split gap in device-independent pixels. The default is six.</summary>
    public double GridSplitterWidth { get => GetValue(GridSplitterWidthProperty); set => SetValue(GridSplitterWidthProperty, value); }
    /// <summary>Gets or sets the vertical split gap in device-independent pixels. The default is six.</summary>
    public double GridSplitterHeight { get => GetValue(GridSplitterHeightProperty); set => SetValue(GridSplitterHeightProperty, value); }
    /// <summary>Gets or sets the document menu, whose data context is the requested item container.</summary>
    public ContextMenu? DocumentContextMenu { get => GetValue(DocumentContextMenuProperty); set => SetValue(DocumentContextMenuProperty, value); }
    /// <summary>Gets or sets the tool menu used by tabs, title bars, and auto-hide strips.</summary>
    public ContextMenu? AnchorableContextMenu { get => GetValue(AnchorableContextMenuProperty); set => SetValue(AnchorableContextMenuProperty, value); }
    /// <summary>Gets or sets whether document-group commands may change a populated group's orientation.</summary>
    public bool AllowMixedOrientation { get => GetValue(AllowMixedOrientationProperty); set => SetValue(AllowMixedOrientationProperty, value); }
    /// <summary>Gets or sets whether unhandled arrow keys move floating windows by 25 device-independent units.</summary>
    /// <remarks>Defaults to false. The setter is public so Avalonia can compile literal XAML assignments.</remarks>
    public bool AllowMovingFloatingWindowWithKeyboard
    {
        get => GetValue(AllowMovingFloatingWindowWithKeyboardProperty);
        set => SetValue(AllowMovingFloatingWindowWithKeyboardProperty, value);
    }
    /// <summary>Gets or sets the template used to display application content.</summary>
    public IDataTemplate? LayoutItemTemplate { get => GetValue(LayoutItemTemplateProperty); set => SetValue(LayoutItemTemplateProperty, value); }
    /// <summary>Gets or sets the content template selector used when no explicit template is set.</summary>
    public DataTemplateSelector? LayoutItemTemplateSelector { get => GetValue(LayoutItemTemplateSelectorProperty); set => SetValue(LayoutItemTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the template for document tab headers, receiving the layout content.</summary>
    public IDataTemplate? DocumentHeaderTemplate { get => GetValue(DocumentHeaderTemplateProperty); set => SetValue(DocumentHeaderTemplateProperty, value); }
    /// <summary>Gets or sets the selector that takes precedence over the document header template.</summary>
    public DataTemplateSelector? DocumentHeaderTemplateSelector { get => GetValue(DocumentHeaderTemplateSelectorProperty); set => SetValue(DocumentHeaderTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the floating window title template, receiving the public root document.</summary>
    public IDataTemplate? DocumentTitleTemplate { get => GetValue(DocumentTitleTemplateProperty); set => SetValue(DocumentTitleTemplateProperty, value); }
    /// <summary>Gets or sets the selector that takes precedence over the floating document title template.</summary>
    public DataTemplateSelector? DocumentTitleTemplateSelector { get => GetValue(DocumentTitleTemplateSelectorProperty); set => SetValue(DocumentTitleTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the document list menu header template, falling back to the document header template.</summary>
    public IDataTemplate? DocumentPaneMenuItemHeaderTemplate { get => GetValue(DocumentPaneMenuItemHeaderTemplateProperty); set => SetValue(DocumentPaneMenuItemHeaderTemplateProperty, value); }
    /// <summary>Gets or sets the selector that takes precedence over the document list menu header template.</summary>
    public DataTemplateSelector? DocumentPaneMenuItemHeaderTemplateSelector { get => GetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty); set => SetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the document list menu icon template, receiving the public layout content.</summary>
    public IDataTemplate? IconContentTemplate { get => GetValue(IconContentTemplateProperty); set => SetValue(IconContentTemplateProperty, value); }
    /// <summary>Gets or sets the icon selector, receiving the public layout content and its menu item.</summary>
    public DataTemplateSelector? IconContentTemplateSelector { get => GetValue(IconContentTemplateSelectorProperty); set => SetValue(IconContentTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the template for tool tab and auto-hide headers, receiving the layout content.</summary>
    public IDataTemplate? AnchorableHeaderTemplate { get => GetValue(AnchorableHeaderTemplateProperty); set => SetValue(AnchorableHeaderTemplateProperty, value); }
    /// <summary>Gets or sets the selector that takes precedence over the tool header template.</summary>
    public DataTemplateSelector? AnchorableHeaderTemplateSelector { get => GetValue(AnchorableHeaderTemplateSelectorProperty); set => SetValue(AnchorableHeaderTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the tool title bar template, receiving the selected layout content.</summary>
    public IDataTemplate? AnchorableTitleTemplate { get => GetValue(AnchorableTitleTemplateProperty); set => SetValue(AnchorableTitleTemplateProperty, value); }
    /// <summary>Gets or sets the selector that takes precedence over the tool title bar template.</summary>
    public DataTemplateSelector? AnchorableTitleTemplateSelector { get => GetValue(AnchorableTitleTemplateSelectorProperty); set => SetValue(AnchorableTitleTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the Avalonia control theme applied to item containers.</summary>
    public ControlTheme? LayoutItemContainerStyle { get => GetValue(LayoutItemContainerStyleProperty); set => SetValue(LayoutItemContainerStyleProperty, value); }
    /// <summary>Gets or sets the container theme selector used when no explicit style is set.</summary>
    public StyleSelector? LayoutItemContainerStyleSelector { get => GetValue(LayoutItemContainerStyleSelectorProperty); set => SetValue(LayoutItemContainerStyleSelectorProperty, value); }
    /// <summary>Gets the stable item container for content in this manager, or null for unattached content.</summary>
    public LayoutItem? GetLayoutItemFromModel(LayoutContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return _layoutItems.Get(content);
    }
    /// <summary>Gets the native host factory that preserves this manager's template scope.</summary>
    public Func<IHostWindow?> DockHostWindowFactory { get; }
    /// <summary>Gets or sets the active document or tool content.</summary>
    public object? ActiveContent
    {
        get => _activeContent;
        set
        {
            if (ReferenceEquals(value, _activeContent)) return;
            if (value is null) Layout.ActiveContent = null;
            else
            {
                foreach (ILayoutElement element in Layout.Descendents())
                    if (element is LayoutContent content && (ReferenceEquals(content.Content, value) || ReferenceEquals(content, value)))
                    {
                        Layout.ActiveContent = content;
                        break;
                    }
            }
            SynchronizeActiveContent();
        }
    }
    /// <summary>Gets or sets the strategy used to insert source items.</summary>
    public ILayoutUpdateStrategy? LayoutUpdateStrategy
    {
        get => _layoutUpdateStrategy;
        set => SetAndRaise(LayoutUpdateStrategyProperty, ref _layoutUpdateStrategy, value);
    }
    /// <summary>Gets or sets the document contents, observing collection changes.</summary>
    public IEnumerable? DocumentsSource
    {
        get => _documentsSource;
        set => SetAndRaise(DocumentsSourceProperty, ref _documentsSource, value);
    }
    /// <summary>Gets or sets the tool contents, observing collection changes.</summary>
    public IEnumerable? AnchorablesSource
    {
        get => _anchorablesSource;
        set => SetAndRaise(AnchorablesSourceProperty, ref _anchorablesSource, value);
    }
    /// <summary>Reattaches source contents to a replacement layout.</summary>
    protected virtual void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
    {
        _documents.SetLayout(newLayout);
        _anchorables.SetLayout(newLayout);
        if (IsLoaded) _floatingWindows.Attach(newLayout);
    }
    /// <summary>Replaces the observed document source.</summary>
    protected virtual void OnDocumentsSourceChanged(AvaloniaPropertyChangedEventArgs e) => _documents.SetSource(DocumentsSource);
    /// <summary>Replaces the observed tool source.</summary>
    protected virtual void OnAnchorablesSourceChanged(AvaloniaPropertyChangedEventArgs e) => _anchorables.SetSource(AnchorablesSource);
    /// <summary>Called when the application content template changes.</summary>
    protected virtual void OnLayoutItemTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the application content template selector changes.</summary>
    protected virtual void OnLayoutItemTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the document tab header template changes.</summary>
    protected virtual void OnDocumentHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears an explicit document header template when a selector is supplied.</summary>
    protected virtual void OnDocumentHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null) SetCurrentValue(DocumentHeaderTemplateProperty, null);
        if (DocumentPaneMenuItemHeaderTemplateSelector is null)
            SetCurrentValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty, DocumentHeaderTemplateSelector);
    }
    /// <summary>Called when the floating document title template changes.</summary>
    protected virtual void OnDocumentTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears an explicit floating document title template when a selector is supplied.</summary>
    protected virtual void OnDocumentTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null) SetCurrentValue(DocumentTitleTemplateProperty, null);
    }
    /// <summary>Called when the document list menu header template changes.</summary>
    protected virtual void OnDocumentPaneMenuItemHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears an explicit document list menu header template when a selector is supplied.</summary>
    protected virtual void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null) SetCurrentValue(DocumentPaneMenuItemHeaderTemplateProperty, null);
    }
    /// <summary>Called when the tool tab and auto-hide header template changes.</summary>
    protected virtual void OnAnchorableHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears an explicit tool header template when a selector is supplied.</summary>
    protected virtual void OnAnchorableHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null) SetCurrentValue(AnchorableHeaderTemplateProperty, null);
    }
    /// <summary>Called when the tool title bar template changes.</summary>
    protected virtual void OnAnchorableTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears an explicit tool title bar template when a selector is supplied.</summary>
    protected virtual void OnAnchorableTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null) SetCurrentValue(AnchorableTitleTemplateProperty, null);
    }
    /// <summary>Applies the new item container style to existing layout content.</summary>
    protected virtual void OnLayoutItemContainerStyleChanged(AvaloniaPropertyChangedEventArgs e) => _layoutItems.RefreshStyles();
    /// <summary>Applies the new item container style selector to existing layout content.</summary>
    protected virtual void OnLayoutItemContainerStyleSelectorChanged(AvaloniaPropertyChangedEventArgs e) => _layoutItems.RefreshStyles();
    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DocumentContextMenuProperty || change.Property == AnchorableContextMenuProperty)
            _contextMenus?.Release(change.OldValue as ContextMenu);
        if (change.Property == DocumentsSourceProperty) OnDocumentsSourceChanged(change);
        if (change.Property == AnchorablesSourceProperty) OnAnchorablesSourceChanged(change);
        if (change.Property == AllowMixedOrientationProperty) _layoutItems?.RefreshCommands();
        if (change.Property == LayoutItemTemplateProperty)
        {
            _layoutItems?.RefreshTemplates();
            OnLayoutItemTemplateChanged(change);
        }
        if (change.Property == LayoutItemTemplateSelectorProperty)
        {
            _layoutItems?.RefreshTemplates();
            OnLayoutItemTemplateSelectorChanged(change);
        }
        if (change.Property == LayoutItemContainerStyleProperty) OnLayoutItemContainerStyleChanged(change);
        if (change.Property == LayoutItemContainerStyleSelectorProperty) OnLayoutItemContainerStyleSelectorChanged(change);
        if (change.Property == DocumentHeaderTemplateProperty)
        {
            CoerceValue(DocumentPaneMenuItemHeaderTemplateProperty);
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.Document);
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.DocumentMenu);
            OnDocumentHeaderTemplateChanged(change);
        }
        if (change.Property == DocumentPaneMenuItemHeaderTemplateProperty)
        {
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.DocumentMenu);
            OnDocumentPaneMenuItemHeaderTemplateChanged(change);
        }
        if (change.Property == DocumentPaneMenuItemHeaderTemplateSelectorProperty)
        {
            OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(change);
            CoerceValue(DocumentPaneMenuItemHeaderTemplateProperty);
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.DocumentMenu);
        }
        if (change.Property == IconContentTemplateProperty || change.Property == IconContentTemplateSelectorProperty)
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.MenuIcon);
        if (change.Property == DocumentTitleTemplateProperty)
        {
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.DocumentTitle);
            OnDocumentTitleTemplateChanged(change);
        }
        if (change.Property == DocumentTitleTemplateSelectorProperty)
        {
            OnDocumentTitleTemplateSelectorChanged(change);
            CoerceValue(DocumentTitleTemplateProperty);
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.DocumentTitle);
        }
        if (change.Property == AnchorableHeaderTemplateProperty)
        {
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.Anchorable);
            OnAnchorableHeaderTemplateChanged(change);
        }
        if (change.Property == DocumentHeaderTemplateSelectorProperty)
        {
            OnDocumentHeaderTemplateSelectorChanged(change);
            CoerceValue(DocumentHeaderTemplateProperty);
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.Document);
        }
        if (change.Property == AnchorableHeaderTemplateSelectorProperty)
        {
            OnAnchorableHeaderTemplateSelectorChanged(change);
            CoerceValue(AnchorableHeaderTemplateProperty);
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.Anchorable);
        }
        if (change.Property == AnchorableTitleTemplateProperty)
        {
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.AnchorableTitle);
            OnAnchorableTitleTemplateChanged(change);
        }
        if (change.Property == AnchorableTitleTemplateSelectorProperty)
        {
            OnAnchorableTitleTemplateSelectorChanged(change);
            CoerceValue(AnchorableTitleTemplateProperty);
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.AnchorableTitle);
        }
    }
    /// <summary>Occurs before replacing the layout.</summary>
    public event EventHandler? LayoutChanging;
    /// <summary>Occurs after replacing the layout.</summary>
    public event EventHandler? LayoutChanged;
    /// <summary>Occurs before a document closes.</summary>
    public event EventHandler<DocumentClosingEventArgs>? DocumentClosing;
    /// <summary>Occurs after a document closes.</summary>
    public event EventHandler<DocumentClosedEventArgs>? DocumentClosed;
    /// <summary>Occurs when active content changes.</summary>
    public event EventHandler? ActiveContentChanged;
    /// <summary>Occurs when a native floating-window control is added or removed.</summary>
    public event EventHandler<LayoutFloatingWindowControlCollectionChangedEventArgs>? LayoutFloatingWindowControlCollectionChanged;

    /// <summary>Occurs before creating a floating window. Source is the public layout content; Handled does not cancel floating.</summary>
    public event EventHandler<RoutedEventArgs> PreviewFloat
    {
        add => AddHandler(PreviewFloatEvent, value);
        remove => RemoveHandler(PreviewFloatEvent, value);
    }
    /// <summary>Occurs after creating a floating window. Source is the public layout content.</summary>
    public event EventHandler<RoutedEventArgs> Floated
    {
        add => AddHandler(FloatedEvent, value);
        remove => RemoveHandler(FloatedEvent, value);
    }
    /// <summary>Occurs before docking content. Source is the public layout content; Handled does not cancel docking.</summary>
    public event EventHandler<RoutedEventArgs> PreviewDock
    {
        add => AddHandler(PreviewDockEvent, value);
        remove => RemoveHandler(PreviewDockEvent, value);
    }
    /// <summary>Occurs after docking content. Source is the public layout content.</summary>
    public event EventHandler<RoutedEventArgs> Docked
    {
        add => AddHandler(DockedEvent, value);
        remove => RemoveHandler(DockedEvent, value);
    }

    /// <summary>Raises the bubbling notification before creating a floating window.</summary>
    protected virtual void RaisePreviewFloatEvent(LayoutContent layoutContent) =>
        RaiseEvent(new LayoutOperationEventArgs(PreviewFloatEvent, layoutContent));
    /// <summary>Raises the bubbling notification after creating a floating window.</summary>
    protected virtual void RaiseFloatedEvent(LayoutContent layoutContent) =>
        RaiseEvent(new LayoutOperationEventArgs(FloatedEvent, layoutContent));
    /// <summary>Raises the bubbling notification before docking content.</summary>
    protected internal virtual void RaisePreviewDockEvent(LayoutContent layoutContent) =>
        RaiseEvent(new LayoutOperationEventArgs(PreviewDockEvent, layoutContent));
    /// <summary>Raises the bubbling notification after docking content.</summary>
    protected internal virtual void RaiseDockedEvent(LayoutContent layoutContent) =>
        RaiseEvent(new LayoutOperationEventArgs(DockedEvent, layoutContent));

    internal void NotifyLayoutOperation(LayoutContent content, bool floating, bool preview)
    {
        if (floating)
        {
            if (preview) RaisePreviewFloatEvent(content);
            else RaiseFloatedEvent(content);
        }
        else
        {
            if (preview) RaisePreviewDockEvent(content);
            else RaiseDockedEvent(content);
        }
    }

    /// <summary>Presents floating layouts after Dock has configured its host factory.</summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _floatingWindows.Attach(Layout);
    }

    /// <summary>Closes native hosts while preserving their layout contents for reattachment.</summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _contextMenus.Close();
        _navigator.Close();
        // Upstream unload closes hosts while preserving content, without
        // making an internal-close request as layout replacement does.
        _floatingWindows.Detach(internalClose: false);
        base.OnDetachedFromVisualTree(e);
    }

    internal void PresentFloatingWindow(LayoutFloatingWindow window) => _floatingWindows.Present(window);
    internal void RemoveFloatingWindow(LayoutFloatingWindowControl window) => _floatingControls.Remove(window);
    internal void NotifyFloatingWindowControlsChanged(NotifyCollectionChangedEventArgs args) =>
        LayoutFloatingWindowControlCollectionChanged?.Invoke(this, new(args));
    internal void ShowContextMenu(Control target, LayoutItem item, LayoutHeaderKind kind, ContextRequestedEventArgs? request, IDock? pane = null) =>
        _contextMenus.Show(kind == LayoutHeaderKind.Document ? DocumentContextMenu : AnchorableContextMenu,
            target, item, request, pane);
    internal void AttachLayoutItem(LayoutItem item) => LogicalChildren.Add(item);
    internal void DetachLayoutItem(LayoutItem item) => LogicalChildren.Remove(item);
    internal bool AreSourceBindingsSuspended => _sourceBindingSuspensions > 0;
    internal void SuspendSourceBindings() => _sourceBindingSuspensions++;
    internal void ResumeSourceBindings()
    {
        if (_sourceBindingSuspensions > 0) _sourceBindingSuspensions--;
    }

    private void LayoutPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LayoutRoot.ActiveContent)) SynchronizeActiveContent();
    }
    private void SynchronizeActiveContent()
    {
        if (SetAndRaise(ActiveContentProperty, ref _activeContent, Layout.ActiveContent?.Content))
            ActiveContentChanged?.Invoke(this, EventArgs.Empty);
    }
    internal void NotifyDocumentClosing(LayoutDocument document, CancelEventArgs args)
    {
        DocumentClosingEventArgs forwarded = new(document) { Cancel = args.Cancel };
        DocumentClosing?.Invoke(this, forwarded);
        args.Cancel |= forwarded.Cancel;
    }
    internal void NotifyDocumentClosed(LayoutDocument document) => DocumentClosed?.Invoke(this, new DocumentClosedEventArgs(document));
}
