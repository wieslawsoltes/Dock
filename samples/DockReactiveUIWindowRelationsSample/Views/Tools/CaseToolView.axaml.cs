using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIWindowRelationsSample.Views.Tools;

public partial class CaseToolView : UserControl
{
    public CaseToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
