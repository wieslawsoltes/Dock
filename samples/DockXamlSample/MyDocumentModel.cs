using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DockXamlSample;

public class MyDocumentModel : INotifyPropertyChanged
{
    private string _title = "";
    private string _content = "";
    private string _editableContent = "";
    private string _status = "New";
    private bool _canClose = true;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public string EditableContent
    {
        get => _editableContent;
        set => SetProperty(ref _editableContent, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool CanClose
    {
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
} 