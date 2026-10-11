using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvalonDock.Infrastructure;
using AvalonDock.Themes;

namespace AvalonDock.Controls;

/// <summary>Selects documents and tools using the live Dock-backed item activation commands.</summary>
[TemplatePart("PART_AnchorableListBox", typeof(ListBox))]
[TemplatePart("PART_DocumentListBox", typeof(ListBox))]
public class NavigatorWindow : Window
{
    /// <summary>Identifies the document containers ordered by most recent activation.</summary>
    public static readonly DirectProperty<NavigatorWindow, LayoutDocumentItem[]?> DocumentsProperty =
        AvaloniaProperty.RegisterDirect<NavigatorWindow, LayoutDocumentItem[]?>(nameof(Documents), window => window.Documents);
    /// <summary>Identifies the visible, enabled tool containers in layout order.</summary>
    public static readonly DirectProperty<NavigatorWindow, IEnumerable<LayoutAnchorableItem>?> AnchorablesProperty =
        AvaloniaProperty.RegisterDirect<NavigatorWindow, IEnumerable<LayoutAnchorableItem>?>(nameof(Anchorables), window => window.Anchorables);
    /// <summary>Identifies the selected document container.</summary>
    public static readonly StyledProperty<LayoutDocumentItem?> SelectedDocumentProperty =
        AvaloniaProperty.Register<NavigatorWindow, LayoutDocumentItem?>(nameof(SelectedDocument));
    /// <summary>Identifies the selected tool container.</summary>
    public static readonly StyledProperty<LayoutAnchorableItem?> SelectedAnchorableProperty =
        AvaloniaProperty.Register<NavigatorWindow, LayoutAnchorableItem?>(nameof(SelectedAnchorable));
    /// <summary>Identifies the label above the document list.</summary>
    public static readonly StyledProperty<string> LayoutDocumentsLabelProperty =
        AvaloniaProperty.Register<NavigatorWindow, string>(nameof(LayoutDocumentsLabel), "Active Files");
    /// <summary>Identifies the label above the tool list.</summary>
    public static readonly StyledProperty<string> LayoutAnchorablesLabelProperty =
        AvaloniaProperty.Register<NavigatorWindow, string>(nameof(LayoutAnchorablesLabel), "Active Tool Windows");

    private LayoutDocumentItem[]? _documents;
    private IEnumerable<LayoutAnchorableItem>? _anchorables;
    private ListBox? _documentsList;
    private ListBox? _anchorablesList;
    private INameScope? _templateScope;
    private bool _selectingDocument;
    private bool _settingSelection;
    private readonly LayoutNavigatorOwner _ownerLifetime;
    private bool _closingWithOwner;

    static NavigatorWindow()
    {
        ShowActivatedProperty.OverrideDefaultValue<NavigatorWindow>(false);
        ShowInTaskbarProperty.OverrideDefaultValue<NavigatorWindow>(false);
        KeyDownEvent.AddClassHandler<NavigatorWindow>(static (window, args) => window.OnPreviewKeyDown(args), RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<NavigatorWindow>(static (window, args) => window.OnPreviewKeyUp(args), RoutingStrategies.Tunnel);
    }

    /// <summary>Captures the manager's enabled documents and visible, enabled tools.</summary>
    /// <param name="manager">The manager supplying the native-backed containers.</param>
    public NavigatorWindow(DockingManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        _ownerLifetime = new(this);
        (LayoutDocumentItem[] documents, LayoutAnchorableItem[] tools) = LayoutNavigatorItems.Capture(manager);
        _settingSelection = true;
        try
        {
            SetDocuments(documents);
            SetAnchorables(tools);
            if (documents.Length > 1) Select(true, 1, tools);
            else if (tools.Length > 1) Select(false, 1, tools);
        }
        finally { _settingSelection = false; }
        DataContext = this;
        Styles.Add(new AvalonDockTheme());
        manager.AttachThemeResources(this);
    }

    /// <summary>Preserves the default navigator template for derived controls.</summary>
    protected override Type StyleKeyOverride => typeof(NavigatorWindow);
    /// <summary>Gets the captured document containers, or a replacement supplied by a derived navigator.</summary>
    public LayoutDocumentItem[]? Documents => _documents;
    /// <summary>Gets the captured tool containers, or a replacement supplied by a derived navigator.</summary>
    public IEnumerable<LayoutAnchorableItem>? Anchorables => _anchorables;
    /// <summary>Gets or sets a document; external selection hides the navigator and invokes its available activation command.</summary>
    public LayoutDocumentItem? SelectedDocument { get => GetValue(SelectedDocumentProperty); set => SetValue(SelectedDocumentProperty, value); }
    /// <summary>Gets or sets a tool; external selection closes the navigator and invokes its available activation command.</summary>
    public LayoutAnchorableItem? SelectedAnchorable { get => GetValue(SelectedAnchorableProperty); set => SetValue(SelectedAnchorableProperty, value); }
    /// <summary>Gets or sets the document-list label. The default is Active Files.</summary>
    public string LayoutDocumentsLabel { get => GetValue(LayoutDocumentsLabelProperty); set => SetValue(LayoutDocumentsLabelProperty, value); }
    /// <summary>Gets or sets the tool-list label. The default is Active Tool Windows.</summary>
    public string LayoutAnchorablesLabel { get => GetValue(LayoutAnchorablesLabelProperty); set => SetValue(LayoutAnchorablesLabelProperty, value); }

    /// <summary>Replaces the document-list source.</summary>
    protected void SetDocuments(LayoutDocumentItem[]? value) => SetAndRaise(DocumentsProperty, ref _documents, value);
    /// <summary>Replaces the tool-list source.</summary>
    protected void SetAnchorables(IEnumerable<LayoutAnchorableItem>? value) => SetAndRaise(AnchorablesProperty, ref _anchorables, value);

    /// <summary>Finds the document and tool list parts in the applied Avalonia template.</summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _templateScope = e.NameScope;
        OnApplyTemplate();
    }

    /// <summary>Refreshes navigator template parts; override this AvalonDock hook to customize template initialization.</summary>
    public virtual void OnApplyTemplate()
    {
        _documentsList = _templateScope?.Find<ListBox>("PART_DocumentListBox");
        _anchorablesList = _templateScope?.Find<ListBox>("PART_AnchorableListBox");
    }

    /// <summary>Focuses the initially selected list after pending host input and activation complete.</summary>
    protected override void OnOpened(EventArgs e)
    {
        _ownerLifetime.Attach(Owner as Window);
        base.OnOpened(e);
        Dispatcher.UIThread.Post(FocusSelectedList, DispatcherPriority.Input);
    }

    /// <summary>Releases owner-lifetime tracking when the navigator closes.</summary>
    protected override void OnClosed(EventArgs e)
    {
        _ownerLifetime.Dispose();
        base.OnClosed(e);
    }

    /// <summary>Raises the closing notification and disregards event-handler cancellation after the former owner has closed.</summary>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (_closingWithOwner) e.Cancel = false;
    }

    internal void CloseWithOwner()
    {
        _closingWithOwner = true;
        try { Close(); }
        finally { _closingWithOwner = false; }
    }

    /// <summary>Cycles the current list with Up/Down, crosses lists with Left/Right, and traverses both with Tab.</summary>
    protected virtual void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!LayoutNavigatorSelection.IsNavigationKey(e.Key)) return;
        IReadOnlyList<LayoutAnchorableItem> tools = GetTools();
        int documentCount = Documents?.Length ?? 0;
        int index = _selectingDocument ? IndexOf(Documents, SelectedDocument) : IndexOf(tools, SelectedAnchorable);
        if (!LayoutNavigatorSelection.Move(e.Key, _selectingDocument ? documentCount : tools.Count,
                _selectingDocument ? tools.Count : documentCount, ref index, out bool switchList)) return;
        Select(switchList ? !_selectingDocument : _selectingDocument, index, tools);
        e.Handled = true;
    }

    /// <summary>Closes and activates the selected item when any key other than a navigation key is released.</summary>
    protected virtual void OnPreviewKeyUp(KeyEventArgs e)
    {
        if (LayoutNavigatorSelection.IsNavigationKey(e.Key)) return;
        Close();
        LayoutItem? selected = (LayoutItem?)SelectedDocument ?? SelectedAnchorable;
        if (selected?.ActivateCommand is { } command && command.CanExecute(null))
        {
            command.Execute(null);
            LayoutNavigatorFocus.Restore(selected);
        }
        e.Handled = true;
    }

    /// <summary>Hides the navigator and invokes the currently selected document's available command.</summary>
    protected virtual void OnSelectedDocumentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_settingSelection || SelectedDocument?.ActivateCommand is not { } command || !command.CanExecute(null)) return;
        Hide();
        SelectedDocument?.ActivateCommand?.Execute(null);
    }

    /// <summary>Closes the navigator and invokes the currently selected tool's available command.</summary>
    protected virtual void OnSelectedAnchorableChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_settingSelection || SelectedAnchorable?.ActivateCommand is not { } command || !command.CanExecute(null)) return;
        Close();
        SelectedAnchorable?.ActivateCommand?.Execute(null);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDocumentProperty) OnSelectedDocumentChanged(change);
        else if (change.Property == SelectedAnchorableProperty) OnSelectedAnchorableChanged(change);
    }

    private void Select(bool document, int index, IReadOnlyList<LayoutAnchorableItem> tools)
    {
        bool setting = _settingSelection;
        _settingSelection = true;
        try
        {
            _selectingDocument = document;
            SetCurrentValue(SelectedDocumentProperty, document ? Documents![index] : null);
            SetCurrentValue(SelectedAnchorableProperty, document ? null : tools[index]);
            FocusSelectedList();
        }
        finally { _settingSelection = setting; }
    }

    private void FocusSelectedList()
    {
        if (!IsVisible) return;
        LayoutItem? selected = (LayoutItem?)SelectedDocument ?? SelectedAnchorable;
        ListBox? list = SelectedDocument is not null ? _documentsList : _anchorablesList;
        if (selected is not null && list is not null)
        {
            if (!list.Focus() && list.ContainerFromItem(selected)?.Focus() != true) Focus();
        }
        else Focus();
    }

    private IReadOnlyList<LayoutAnchorableItem> GetTools() => Anchorables switch
    {
        IReadOnlyList<LayoutAnchorableItem> list => list,
        { } items => new List<LayoutAnchorableItem>(items),
        _ => Array.Empty<LayoutAnchorableItem>()
    };

    private static int IndexOf<T>(IReadOnlyList<T>? items, T? selected) where T : class
    {
        if (items is not null && selected is not null)
            for (int index = 0; index < items.Count; index++)
                if (ReferenceEquals(items[index], selected)) return index;
        return -1;
    }
}
