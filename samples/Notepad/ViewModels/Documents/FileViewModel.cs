using System.Collections.Generic;
using Avalonia.Media;
using Dock.Model.Mvvm.Controls;

namespace Notepad.ViewModels.Documents;

public class FileViewModel : Document
{
    private string _path = string.Empty;
    private string _text = string.Empty;
    private string _encoding = string.Empty;
    private int _selectionStart;
    private int _selectionEnd;
    private int _caretIndex;
    private TextWrapping _textWrapping = TextWrapping.NoWrap;
    private bool _showStatusBar = true;
    private readonly Stack<string> _undoStack = new();
    private FontFamily _fontFamily = new("Consolas");

    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    public string Text
    {
        get => _text;
        set
        {
            if (value != _text)
            {
                _undoStack.Push(_text);
                SetProperty(ref _text, value);
                IsModified = true;
            }
        }
    }

    public string Encoding
    {
        get => _encoding;
        set => SetProperty(ref _encoding, value);
    }

    public int SelectionStart
    {
        get => _selectionStart;
        set => SetProperty(ref _selectionStart, value);
    }

    public int SelectionEnd
    {
        get => _selectionEnd;
        set => SetProperty(ref _selectionEnd, value);
    }

    public int CaretIndex
    {
        get => _caretIndex;
        set => SetProperty(ref _caretIndex, value);
    }

    public TextWrapping TextWrapping
    {
        get => _textWrapping;
        set => SetProperty(ref _textWrapping, value);
    }

    public bool ShowStatusBar
    {
        get => _showStatusBar;
        set => SetProperty(ref _showStatusBar, value);
    }

    public FontFamily FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            Text = _undoStack.Pop();
        }
    }
}
