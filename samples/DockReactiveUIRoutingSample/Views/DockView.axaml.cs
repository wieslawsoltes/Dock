using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using DockReactiveUIRoutingSample.ViewModels;

namespace DockReactiveUIRoutingSample.Views;

public partial class DockView : ReactiveUserControl<DockViewModel>
{
    public DockView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}