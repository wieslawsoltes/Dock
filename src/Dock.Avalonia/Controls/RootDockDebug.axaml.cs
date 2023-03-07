using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls;

/// <summary>
/// 
/// </summary>
public class RootDockDebug : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public RootDockDebug()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
