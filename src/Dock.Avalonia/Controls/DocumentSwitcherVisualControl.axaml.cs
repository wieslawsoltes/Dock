using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Visual Studio-like document switcher control.
/// </summary>
public partial class DocumentSwitcherVisualControl : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentSwitcherVisualControl"/> class.
    /// </summary>
    public DocumentSwitcherVisualControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
