using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
