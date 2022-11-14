using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace Notepad.ViewModels.Documents;

public class FileViewModel : Document
{
    private string _path = string.Empty;
    private string _text = string.Empty;
    private string _encoding = string.Empty;

    private int _selectionStart = -1;
    private int _selectionEnd = -1;

    public string Path
    {
        get => _path;
        set => this.RaiseAndSetIfChanged(ref _path, value);
    }

    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    public string Encoding
    {
        get => _encoding;
        set => this.RaiseAndSetIfChanged(ref _encoding, value);
    }

    public int SelectionStart
    {
        get => _selectionStart;
        set => this.RaiseAndSetIfChanged(ref _selectionStart, value);
    }

    public int SelectionEnd
    {
        get => _selectionEnd;
        set => this.RaiseAndSetIfChanged(ref _selectionEnd, value);
    }

    public string SelectedText 
    {
        get => _selectionStart >= 0 && _selectionEnd > _selectionStart?
            _text.Substring(_selectionStart, _selectionEnd-_selectionStart)
            : string.Empty;
        set
        {
            if (SelectionStart < 0
            || SelectionEnd < 0 
            || SelectionEnd < SelectionStart)
                return;
            Text = _text.Substring(0, SelectionStart) + value + _text.Substring(SelectionEnd);
            SelectionEnd = SelectionStart + value.Length;
        }
    }
}