using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Diagnostics;

namespace DockMvvmSample.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        // this.AttachDockDebugOverlay();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
