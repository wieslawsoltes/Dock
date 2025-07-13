using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using DockReactiveUIDiSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUIDiSample.Views;

public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
{

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private MainWindowViewModel? _viewModel;
    public MainWindowViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            DataContext = value;
        }
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainWindowViewModel?)value;
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

