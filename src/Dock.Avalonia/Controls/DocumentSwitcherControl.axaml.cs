using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Control hosting the document switcher list.
/// </summary>
public partial class DocumentSwitcherControl : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentSwitcherControl"/> class.
    /// </summary>
    public DocumentSwitcherControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
