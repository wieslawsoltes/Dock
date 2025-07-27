using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DockXamlSample;

public class ItemsSourceExampleViewModel : INotifyPropertyChanged
{
    private int _documentCounter = 1;

    public ItemsSourceExampleViewModel()
    {
        Documents = new ObservableCollection<MyDocumentModel>();
        
        // Add some initial documents
        AddDocument("Welcome", "Welcome to the ItemsSource example!", "This demonstrates automatic document creation from a collection.");
        AddDocument("Documentation", "How to use ItemsSource", "Bind your collection to DocumentDock.ItemsSource and define a DocumentTemplate.");
        
        // Commands
        AddDocumentCommand = new Command(AddNewDocument);
        RemoveDocumentCommand = new Command(RemoveLastDocument, () => Documents.Count > 0);
        ClearAllCommand = new Command(ClearAllDocuments, () => Documents.Count > 0);
        
        // Listen to collection changes to update command states
        Documents.CollectionChanged += (_, args) => 
        {
            System.Diagnostics.Debug.WriteLine($"Documents collection changed: {args.Action}, Count: {Documents.Count}");
            ((Command)RemoveDocumentCommand).RaiseCanExecuteChanged();
            ((Command)ClearAllCommand).RaiseCanExecuteChanged();
        };
        
        System.Diagnostics.Debug.WriteLine($"ItemsSourceExampleViewModel created with {Documents.Count} documents");
        
        // Debug output for each document
        for (int i = 0; i < Documents.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"Document {i}: Title='{Documents[i].Title}', Content='{Documents[i].Content}'");
        }
    }

    public ObservableCollection<MyDocumentModel> Documents { get; }
    
    // Debug property to check if binding is working
    public string DebugInfo => $"ViewModel active, Documents: {Documents.Count}";

    public ICommand AddDocumentCommand { get; }
    public ICommand RemoveDocumentCommand { get; }
    public ICommand ClearAllCommand { get; }

    private void AddNewDocument()
    {
        AddDocument(
            $"Document {_documentCounter}", 
            $"This is automatically generated document #{_documentCounter}",
            $"You can edit this content. Document created at {System.DateTime.Now:HH:mm:ss}"
        );
        _documentCounter++;
    }

    private void AddDocument(string title, string content, string editableContent)
    {
        Documents.Add(new MyDocumentModel
        {
            Title = title,
            Content = content,
            EditableContent = editableContent,
            Status = $"Created at {System.DateTime.Now:HH:mm:ss}",
            CanClose = true
        });
    }

    private void RemoveLastDocument()
    {
        if (Documents.Count > 0)
        {
            Documents.RemoveAt(Documents.Count - 1);
        }
    }

    private void ClearAllDocuments()
    {
        Documents.Clear();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Command : ICommand
{
    private readonly System.Action _execute;
    private readonly System.Func<bool>? _canExecute;

    public Command(System.Action execute, System.Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public event System.EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
} 