using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockXamlSample;

public partial class ItemsSourceExampleWindow : Window
{
    public ItemsSourceExampleWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 