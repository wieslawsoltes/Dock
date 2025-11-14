using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Notepad.ViewModels.Documents;
using Notepad.ViewModels.Tools;

namespace Notepad.ViewModels;

public class MainWindowViewModel : ObservableObject, IDropTarget
{
    private readonly IFactory? _factory;
    private IRootDock? _layout;

    public IRootDock? Layout
    {
        get => _layout;
        set => SetProperty(ref _layout, value);
    }

    public MainWindowViewModel()
    {
        _factory = new NotepadFactory();

        Layout = _factory?.CreateLayout();
        if (Layout is { })
        {
            _factory?.InitLayout(Layout);
        }
    }

    private Encoding GetEncoding(string path)
    {
        using var reader = new StreamReader(path, Encoding.Default, true);
        if (reader.Peek() >= 0)
        {
            reader.Read();
        }
        return reader.CurrentEncoding;
    }

    private FileViewModel OpenFileViewModel(string path)
    {
        var encoding = GetEncoding(path);
        string text = File.ReadAllText(path, encoding);
        string title = Path.GetFileName(path);
        return new FileViewModel()
        {
            Path = path,
            Title = title,
            Text = text,
            Encoding = encoding.WebName
        };
    }

    private void SaveFileViewModel(FileViewModel fileViewModel)
    {
        File.WriteAllText(fileViewModel.Path, fileViewModel.Text, Encoding.GetEncoding(fileViewModel.Encoding));
    }

    private void UpdateFileViewModel(FileViewModel fileViewModel, string path)
    {
        fileViewModel.Path = path;
        fileViewModel.Title = Path.GetFileName(path);
    }

    private void AddFileViewModel(FileViewModel fileViewModel)
    {
        var files = _factory?.GetDockable<IDocumentDock>("Files") as DocumentDock;
        files?.AddDocument(fileViewModel);
    }

    private FileViewModel? GetFileViewModel()
    {
        var files = _factory?.GetDockable<IDocumentDock>("Files");
        return files?.ActiveDockable as FileViewModel;
    }

    private FileViewModel GetUntitledFileViewModel()
    {
        return new FileViewModel
        {
            Path = string.Empty,
            Title = "Untitled",
            Text = "",
            Encoding = Encoding.Default.WebName
        };
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock)
        {
            if (dock.Close.CanExecute(null))
            {
                dock.Close.Execute(null);
            }
        }
    }

    public void FileNew()
    {
        var untitledFileViewModel = GetUntitledFileViewModel();
        AddFileViewModel(untitledFileViewModel);
    }

    public async void FileOpen()
    {
        var window = GetWindow();
        if (window?.StorageProvider is not { } storageProvider)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open file",
            FileTypeFilter =
            [
                new FilePickerFileType("Text document") { Patterns = ["*.txt"] },
                new FilePickerFileType("All") { Patterns = ["*.*"] }
            ],
            AllowMultiple = true
        });

        foreach (var file in result)
        {
            var path = StorageProviderExtensions.TryGetLocalPath(file);
            if (path is not null)
            {
                var untitledFileViewModel = OpenFileViewModel(path);
                AddFileViewModel(untitledFileViewModel);
            }
        }
    }

    public async void FileSave()
    {
        if (GetFileViewModel() is { } fileViewModel)
        {
            if (string.IsNullOrEmpty(fileViewModel.Path))
            {
                await FileSaveAsImpl(fileViewModel);
            }
            else
            {
                SaveFileViewModel(fileViewModel);
            }
        }
    }

    public async void FileSaveAs()
    {
        if (GetFileViewModel() is { } fileViewModel)
        {
            await FileSaveAsImpl(fileViewModel);
        }
    }

    private async Task FileSaveAsImpl(FileViewModel fileViewModel)
    {
        var window = GetWindow();
        if (window?.StorageProvider is not { } storageProvider)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save file",
            FileTypeChoices =
            [
                new FilePickerFileType("Text document") { Patterns = ["*.txt"] },
                new FilePickerFileType("All") { Patterns = ["*.*"] }
            ],
            SuggestedFileName = fileViewModel.Title,
            DefaultExtension = "txt",
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            var path = StorageProviderExtensions.TryGetLocalPath(file);
            if (path is not null)
            {
                UpdateFileViewModel(fileViewModel, path);
                SaveFileViewModel(fileViewModel);
            }
        }
    }

    public void FileExit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    public void EditFind()
    {
        if (_factory?.GetDockable<ITool>("Find") is { } tool && Layout is { })
        {
            _factory.SetActiveDockable(tool);
            _factory.SetFocusedDockable(Layout, tool);
        }
    }

    public void EditReplace()
    {
        if (_factory?.GetDockable<ITool>("Replace") is { } tool && Layout is { })
        {
            _factory.SetActiveDockable(tool);
            _factory.SetFocusedDockable(Layout, tool);
        }
    }

    public void EditWrapLines()
    {
        if (GetFileViewModel() is { } file)
        {
            file.TextWrapping = file.TextWrapping == TextWrapping.NoWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }
    }

    public void EditFindNext()
    {
        if (_factory?.GetDockable<FindViewModel>("Find") is { } find)
        {
            find.FindNext();
        }
    }

    public void EditReplaceNext()
    {
        if (_factory?.GetDockable<ReplaceViewModel>("Replace") is { } replace)
        {
            replace.ReplaceNext();
        }
    }

    public void EditUndo()
    {
        if (GetFileViewModel() is { } file)
        {
            file.Undo();
        }
    }

    public async void EditCut()
    {
        if (GetFileViewModel() is { } file)
        {
            var length = file.SelectionEnd - file.SelectionStart;
            if (length > 0)
            {
                var text = file.Text.Substring(file.SelectionStart, length);
                if (GetWindow()?.Clipboard is { } clipboard)
                {
                    await clipboard.SetTextAsync(text);
                }
                file.Text = file.Text.Remove(file.SelectionStart, length);
                file.SelectionEnd = file.SelectionStart;
                file.CaretIndex = file.SelectionStart;
            }
        }
    }

    public async void EditCopy()
    {
        if (GetFileViewModel() is { } file)
        {
            var length = file.SelectionEnd - file.SelectionStart;
            if (length > 0)
            {
                var text = file.Text.Substring(file.SelectionStart, length);
                if (GetWindow()?.Clipboard is { } clipboard)
                {
                    await clipboard.SetTextAsync(text);
                }
            }
        }
    }

    public async void EditPaste()
    {
        if (GetFileViewModel() is { } file)
        {
            if (GetWindow()?.Clipboard is { } clipboard)
            {
                var text = await ClipboardExtensions.TryGetTextAsync(clipboard);
                if (!string.IsNullOrEmpty(text))
                {
                    var start = file.SelectionStart;
                    var length = file.SelectionEnd - file.SelectionStart;
                    file.Text = file.Text.Remove(start, length).Insert(start, text);
                    file.SelectionStart = start + text.Length;
                    file.SelectionEnd = file.SelectionStart;
                    file.CaretIndex = file.SelectionStart;
                }
            }
        }
    }

    public void EditDelete()
    {
        if (GetFileViewModel() is { } file)
        {
            var length = file.SelectionEnd - file.SelectionStart;
            if (length > 0)
            {
                file.Text = file.Text.Remove(file.SelectionStart, length);
                file.SelectionEnd = file.SelectionStart;
                file.CaretIndex = file.SelectionStart;
            }
        }
    }

    public void EditSelectAll()
    {
        if (GetFileViewModel() is { } file)
        {
            file.SelectionStart = 0;
            file.SelectionEnd = file.Text.Length;
            file.CaretIndex = file.SelectionEnd;
        }
    }

    public void EditTimeDate()
    {
        if (GetFileViewModel() is { } file)
        {
            var insert = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            var start = file.SelectionStart;
            var length = file.SelectionEnd - file.SelectionStart;
            file.Text = file.Text.Remove(start, length).Insert(start, insert);
            file.SelectionStart = start + insert.Length;
            file.SelectionEnd = file.SelectionStart;
            file.CaretIndex = file.SelectionStart;
        }
    }

    public void FormatFont()
    {
        if (GetFileViewModel() is { } file)
        {
            file.FontFamily = file.FontFamily.Name == "Consolas" ? new FontFamily("Segoe UI") : new FontFamily("Consolas");
        }
    }

    public async void HelpGetHelp()
    {
        var url = "https://github.com/wieslawsoltes/Dock";
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            var window = GetWindow();
            if (window is { })
            {
                var dlg = new Window
                {
                    Title = "Help",
                    Width = 300,
                    Height = 100,
                    Content = new TextBlock { Text = url, Margin = new Thickness(20) }
                };
                await dlg.ShowDialog(window);
            }
        }
    }

    public async void HelpAbout()
    {
        var window = GetWindow();
        if (window is null)
        {
            return;
        }
        var dlg = new Window
        {
            Title = "About Notepad",
            Width = 300,
            Height = 150,
            Content = new TextBlock { Text = "Notepad sample using Dock", Margin = new Thickness(20) }
        };
        await dlg.ShowDialog(window);
    }

    public void ViewStatusBar()
    {
        if (GetFileViewModel() is { } file)
        {
            file.ShowStatusBar = !file.ShowStatusBar;
        }
    }

    public void DragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer is null || !e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
        }
    }

    public void Drop(object? sender, DragEventArgs e)
    {
        var storageItems = e.DataTransfer?.TryGetFiles();
        if (storageItems is not null)
        {
            foreach (var file in storageItems)
            {
                var path = file.TryGetLocalPath();
                if (path is not null)
                {
                    var untitledFileViewModel = OpenFileViewModel(path);
                    AddFileViewModel(untitledFileViewModel);
                }
            }
            e.Handled = true;
        }
    }

    private Window? GetWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }
        return null;
    }
}
