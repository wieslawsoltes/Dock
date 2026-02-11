using System;
using System.Collections.ObjectModel;
using System.Reactive;
using DockXamlReactiveUISample.Models;
using Dock.Model.Core;
using Dock.Settings;
using ReactiveUI;

namespace DockXamlReactiveUISample.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private int _documentCounter = 1;
    private int _toolCounter = 1;
    private string _summary = string.Empty;
    private bool _updateItemsSourceOnUnregister;
    private bool? _documentCanUpdateItemsSourceOnUnregister;
    private bool? _toolCanUpdateItemsSourceOnUnregister;
    private string _documentContainerLookup = "Not checked";
    private string _toolContainerLookup = "Not checked";
    private bool? _firstDocumentCloseOverride;
    private bool? _firstDocumentFloatOverride;
    private bool? _firstToolCloseOverride;
    private bool? _firstToolFloatOverride;

    public MainWindowViewModel(IFactory factory)
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        RootDockCapabilityPolicy = new DockCapabilityPolicy();
        DocumentDockCapabilityPolicy = new DockCapabilityPolicy();
        ToolDockCapabilityPolicy = new DockCapabilityPolicy();
        RootCapabilities = new CapabilityPolicyEditor("Root Dock", RootDockCapabilityPolicy);
        DocumentDockCapabilities = new CapabilityPolicyEditor("Document Dock", DocumentDockCapabilityPolicy);
        ToolDockCapabilities = new CapabilityPolicyEditor("Tool Dock", ToolDockCapabilityPolicy);

        _updateItemsSourceOnUnregister = DockSettings.UpdateItemsSourceOnUnregister;

        Documents = new ObservableCollection<DocumentItem>();
        Tools = new ObservableCollection<ToolItem>();

        AddDocumentCommand = ReactiveCommand.Create(AddDocument);
        RemoveDocumentCommand = ReactiveCommand.Create(RemoveDocument, this.WhenAnyValue(x => x.Documents.Count, count => count > 0));
        AddToolCommand = ReactiveCommand.Create(AddTool);
        RemoveToolCommand = ReactiveCommand.Create(RemoveTool, this.WhenAnyValue(x => x.Tools.Count, count => count > 0));
        ClearAllCommand = ReactiveCommand.Create(ClearAll, this.WhenAnyValue(x => x.Documents.Count, x => x.Tools.Count, (docCount, toolCount) => docCount + toolCount > 0));
        LookupDocumentContainerCommand = ReactiveCommand.Create(LookupFirstDocumentContainer, this.WhenAnyValue(x => x.Documents.Count, count => count > 0));
        LookupToolContainerCommand = ReactiveCommand.Create(LookupFirstToolContainer, this.WhenAnyValue(x => x.Tools.Count, count => count > 0));

        Documents.CollectionChanged += (_, _) =>
        {
            UpdateSummary();
            ApplyFirstDocumentOverrides();
        };
        Tools.CollectionChanged += (_, _) =>
        {
            UpdateSummary();
            ApplyFirstToolOverrides();
        };

        AddDocument("Welcome", "Welcome to the ReactiveUI ItemsSource sample with custom container generation, per-dock container themes, and template selectors.", "You can edit this text.");
        AddDocument("Notes", "Closing a generated document removes it from the source collection. Only the first generated item in each dock uses the selector template override.", "Try the close button on tabs.");

        AddTool("Explorer", "Source-backed tool generated via ToolDock.ItemsSource with custom generator, selector template, and per-dock theme metadata.");
        AddTool("Properties", "Another generated tool. Closing it updates the source collection.");

        UpdateSummary();
    }

    public IFactory Factory { get; }

    public DockCapabilityPolicy RootDockCapabilityPolicy { get; }

    public DockCapabilityPolicy DocumentDockCapabilityPolicy { get; }

    public DockCapabilityPolicy ToolDockCapabilityPolicy { get; }

    public CapabilityPolicyEditor RootCapabilities { get; }

    public CapabilityPolicyEditor DocumentDockCapabilities { get; }

    public CapabilityPolicyEditor ToolDockCapabilities { get; }

    public ObservableCollection<DocumentItem> Documents { get; }

    public ObservableCollection<ToolItem> Tools { get; }

    public ReactiveCommand<Unit, Unit> AddDocumentCommand { get; }

    public ReactiveCommand<Unit, Unit> RemoveDocumentCommand { get; }

    public ReactiveCommand<Unit, Unit> AddToolCommand { get; }

    public ReactiveCommand<Unit, Unit> RemoveToolCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearAllCommand { get; }

    public ReactiveCommand<Unit, Unit> LookupDocumentContainerCommand { get; }

    public ReactiveCommand<Unit, Unit> LookupToolContainerCommand { get; }

    public string Summary
    {
        get => _summary;
        private set => this.RaiseAndSetIfChanged(ref _summary, value);
    }

    public bool UpdateItemsSourceOnUnregister
    {
        get => _updateItemsSourceOnUnregister;
        set
        {
            if (_updateItemsSourceOnUnregister == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _updateItemsSourceOnUnregister, value);
            DockSettings.UpdateItemsSourceOnUnregister = value;
        }
    }

    public bool? DocumentCanUpdateItemsSourceOnUnregister
    {
        get => _documentCanUpdateItemsSourceOnUnregister;
        set => this.RaiseAndSetIfChanged(ref _documentCanUpdateItemsSourceOnUnregister, value);
    }

    public bool? ToolCanUpdateItemsSourceOnUnregister
    {
        get => _toolCanUpdateItemsSourceOnUnregister;
        set => this.RaiseAndSetIfChanged(ref _toolCanUpdateItemsSourceOnUnregister, value);
    }

    public string DocumentContainerLookup
    {
        get => _documentContainerLookup;
        private set => this.RaiseAndSetIfChanged(ref _documentContainerLookup, value);
    }

    public string ToolContainerLookup
    {
        get => _toolContainerLookup;
        private set => this.RaiseAndSetIfChanged(ref _toolContainerLookup, value);
    }

    public bool? FirstDocumentCloseOverride
    {
        get => _firstDocumentCloseOverride;
        set
        {
            if (_firstDocumentCloseOverride == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _firstDocumentCloseOverride, value);
            ApplyFirstDocumentOverrides();
        }
    }

    public bool? FirstDocumentFloatOverride
    {
        get => _firstDocumentFloatOverride;
        set
        {
            if (_firstDocumentFloatOverride == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _firstDocumentFloatOverride, value);
            ApplyFirstDocumentOverrides();
        }
    }

    public bool? FirstToolCloseOverride
    {
        get => _firstToolCloseOverride;
        set
        {
            if (_firstToolCloseOverride == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _firstToolCloseOverride, value);
            ApplyFirstToolOverrides();
        }
    }

    public bool? FirstToolFloatOverride
    {
        get => _firstToolFloatOverride;
        set
        {
            if (_firstToolFloatOverride == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _firstToolFloatOverride, value);
            ApplyFirstToolOverrides();
        }
    }

    private void AddDocument()
    {
        var index = _documentCounter;
        AddDocument(
            $"Document {index}",
            $"Generated document #{index} created via source collection.",
            $"Editable content for document #{index}.");
    }

    private void RemoveDocument()
    {
        if (Documents.Count > 0)
        {
            Documents.RemoveAt(Documents.Count - 1);
        }
    }

    private void AddTool()
    {
        var index = _toolCounter;
        AddTool(
            $"Tool {index}",
            $"Generated tool #{index} created via source collection.");
    }

    private void RemoveTool()
    {
        if (Tools.Count > 0)
        {
            Tools.RemoveAt(Tools.Count - 1);
        }
    }

    private void ClearAll()
    {
        Documents.Clear();
        Tools.Clear();
    }

    private void LookupFirstDocumentContainer()
    {
        if (Documents.Count == 0)
        {
            DocumentContainerLookup = "No source items.";
            return;
        }

        var sourceItem = Documents[0];
        var container = Factory.GetContainerFromItem(sourceItem);
        DocumentContainerLookup = container is null
            ? "No tracked container."
            : $"{container.GetType().Name}: {container.Title}";
    }

    private void LookupFirstToolContainer()
    {
        if (Tools.Count == 0)
        {
            ToolContainerLookup = "No source items.";
            return;
        }

        var sourceItem = Tools[0];
        var container = Factory.GetContainerFromItem(sourceItem);
        ToolContainerLookup = container is null
            ? "No tracked container."
            : $"{container.GetType().Name}: {container.Title}";
    }

    private void ApplyFirstDocumentOverrides()
    {
        if (Documents.Count == 0)
        {
            return;
        }

        var first = Documents[0];
        first.CloseOverride = FirstDocumentCloseOverride;
        first.FloatOverride = FirstDocumentFloatOverride;
        ApplyOverridesToContainer(first, first.CloseOverride, first.FloatOverride);
    }

    private void ApplyFirstToolOverrides()
    {
        if (Tools.Count == 0)
        {
            return;
        }

        var first = Tools[0];
        first.CloseOverride = FirstToolCloseOverride;
        first.FloatOverride = FirstToolFloatOverride;
        ApplyOverridesToContainer(first, first.CloseOverride, first.FloatOverride);
    }

    private void ApplyOverridesToContainer(object sourceItem, bool? canClose, bool? canFloat)
    {
        var container = Factory.GetContainerFromItem(sourceItem);
        if (container is null)
        {
            return;
        }

        var overrides = container.DockCapabilityOverrides ?? new DockCapabilityOverrides();
        overrides.CanClose = canClose;
        overrides.CanFloat = canFloat;

        container.DockCapabilityOverrides = overrides.HasAnyOverride ? overrides : null;
    }

    private void AddDocument(string title, string content, string editableContent)
    {
        var item = new DocumentItem
        {
            Title = title,
            Content = content,
            EditableContent = editableContent,
            Status = $"Created at {DateTime.Now:HH:mm:ss}",
            CanClose = true
        };

        if (Documents.Count == 0)
        {
            item.CloseOverride = FirstDocumentCloseOverride;
            item.FloatOverride = FirstDocumentFloatOverride;
        }

        Documents.Add(item);

        _documentCounter++;
    }

    private void AddTool(string title, string description)
    {
        var item = new ToolItem
        {
            Title = title,
            Description = description,
            Status = $"Created at {DateTime.Now:HH:mm:ss}",
            CanClose = true
        };

        if (Tools.Count == 0)
        {
            item.CloseOverride = FirstToolCloseOverride;
            item.FloatOverride = FirstToolFloatOverride;
        }

        Tools.Add(item);

        _toolCounter++;
    }

    private void UpdateSummary()
    {
        Summary = $"Documents: {Documents.Count} | Tools: {Tools.Count}";
    }
}
