using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using DockReactiveUIRiderSample.ViewModels;

namespace DockReactiveUIRiderSample.Views;

public partial class MainView : UserControl
{
    private const double MacMenuLeftMargin = 75;

    public MainView()
    {
        InitializeComponent();
        ApplyPlatformInsets();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.Source is Visual source && source.GetSelfAndVisualAncestors().OfType<MenuItem>().Any())
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.BeginMoveDrag(e);
            e.Handled = true;
        }
    }

    private void ApplyPlatformInsets()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        var menu = this.FindControl<Menu>("MainMenu");
        if (menu is not null)
        {
            menu.Margin = new Thickness(MacMenuLeftMargin, 0, 0, 0);
            menu.Padding = new Thickness(0);
        }
    }

    private async void OnOpenSolutionClicked(object? sender, RoutedEventArgs e)
    {
        var storageProvider = (this.GetVisualRoot() as TopLevel)?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open solution",
            FileTypeFilter =
            [
                new FilePickerFileType("Solution") { Patterns = ["*.sln", "*.slnx"] },
                new FilePickerFileType("All") { Patterns = ["*.*"] }
            ],
            AllowMultiple = false
        });

        var file = result.FirstOrDefault();
        if (file is null)
        {
            return;
        }

        var path = StorageProviderExtensions.TryGetLocalPath(file);
        if (path is null)
        {
            return;
        }

        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.LoadSolutionAsync(path);
        }
    }
}
