using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels;
using ReactiveUI.Avalonia.Reactive;

namespace DockFigmaSample.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
