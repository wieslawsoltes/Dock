using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
