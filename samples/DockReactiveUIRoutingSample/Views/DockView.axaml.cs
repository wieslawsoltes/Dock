using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;
using DockReactiveUIRoutingSample.ViewModels;

namespace DockReactiveUIRoutingSample.Views;

public partial class DockView : ReactiveUserControl<DockViewModel>
{
    public DockView()
    {
        InitializeComponent();
    }
}
