using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;
using DockReactiveUIRoutingSample.ViewModels.Inner;

namespace DockReactiveUIRoutingSample.Views.Inner;

public partial class InnerView : ReactiveUserControl<InnerViewModel>
{
    public InnerView()
    {
        InitializeComponent();
    }
}
