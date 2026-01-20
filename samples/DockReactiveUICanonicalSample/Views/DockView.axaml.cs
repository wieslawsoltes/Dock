using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels;

namespace DockReactiveUICanonicalSample.Views;

public partial class DockView : DockReactiveUserControl<DockViewModel>
{
    public DockView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
