using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using DockReactiveUIDiSample.ViewModels;
using Avalonia.ReactiveUI;

namespace DockReactiveUIDiSample.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void FileLoadLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.LoadLayout();
    }

    private async void FileSaveLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { })
        {
            await ViewModel.SaveLayoutAsync();
        }
    }

    private void FileCloseLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CloseLayout();
    }
}

