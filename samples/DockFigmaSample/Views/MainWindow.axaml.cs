using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
