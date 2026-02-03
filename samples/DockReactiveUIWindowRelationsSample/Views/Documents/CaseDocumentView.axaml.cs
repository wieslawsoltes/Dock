using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIWindowRelationsSample.Views.Documents;

public partial class CaseDocumentView : UserControl
{
    public CaseDocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
