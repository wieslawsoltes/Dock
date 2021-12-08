using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.Views;

public class DebugView : UserControl
{
    public DebugView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}