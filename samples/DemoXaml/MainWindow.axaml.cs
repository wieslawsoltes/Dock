using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Serializer;

namespace AvaloniaDemo.Xaml;

public class MainWindow : Window
{
    private readonly IDockSerializer _serializer;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        _serializer = new DockSerializer(typeof(AvaloniaList<>));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void FileOpenLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new() {Name = "Layout", Extensions = {"json"}},
                new() {Name = "All", Extensions = {"*"}}
            },
            AllowMultiple = false
        };
        var window = GetWindow();
        if (window is null)
        {
            return;
        }
        var result = await dlg.ShowAsync(window);
        if (result is { Length: 1 })
        {
            if (!string.IsNullOrEmpty(result[0]))
            {
                var path = result[0];
                var dock = this.FindControl<DockControl>("Dock");
                if (dock is { })
                {
                    await using var stream = File.OpenRead(path);
                    var layout = _serializer.Load<IDock>(stream);
                    dock.Layout = layout;
                }
            }
        }
    }

    private async void FileSaveLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new() {Name = "Layout", Extensions = {"json"}},
                new() {Name = "All", Extensions = {"*"}}
            },
            InitialFileName = "layout",
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
                var dock = this.FindControl<DockControl>("Dock");
                if (dock?.Layout is { })
                {
                    var stream = File.Create(result);
                    _serializer.Save(stream, dock.Layout);
                }
            }
        }
    }

    private void FileCloseLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        var dock = this.FindControl<DockControl>("Dock");
        if (dock is { })
        {
            dock.Layout = null;
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
