using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockInpcSample.Views.Documents;

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
