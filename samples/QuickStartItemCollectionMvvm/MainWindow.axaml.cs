using Avalonia.Controls;
using QuickStartMultiDocumentMvvm;

namespace DockTest;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
