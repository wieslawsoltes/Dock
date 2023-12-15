/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Notepad.ViewModels.Documents;

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
        var dlg = new OpenFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new() {Name = "Text document", Extensions = {"txt"}},
                new() {Name = "All", Extensions = {"*"}}
            },
            AllowMultiple = true
        };
        var window = GetWindow();
        if (window is null)
        {
            return;
        }
        var result = await dlg.ShowAsync(window);
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
        var dlg = new SaveFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new() {Name = "Text document", Extensions = {"txt"}},
                new() {Name = "All", Extensions = {"*"}}
            },
            InitialFileName = fileViewModel.Title,
            DefaultExtension = "txt"
        };
        var window = GetWindow();
        if (window is null)
        {
            return;
        }
        var result = await dlg.ShowAsync(window);
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
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    public void DragOver(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.None; 
            e.Handled = true;
        }
    }

    public void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
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
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }
        return null;
    }
}
