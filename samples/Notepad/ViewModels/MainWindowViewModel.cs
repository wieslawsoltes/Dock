using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Notepad.ViewModels.Documents;
using ReactiveUI;

namespace Notepad.ViewModels;

public class MainWindowViewModel : ReactiveObject, IDropTarget
{
    private readonly IFactory? _factory;
    private IRootDock? _layout;

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
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
        Encoding encoding = GetEncoding(path);
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
        var files = _factory?.GetDockable<IDocumentDock>("Files");
        if (Layout is { } && files is { })
        {
            _factory?.AddDockable(files, fileViewModel);
            _factory?.SetActiveDockable(fileViewModel);
            _factory?.SetFocusedDockable(Layout, fileViewModel);
        }
    }

    private FileViewModel? GetFileViewModel()
    {
        var files = _factory?.GetDockable<IDocumentDock>("Files");
        return files?.ActiveDockable as FileViewModel;
    }

    private FileViewModel GetUntitledFileViewModel()
    {
        return new()
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
        var dlg = new OpenFileDialog();
        dlg.Filters.Add(new FileDialogFilter() { Name = "Text document", Extensions = { "txt" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
        dlg.AllowMultiple = true;
        var result = await dlg.ShowAsync(GetWindow());
        if (result is { Length: > 0 })
        {
            foreach (var path in result)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var untitledFileViewModel = OpenFileViewModel(path);
                    AddFileViewModel(untitledFileViewModel);
                }
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
        var dlg = new SaveFileDialog();
        dlg.Filters.Add(new FileDialogFilter() { Name = "Text document", Extensions = { "txt" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
        dlg.InitialFileName = fileViewModel.Title;
        dlg.DefaultExtension = "txt";
        var result = await dlg.ShowAsync(GetWindow());
        if (result is { })
        {
            if (!string.IsNullOrEmpty(result))
            {
                UpdateFileViewModel(fileViewModel, result);
                SaveFileViewModel(fileViewModel);
            }
        }
    }

    public void FileExit()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    public void DragOver(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.FileNames))
        {
            e.DragEffects = DragDropEffects.None; 
            e.Handled = true;
        }
    }

    public void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.FileNames))
        {
            var result = e.Data.GetFileNames();
            if (result is {})
            {
                foreach (var path in result)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        var untitledFileViewModel = OpenFileViewModel(path);
                        AddFileViewModel(untitledFileViewModel);
                    }
                }
            }
            e.Handled = true;
        }
    }

    private Window? GetWindow()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }
        return null;
    }
}