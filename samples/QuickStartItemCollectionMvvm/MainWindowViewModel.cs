using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QuickStartMultiDocumentMvvm;

public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>
    ///     The set of documents
    /// </summary>
    [ObservableProperty] private ObservableCollection<FileDocument> _documents = [];


    private int _index;

    /// <summary>
    ///     Factory used to provide AddDocument when + pressed on tab bar
    /// </summary>
    [ObservableProperty] private DockFactory _dockFactory;
    public MainWindowViewModel()
    {
        DockFactory = new DockFactory(AddDocument);
        //add a couple of docs to start
        AddDocument();
        AddDocument();
    }

    private void AddDocument()
    {
        var doc = new FileDocument
        {
            Title = $"New {_index}",
            Content = $"This is document {_index} created at {DateTime.Now.ToShortTimeString()}"
        };
        _index++;
        Documents.Add(doc);
    }

    [RelayCommand]
    public void ClearAll()
        => Documents.Clear();
}
