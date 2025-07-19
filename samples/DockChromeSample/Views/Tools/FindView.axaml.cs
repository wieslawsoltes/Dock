using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockChromeSample.Views.Tools;

public partial class FindView : UserControl
{
    public FindView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
