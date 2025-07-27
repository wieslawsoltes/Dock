using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Serializer;

namespace DockXamlSample;

public partial class MainView : UserControl
{
    private IDockSerializer? _serializer;
    private IDockState? _dockState;
    private IRootDock? _rootDock;
    private MenuItem? _viewsMenu;

    public MainView()
    {
        InitializeComponent();
        InitializeDockState();
    }

    private void InitializeDockState()
    {
        _serializer = new DockSerializer(typeof(AvaloniaList<>));
        // TODO:
        // _serializer = new AvaloniaDockSerializer();
        _dockState = new DockState();

        var layout = DockControl?.Layout;
        if (layout != null)
        {
            _dockState.Save(layout);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _viewsMenu = ViewsMenu;
        _rootDock = DockControl?.Layout as IRootDock;

        if (DockControl?.Factory is { } factory)
        {
            factory.DockableHidden += (_, _) => UpdateViewsMenu();
            factory.DockableRestored += (_, _) => UpdateViewsMenu();
        }

        UpdateViewsMenu();
    }

    private async Task OpenLayout()
    {
        if (_serializer is null || _dockState is null)
        {
            return;
        }

        var storageProvider = (this.GetVisualRoot() as TopLevel)?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open layout",
            FileTypeFilter = [new FilePickerFileType("Json") { Patterns = ["*.json"] }, new FilePickerFileType("All") { Patterns = ["*.*"] }],
            AllowMultiple = false
        });

        var file = result.FirstOrDefault();

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                if (DockControl is not null)
                {
                    var layout = _serializer.Load<IDock?>(stream);
                    // TODO:
                    // var layout = await JsonSerializer.DeserializeAsync(
                    //     stream, 
                    //     AvaloniaDockSerializer.s_serializerContext.RootDock);
                    if (layout is { })
                    {
                        DockControl.Layout = layout;
                        DockControl.Factory?.InitLayout(layout);
                        _dockState.Restore(layout);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task SaveLayout()
    {
        if (_serializer is null || _dockState is null)
        {
            return;
        }

        var storageProvider = (this.GetVisualRoot() as TopLevel)?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save layout",
            FileTypeChoices = [new FilePickerFileType("Json") { Patterns = ["*.json"] }, new FilePickerFileType("All") { Patterns = ["*.*"] }],
            SuggestedFileName = "layout",
            DefaultExtension = "json",
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                if (DockControl?.Layout is not null)
                {
                    _serializer.Save(stream, DockControl.Layout);
                    // TODO:
                    // await JsonSerializer.SerializeAsync(
                    //     stream, 
                    //     (RootDock)dock.Layout, AvaloniaDockSerializer.s_serializerContext.RootDock);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private void CloseLayout()
    {
        if (DockControl is not null)
        {
            DockControl.Layout = null;
        }
    }

    private async void FileOpenLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        await OpenLayout();
    }

    private async void FileSaveLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        await SaveLayout();
    }

    private void FileCloseLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        CloseLayout();
    }

    private void FileItemsSourceExample_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new ItemsSourceExampleWindow();
        window.Show();
    }

    private void ViewsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: IDockable dockable } && DockControl?.Factory is { } factory)
        {
            factory.RestoreDockable(dockable);
        }
    }

    private void UpdateViewsMenu()
    {
        if (_viewsMenu is null)
        {
            return;
        }

        _viewsMenu.Items.Clear();

        if (_rootDock?.HiddenDockables is { Count: >0 } hidden)
        {
            foreach (var dockable in hidden)
            {
                var item = new MenuItem { Header = dockable.Title ?? dockable.Id, Tag = dockable };
                item.Click += ViewsMenuItem_OnClick;
                _viewsMenu.Items.Add(item);
            }
            _viewsMenu.IsEnabled = true;
        }
        else
        {
            _viewsMenu.IsEnabled = false;
        }
    }
}

