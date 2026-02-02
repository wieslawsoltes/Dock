using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIWindowRelationsSample.Views.Tools;

public partial class WindowCasesToolView : UserControl
{
    public WindowCasesToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
