using System;
using System.IO;
using System.Text;
using AvaloniaEdit.Document;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace DockReactiveUIRiderSample.ViewModels.Documents;

public class EditorDocumentViewModel : Document
{
    private readonly Encoding _encoding;
    private int _caretLine;
    private int _caretColumn;

    private EditorDocumentViewModel(string filePath, TextDocument document, Encoding encoding)
    {
        FilePath = filePath;
        Title = Path.GetFileName(filePath);
        Document = document;
        _encoding = encoding;
        _caretLine = 1;
        _caretColumn = 1;

        Document.Changed += (_, _) => IsModified = true;
        IsModified = false;
    }

    public string FilePath { get; }

    public TextDocument Document { get; }

    public string EncodingName => _encoding.WebName;

    public string Extension => Path.GetExtension(FilePath);

    public int CaretLine
    {
        get => _caretLine;
        set => this.RaiseAndSetIfChanged(ref _caretLine, value);
    }

    public int CaretColumn
    {
        get => _caretColumn;
        set => this.RaiseAndSetIfChanged(ref _caretColumn, value);
    }

    public void Save()
    {
        File.WriteAllText(FilePath, Document.Text, _encoding);
        IsModified = false;
    }

    public static EditorDocumentViewModel LoadFromFile(string path)
    {
        var encoding = DetectEncoding(path);
        var text = File.ReadAllText(path, encoding);
        var document = new TextDocument(text);
        return new EditorDocumentViewModel(path, document, encoding);
    }

    private static Encoding DetectEncoding(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8, true);
        if (reader.Peek() >= 0)
        {
            reader.Read();
        }

        return reader.CurrentEncoding;
    }
}
