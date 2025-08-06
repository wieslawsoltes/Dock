using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels;

namespace DockReactiveUIRoutingSample.Views;

public partial class DockView : ReactiveUserControl<DockViewModel>
{
    public DockView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}