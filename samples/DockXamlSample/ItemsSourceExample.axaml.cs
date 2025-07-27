using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using System.Linq;

namespace DockXamlSample;

public partial class ItemsSourceExample : UserControl
{
    public ItemsSourceExample()
    {
        InitializeComponent();
        
        // Add debug logging to see if this is even called
        System.Diagnostics.Debug.WriteLine("ItemsSourceExample constructor called");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 