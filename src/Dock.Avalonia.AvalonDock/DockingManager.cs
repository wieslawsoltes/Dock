using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Infrastructure;
using CompatibilityTheme = AvalonDock.Themes.Theme;

namespace AvalonDock;

/// <summary>Hosts an AvalonDock-compatible layout using Dock's controls and models.</summary>
public partial class DockingManager : TemplatedControl
{
    /// <summary>Identifies the AvalonDock resource theme applied to this manager and its auxiliary windows.</summary>
    public new static readonly StyledProperty<CompatibilityTheme?> ThemeProperty =
        AvaloniaProperty.Register<DockingManager, CompatibilityTheme?>(nameof(Theme));
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
    /// <summary>Identifies whether a floating caption may fall back to the Windows system menu.</summary>
    public static readonly StyledProperty<bool> ShowSystemMenuProperty =
        AvaloniaProperty.Register<DockingManager, bool>(nameof(ShowSystemMenu), true);
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
            coerce: static (owner, value) => value is null ? owner.GetValue(DocumentHeaderTemplateProperty)
                : owner.GetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty) is null ? value : null);
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
    /// <summary>Identifies the control theme applied to document panes.</summary>
    public static readonly StyledProperty<ControlTheme?> DocumentPaneControlStyleProperty =
        AvaloniaProperty.Register<DockingManager, ControlTheme?>(nameof(DocumentPaneControlStyle));
    /// <summary>Identifies the control theme applied to tool panes.</summary>
    public static readonly StyledProperty<ControlTheme?> AnchorablePaneControlStyleProperty =
        AvaloniaProperty.Register<DockingManager, ControlTheme?>(nameof(AnchorablePaneControlStyle));
    /// <summary>Identifies the public control displayed for the layout's root panel.</summary>
    public static readonly StyledProperty<LayoutPanelControl?> LayoutRootPanelProperty =
        AvaloniaProperty.Register<DockingManager, LayoutPanelControl?>(nameof(LayoutRootPanel));
    /// <summary>Identifies the theme applied to document and tool containers.</summary>
    public static readonly StyledProperty<ControlTheme?> LayoutItemContainerStyleProperty =
        AvaloniaProperty.Register<DockingManager, ControlTheme?>(nameof(LayoutItemContainerStyle));
    /// <summary>Identifies the item container style selector.</summary>
    public static readonly StyledProperty<StyleSelector?> LayoutItemContainerStyleSelectorProperty =
        AvaloniaProperty.Register<DockingManager, StyleSelector?>(nameof(LayoutItemContainerStyleSelector));
    /// <summary>Identifies the layout property, supporting styles and null-to-empty-layout coercion.</summary>
    public static readonly StyledProperty<LayoutRoot> LayoutProperty =
        AvaloniaProperty.Register<DockingManager, LayoutRoot>(nameof(Layout), defaultValue: null!,
            coerce: static (owner, value) => ((DockingManager)owner).CoerceLayout(value));
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
    public static readonly StyledProperty<IEnumerable?> DocumentsSourceProperty =
        AvaloniaProperty.Register<DockingManager, IEnumerable?>(nameof(DocumentsSource));
    /// <summary>Identifies the tool content source.</summary>
    public static readonly StyledProperty<IEnumerable?> AnchorablesSourceProperty =
        AvaloniaProperty.Register<DockingManager, IEnumerable?>(nameof(AnchorablesSource));
    /// <summary>Identifies the insertion strategy.</summary>
    public static readonly StyledProperty<ILayoutUpdateStrategy?> LayoutUpdateStrategyProperty =
        AvaloniaProperty.Register<DockingManager, ILayoutUpdateStrategy?>(nameof(LayoutUpdateStrategy));

    internal LayoutSizingViews SizingViews { get; } = new();
    internal LayoutFloatingWindowCreation FloatingWindowCreation { get; }
    private readonly LayoutFloatingWindowPresenter _floatingWindows = new();
    private readonly LayoutFloatingWindowControls _floatingControls;
    private readonly LayoutSourceBinding<LayoutDocument> _documents;
    private readonly LayoutSourceBinding<LayoutAnchorable> _anchorables;
    private readonly LayoutItemRegistry _layoutItems;
    private readonly LayoutRootEventSubscriptions _layoutRootEvents;
    private readonly LayoutContextMenus _contextMenus = new();
    private readonly ILayoutSystemMenu _systemMenu = new LayoutWindowsSystemMenu();
    private readonly LayoutNavigatorPresenter _navigator = new();
    private readonly LayoutThemeCoordinator _themeResources;
    private int _sourceBindingSuspensions;
    private LayoutPanel? _presentedRootPanel;

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
        _autoHide = new(this);
        _themeResources = new(this);
        DockAdornerWindow.SetFactory(this, new LayoutOverlayWindowFactory(this));
        LayoutHostWindowFactory hosts = new(this);
        DockHostWindowFactory = hosts.Create;
        FloatingWindowCreation = new(this, hosts);
        LayoutRoot layout = CreateDefaultLayout();
        SetValue(LayoutProperty, layout);
        _layoutItems = new LayoutItemRegistry(this);
        _layoutRootEvents = new(this);
        _floatingControls = new(this);
        LayoutSourceSubscriptions subscriptions = new(this);
        _documents = new(this, _layoutItems, static content => new LayoutDocument { Content = content },
            LayoutSourcePlacement.FindDocumentDestination, LayoutSourcePlacement.InsertDocument, subscriptions);
        _anchorables = new(this, _layoutItems, static content => new LayoutAnchorable { Content = content },
            LayoutSourcePlacement.FindAnchorableDestination, LayoutSourcePlacement.InsertAnchorable, subscriptions);
        OnLayoutChanged(null!, layout);
    }

    /// <summary>Preserves the default manager theme for derived controls.</summary>
    protected override Type StyleKeyOverride => typeof(DockingManager);

    /// <summary>Gets or sets AvalonDock resource themes. Avalonia's control template theme remains available through StyledElement.Theme.</summary>
    public new CompatibilityTheme? Theme { get => GetValue(ThemeProperty); set => SetValue(ThemeProperty, value); }

    /// <summary>Gets the typed resource factories for application theme URIs.</summary>
    /// <remarks>Register compiled resource dictionaries before assigning <see cref="Theme"/>.
    /// Registrations belong to this manager and are also used by its floating, overlay, and navigator windows.</remarks>
    public AvalonDock.Themes.ThemeResourceRegistry ThemeResources { get; } = new();

    /// <summary>Applies the callback's new theme to this manager and the current <see cref="Theme"/> to its auxiliary windows.</summary>
    /// <remarks>Overriding or redirecting the callback can leave manager resources different from the current property.
    /// New windows always read the current property, including after a suppressed callback.</remarks>
    protected virtual void OnThemeChanged(AvaloniaPropertyChangedEventArgs e) => _themeResources.Apply(e.NewValue as CompatibilityTheme);

    /// <summary>Gets or sets the AvalonDock layout. Assigning null creates a fresh empty document layout.</summary>
    /// <remarks>The constructor supplies a local layout value. Clear it to allow a style to supply the layout.</remarks>
    [Content]
    [AllowNull]
    public LayoutRoot Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value!);
    }

    private static LayoutRoot CreateDefaultLayout() =>
        new() { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())) };

    private LayoutRoot CoerceLayout(LayoutRoot? value)
    {
        if (value is null) return CreateDefaultLayout();
        if (value.Manager is not null && !ReferenceEquals(value.Manager, this))
            throw new InvalidOperationException("A layout can belong to only one DockingManager.");
        LayoutChanging?.Invoke(this, EventArgs.Empty);
        return value;
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
    /// <summary>Gets or sets whether a native floating caption may show the Windows system menu. Defaults to true.</summary>
    /// <remarks>A configured document or single-pane tool menu takes precedence, including when this is false.
    /// This controls native Windows caption right-clicks; it does not disable application context menus.</remarks>
    public bool ShowSystemMenu { get => GetValue(ShowSystemMenuProperty); set => SetValue(ShowSystemMenuProperty, value); }
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
    /// <summary>Gets or sets the document menu header selector. Explicit templates are suppressed; a null assignment may still inherit the document header template.</summary>
    public DataTemplateSelector? DocumentPaneMenuItemHeaderTemplateSelector { get => GetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty); set => SetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the document list menu icon template, receiving the public layout content.</summary>
    public IDataTemplate? IconContentTemplate { get => GetValue(IconContentTemplateProperty); set => SetValue(IconContentTemplateProperty, value); }
    /// <summary>Gets or sets the icon selector, receiving the current icon and its MenuItemEx container. A null result preserves that icon.</summary>
    public DataTemplateSelector? IconContentTemplateSelector { get => GetValue(IconContentTemplateSelectorProperty); set => SetValue(IconContentTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the template for tool tab and auto-hide headers, receiving the layout content.</summary>
    public IDataTemplate? AnchorableHeaderTemplate { get => GetValue(AnchorableHeaderTemplateProperty); set => SetValue(AnchorableHeaderTemplateProperty, value); }
    /// <summary>Gets or sets the selector that takes precedence over the tool header template.</summary>
    public DataTemplateSelector? AnchorableHeaderTemplateSelector { get => GetValue(AnchorableHeaderTemplateSelectorProperty); set => SetValue(AnchorableHeaderTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the tool title bar template, receiving the selected layout content.</summary>
    public IDataTemplate? AnchorableTitleTemplate { get => GetValue(AnchorableTitleTemplateProperty); set => SetValue(AnchorableTitleTemplateProperty, value); }
    /// <summary>Gets or sets the selector that takes precedence over the tool title bar template.</summary>
    public DataTemplateSelector? AnchorableTitleTemplateSelector { get => GetValue(AnchorableTitleTemplateSelectorProperty); set => SetValue(AnchorableTitleTemplateSelectorProperty, value); }
    /// <summary>Gets or sets the Avalonia control theme for existing and future document panes.</summary>
    public ControlTheme? DocumentPaneControlStyle { get => GetValue(DocumentPaneControlStyleProperty); set => SetValue(DocumentPaneControlStyleProperty, value); }
    /// <summary>Gets or sets the Avalonia control theme for existing and future tool panes.</summary>
    public ControlTheme? AnchorablePaneControlStyle { get => GetValue(AnchorablePaneControlStyleProperty); set => SetValue(AnchorablePaneControlStyleProperty, value); }
    /// <summary>Gets or sets the root panel control displayed by the native layout template.</summary>
    public LayoutPanelControl? LayoutRootPanel { get => GetValue(LayoutRootPanelProperty); set => SetValue(LayoutRootPanelProperty, value); }
    /// <summary>Gets or sets the Avalonia control theme applied to item containers.</summary>
    /// <remarks>When both style providers are null, existing containers retain their last theme.
    /// A non-null selector returning null clears that theme.</remarks>
    public ControlTheme? LayoutItemContainerStyle { get => GetValue(LayoutItemContainerStyleProperty); set => SetValue(LayoutItemContainerStyleProperty, value); }
    /// <summary>Gets or sets the container theme selector used when no explicit style is set.</summary>
    /// <remarks>Selection runs after default commands are cleared and before they are restored.
    /// Clearing this property preserves existing themes when no explicit style is set.</remarks>
    public StyleSelector? LayoutItemContainerStyleSelector { get => GetValue(LayoutItemContainerStyleSelectorProperty); set => SetValue(LayoutItemContainerStyleSelectorProperty, value); }
    /// <summary>Looks up the registered item container without creating one.</summary>
    /// <remarks>Source items are registered after their insertion callbacks. Removed items remain registered until deferred cleanup.</remarks>
    public LayoutItem? GetLayoutItemFromModel(LayoutContent? content)
    {
        return content is null ? null : _layoutItems.Get(content);
    }
    /// <summary>Gets an enumerator over this manager's current Avalonia logical children.</summary>
    /// <remarks>Template visuals are excluded; item containers and explicitly attached logical children are included.</remarks>
    public IEnumerator LogicalChildrenPublic
    {
        get { VerifyAccess(); return LogicalChildren.GetEnumerator(); }
    }
    internal LayoutItem? GetLayoutItemForNativePresentation(LayoutContent content) => _layoutItems.GetForNativePresentation(content);
    /// <summary>Gets the native host factory that preserves this manager's template scope.</summary>
    public Func<IHostWindow?> DockHostWindowFactory { get; }
    /// <summary>Gets or sets the strategy used to insert source items.</summary>
    public ILayoutUpdateStrategy? LayoutUpdateStrategy
    {
        get => GetValue(LayoutUpdateStrategyProperty);
        set => SetValue(LayoutUpdateStrategyProperty, value);
    }
    /// <summary>Gets or sets the document contents, observing collection changes.</summary>
    public IEnumerable? DocumentsSource
    {
        get => GetValue(DocumentsSourceProperty);
        set => SetValue(DocumentsSourceProperty, value);
    }
    /// <summary>Gets or sets the tool contents, observing collection changes.</summary>
    public IEnumerable? AnchorablesSource
    {
        get => GetValue(AnchorablesSourceProperty);
        set => SetValue(AnchorablesSourceProperty, value);
    }
    /// <summary>Transfers layout ownership, containers, native hosts and source contents, then raises <see cref="LayoutChanged"/>.</summary>
    /// <remarks>An override can suppress these side effects by omitting the base call; the property value has already changed.
    /// Ownership, containers and presentation follow the current <see cref="Layout"/> property.
    /// Source removal/import and root notification registrations use the supplied old/new arguments.
    /// New root notifications attach only after imports and presentation complete successfully.
    /// The constructor invokes this hook with a null old layout after creating its internal services.</remarks>
    protected virtual void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
    {
        _layoutRootEvents.Detach(oldLayout);
        _contextMenus.Close();
        _navigator.Close();
        _floatingWindows.Detach(registeredHosts: _floatingControls.Items);
        _autoHide.Detach(hide: true);
        _documents.Detach(oldLayout, DocumentsSource);
        _anchorables.Detach(oldLayout, AnchorablesSource);
        if (oldLayout is not null && ReferenceEquals(oldLayout.Manager, this)) oldLayout.Manager = null;
        _layoutItems.DetachLayout();
        Layout.Manager = this;
        _layoutItems.AttachLayout(Layout);
        _documents.Attach(newLayout, DocumentsSource);
        _anchorables.Attach(newLayout, AnchorablesSource);
        _floatingControls.SetLayout(Layout);
        RaisePropertyChanged(DockFactoryProperty, oldLayout?.DockFactory!, DockFactory);
        RaisePropertyChanged(DockLayoutProperty, (RootDock?)oldLayout?.DockModel!, DockLayout);
        if (IsInitialized) EnsureLayoutRootPanel();
        if (IsInitialized) EnsureAnchorSidePanels();
        if (IsLoaded) _floatingWindows.Attach(Layout);
        if (IsLoaded) _autoHide.Attach(Layout);
        _layoutRootEvents.Attach(newLayout);
        LayoutChanged?.Invoke(this, EventArgs.Empty);
        _layoutItems.RefreshCommands();
    }
    /// <summary>Detaches the document source in <paramref name="e"/>'s old value and imports its new value.</summary>
    /// <remarks>The arguments control attachment even when an override changes the current source before calling base.
    /// Collection notifications update the layout only while their sender is a current source.</remarks>
    protected virtual void OnDocumentsSourceChanged(AvaloniaPropertyChangedEventArgs e)
        => _documents.ChangeSource(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
    /// <summary>Detaches the tool source in <paramref name="e"/>'s old value and imports its new value.</summary>
    /// <remarks>The arguments control attachment even when an override changes the current source before calling base.
    /// Collection notifications update the layout only while their sender is a current source.</remarks>
    protected virtual void OnAnchorablesSourceChanged(AvaloniaPropertyChangedEventArgs e)
        => _anchorables.ChangeSource(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
    /// <summary>Called when the application content template changes.</summary>
    protected virtual void OnLayoutItemTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the application content template selector changes.</summary>
    protected virtual void OnLayoutItemTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the document tab header template changes.</summary>
    protected virtual void OnDocumentHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears a non-null document header template and its local binding when a selector is supplied.</summary>
    protected virtual void OnDocumentHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null && DocumentHeaderTemplate is not null) SetTemplateCallbackValue(DocumentHeaderTemplateProperty, null);
        if (DocumentPaneMenuItemHeaderTemplateSelector is null)
            SetTemplateCallbackValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty, DocumentHeaderTemplateSelector);
    }
    /// <summary>Called when the floating document title template changes.</summary>
    protected virtual void OnDocumentTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears the floating document title template and its local binding when a selector is supplied.</summary>
    protected virtual void OnDocumentTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null) SetTemplateCallbackValue(DocumentTitleTemplateProperty, null);
    }
    /// <summary>Called when the document list menu header template changes.</summary>
    protected virtual void OnDocumentPaneMenuItemHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears a non-null document list menu header template and its local binding when a selector is supplied.</summary>
    protected virtual void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null && DocumentPaneMenuItemHeaderTemplate is not null) SetTemplateCallbackValue(DocumentPaneMenuItemHeaderTemplateProperty, null);
    }
    /// <summary>Called when the tool tab and auto-hide header template changes.</summary>
    protected virtual void OnAnchorableHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears the tool header template and its local binding when a selector is supplied.</summary>
    protected virtual void OnAnchorableHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null) SetTemplateCallbackValue(AnchorableHeaderTemplateProperty, null);
    }
    /// <summary>Called when the tool title bar template changes.</summary>
    protected virtual void OnAnchorableTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Clears a non-null tool title bar template and its local binding when a selector is supplied.</summary>
    protected virtual void OnAnchorableTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null && AnchorableTitleTemplate is not null) SetTemplateCallbackValue(AnchorableTitleTemplateProperty, null);
    }
    private void SetTemplateCallbackValue<T>(StyledProperty<T> property, T value)
    {
        // WPF's selector callbacks replace local template bindings. Avalonia's
        // SetValue preserves them, so detach only a local binding explicitly;
        // style bindings remain available when the local override is cleared.
        if (BindingOperations.GetBindingExpressionBase(this, property) is { } binding &&
            this.GetDiagnostic(property).Priority == BindingPriority.LocalValue)
            binding.Dispose();
        SetValue(property, value);
    }

    /// <summary>Called when the document pane control theme changes.</summary>
    protected virtual void OnDocumentPaneControlStyleChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the tool pane control theme changes.</summary>
    protected virtual void OnAnchorablePaneControlStyleChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called after the displayed root panel control changes.</summary>
    protected virtual void OnLayoutRootPanelChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Reattaches current layout contents and reapplies current container style settings.</summary>
    /// <remarks>The base call reads current manager settings, including when replaying older arguments.
    /// Overrides can omit base to preserve existing containers; new source containers use current settings.
    /// Each successful base call adds a distinct element-event registration on the then-current layout.</remarks>
    protected virtual void OnLayoutItemContainerStyleChanged(AvaloniaPropertyChangedEventArgs e) => _layoutItems.RefreshStyles();
    /// <summary>Reattaches current layout contents and reapplies current container style settings.</summary>
    /// <remarks>The base call reads current manager settings, including when replaying older arguments.
    /// Overrides can omit base to preserve existing containers; new source containers use current settings.
    /// Each successful base call adds a distinct element-event registration on the then-current layout.</remarks>
    protected virtual void OnLayoutItemContainerStyleSelectorChanged(AvaloniaPropertyChangedEventArgs e) => _layoutItems.RefreshStyles();
    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        // The constructor installs its per-instance layout before creating the
        // services which observe it. Subsequent changes use the public hook.
        if (change.Property == LayoutProperty && _layoutItems is not null)
            OnLayoutChanged((LayoutRoot)change.OldValue!, (LayoutRoot)change.NewValue!);
        if (change.Property == ActiveContentProperty) ApplyActiveContentChange(change);
        OnAnchorControlPropertyChanged(change);
        if (change.Property == AutoHideWindowProperty) OnAutoHideWindowChanged(change);
        if (change.Property == AutoHideWindowClosingTimerProperty) OnAutoHideWindowClosingTimerChanged(change);
        if (change.Property == ThemeProperty) OnThemeChanged(change);
        if (change.Property == DocumentPaneControlStyleProperty) OnDocumentPaneControlStyleChanged(change);
        if (change.Property == AnchorablePaneControlStyleProperty) OnAnchorablePaneControlStyleChanged(change);
        if (change.Property == LayoutRootPanelProperty) OnLayoutRootPanelChanged(change);
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
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.DocumentMenu);
        }
        if (change.Property == DocumentTitleTemplateProperty)
        {
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.DocumentTitle);
            OnDocumentTitleTemplateChanged(change);
        }
        if (change.Property == DocumentTitleTemplateSelectorProperty)
        {
            OnDocumentTitleTemplateSelectorChanged(change);
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
            _layoutItems?.RefreshHeaders(LayoutHeaderKind.Document);
        }
        if (change.Property == AnchorableHeaderTemplateSelectorProperty)
        {
            OnAnchorableHeaderTemplateSelectorChanged(change);
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
        EnsureLayoutRootPanel();
        EnsureAnchorSidePanels();
        EnsureAutoHideWindow();
        _autoHide.Attach(Layout);
        _floatingControls.Resume();
        _floatingWindows.Attach(Layout);
    }

    /// <summary>Closes native hosts while preserving their layout contents for reattachment.</summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _contextMenus.Close();
        _navigator.Close();
        _autoHide.Detach(hide: true);
        // Upstream unload closes hosts while preserving content, without
        // making an internal-close request as layout replacement does.
        Dispatcher.UIThread.Post(() =>
        {
            if (VisualRoot is not null) return;
            _floatingWindows.Detach(internalClose: false, registeredHosts: _floatingControls.Items);
            _floatingControls.Suspend();
        }, DispatcherPriority.Loaded);
        base.OnDetachedFromVisualTree(e);
    }

    internal void PresentFloatingWindow(LayoutFloatingWindow window) => _floatingWindows.Present(window);
    internal void AttachThemeResources(StyledElement window) => _themeResources.Attach(window);
    internal void RemoveFloatingWindow(LayoutFloatingWindowControl window) => _floatingControls.Remove(window);
    internal bool TryShowSystemMenu(Window window, PixelPoint position, bool canClose) => _systemMenu.TryShow(window, position, canClose);
    internal void NotifyFloatingWindowControlsChanged(NotifyCollectionChangedEventArgs args) =>
        LayoutFloatingWindowControlCollectionChanged?.Invoke(this, new(args));
    internal void ShowContextMenu(Control target, LayoutItem item, LayoutHeaderKind kind, ContextRequestedEventArgs? request, IDock? pane = null, PixelPoint? screenPosition = null) =>
        _contextMenus.Show(kind == LayoutHeaderKind.Document ? DocumentContextMenu : AnchorableContextMenu,
            target, item, request, pane, screenPosition);
    internal void AttachLayoutItem(LayoutItem item) => LogicalChildren.Add(item);
    internal void DetachLayoutItem(LayoutItem item) => LogicalChildren.Remove(item);
    internal bool AreSourceBindingsSuspended => _sourceBindingSuspensions > 0;
    internal void SuspendSourceBindings() => _sourceBindingSuspensions++;
    internal void ResumeSourceBindings()
    {
        if (_sourceBindingSuspensions > 0) _sourceBindingSuspensions--;
    }

    internal void LayoutRootUpdated() => _layoutItems.RefreshCommands();

    internal void LayoutPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LayoutRoot.ActiveContent)) SynchronizeActiveContent();
        if (e.PropertyName == nameof(LayoutRoot.RootPanel) && IsInitialized) EnsureLayoutRootPanel();
        if (e.PropertyName is nameof(LayoutRoot.LeftSide) or nameof(LayoutRoot.TopSide) or nameof(LayoutRoot.RightSide) or nameof(LayoutRoot.BottomSide)
            && IsInitialized) EnsureAnchorSidePanels();
    }

    internal void EnsureLayoutRootPanel()
    {
        if (ReferenceEquals(_presentedRootPanel, Layout.RootPanel)) return;
        _presentedRootPanel = Layout.RootPanel;
        SetCurrentValue(LayoutRootPanelProperty, new LayoutPanelControl(Layout.RootPanel));
    }
    internal void NotifyDocumentClosing(LayoutDocument document, CancelEventArgs args)
    {
        DocumentClosingEventArgs forwarded = new(document) { Cancel = args.Cancel };
        DocumentClosing?.Invoke(this, forwarded);
        args.Cancel |= forwarded.Cancel;
    }
    internal void NotifyDocumentClosed(LayoutDocument document) => DocumentClosed?.Invoke(this, new DocumentClosedEventArgs(document));
}
