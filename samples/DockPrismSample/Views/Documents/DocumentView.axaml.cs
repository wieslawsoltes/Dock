using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockPrismSample.Views.Documents;

public partial class DocumentView : UserControl
{
    public DocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
