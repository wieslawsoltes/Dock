using Avalonia;
using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels;
using ReactiveUI.Avalonia;

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

}

