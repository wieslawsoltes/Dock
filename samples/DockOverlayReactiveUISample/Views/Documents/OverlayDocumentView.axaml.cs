using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockOverlayReactiveUISample.Views.Documents;

public partial class OverlayDocumentView : UserControl
{
    public OverlayDocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
