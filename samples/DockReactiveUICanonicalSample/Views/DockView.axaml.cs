using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views;

public partial class DockView : ReactiveUserControl<DockViewModel>
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
