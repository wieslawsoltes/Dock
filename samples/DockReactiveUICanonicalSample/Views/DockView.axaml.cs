using Avalonia.Markup.Xaml;
using Dock.Model.ReactiveUI.Navigation.ViewModels;
using Dock.Model.ReactiveUI.Services.Avalonia.Controls;

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
