using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using DockReactiveUIRoutingSample.ViewModels.Inner;

namespace DockReactiveUIRoutingSample.Views.Inner;

public partial class InnerView : ReactiveUserControl<InnerViewModel>
{
    public InnerView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
