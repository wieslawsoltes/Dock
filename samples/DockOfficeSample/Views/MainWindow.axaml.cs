using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels;
using ReactiveUI.Avalonia.Reactive;

namespace DockOfficeSample.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
