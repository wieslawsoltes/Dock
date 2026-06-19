using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using DockQuickStartSample.Models;
using ReactiveUI;

namespace DockQuickStartSample.ViewModels;

public sealed class MainWindowViewModel : ReactiveObject
{
    private int _nextDocumentNumber = 3;

    public MainWindowViewModel()
    {
        Documents = new ObservableCollection<FileDocument>
        {
            new()
            {
                Title = "Document 1.txt",
                Content = "This document is generated from MainWindowViewModel.Documents."
            },
            new()
            {
                Title = "Document 2.txt",
                Content = "Edit this text. The tab is backed by a FileDocument model."
            }
        };

        Documents.CollectionChanged += OnDocumentsChanged;
        AddDocumentCommand = ReactiveCommand.Create(AddDocument);
        ResetDocumentsCommand = ReactiveCommand.Create(ResetDocuments);
    }

    public ObservableCollection<FileDocument> Documents { get; }

    public ReactiveCommand<Unit, Unit> AddDocumentCommand { get; }

    public ReactiveCommand<Unit, Unit> ResetDocumentsCommand { get; }

    public string Status => $"{Documents.Count} document(s)";

    private void AddDocument()
    {
        var documentNumber = _nextDocumentNumber++;
        Documents.Add(new FileDocument
        {
            Title = $"Document {documentNumber}.txt",
            Content = $"New document {documentNumber} created from the Add Document command."
        });
    }

    private void ResetDocuments()
    {
        Documents.Clear();
        _nextDocumentNumber = 1;
        AddDocument();
        AddDocument();
    }

    private void OnDocumentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(Status));
    }
}
