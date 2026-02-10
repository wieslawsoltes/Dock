using System;
using System.Collections.ObjectModel;
using System.Reactive;
using DockReactiveUIItemsSourceSample.Models;
using ReactiveUI;

namespace DockReactiveUIItemsSourceSample.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private int _documentCounter = 1;
    private int _toolCounter = 1;
    private string _summary = string.Empty;

    public MainWindowViewModel()
    {
        Documents = new ObservableCollection<DocumentItem>();
        Tools = new ObservableCollection<ToolItem>();

        AddDocumentCommand = ReactiveCommand.Create(AddDocument);
        RemoveDocumentCommand = ReactiveCommand.Create(RemoveDocument, this.WhenAnyValue(x => x.Documents.Count, count => count > 0));
        AddToolCommand = ReactiveCommand.Create(AddTool);
        RemoveToolCommand = ReactiveCommand.Create(RemoveTool, this.WhenAnyValue(x => x.Tools.Count, count => count > 0));
        ClearAllCommand = ReactiveCommand.Create(ClearAll, this.WhenAnyValue(x => x.Documents.Count, x => x.Tools.Count, (docCount, toolCount) => docCount + toolCount > 0));

        Documents.CollectionChanged += (_, _) => UpdateSummary();
        Tools.CollectionChanged += (_, _) => UpdateSummary();

        AddDocument("Welcome", "Welcome to the ReactiveUI ItemsSource sample with custom container generation.", "You can edit this text.");
        AddDocument("Notes", "Closing a generated document removes it from the source collection.", "Try the close button on tabs.");

        AddTool("Explorer", "Source-backed tool generated via ToolDock.ItemsSource and custom container hooks.");
        AddTool("Properties", "Another generated tool. Closing it updates the source collection.");

        UpdateSummary();
    }

    public ObservableCollection<DocumentItem> Documents { get; }

    public ObservableCollection<ToolItem> Tools { get; }

    public ReactiveCommand<Unit, Unit> AddDocumentCommand { get; }

    public ReactiveCommand<Unit, Unit> RemoveDocumentCommand { get; }

    public ReactiveCommand<Unit, Unit> AddToolCommand { get; }

    public ReactiveCommand<Unit, Unit> RemoveToolCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearAllCommand { get; }

    public string Summary
    {
        get => _summary;
        private set => this.RaiseAndSetIfChanged(ref _summary, value);
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

    private void AddDocument(string title, string content, string editableContent)
    {
        Documents.Add(new DocumentItem
        {
            Title = title,
            Content = content,
            EditableContent = editableContent,
            Status = $"Created at {DateTime.Now:HH:mm:ss}",
            CanClose = true
        });

        _documentCounter++;
    }

    private void AddTool(string title, string description)
    {
        Tools.Add(new ToolItem
        {
            Title = title,
            Description = description,
            Status = $"Created at {DateTime.Now:HH:mm:ss}",
            CanClose = true
        });

        _toolCounter++;
    }

    private void UpdateSummary()
    {
        Summary = $"Documents: {Documents.Count} | Tools: {Tools.Count}";
    }
}
