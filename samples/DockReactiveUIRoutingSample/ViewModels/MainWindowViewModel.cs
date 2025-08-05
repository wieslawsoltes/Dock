using System;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new RoutingState();

    public MainWindowViewModel()
    {
        // Navigate to the dock view on startup
        var dockViewModel = new DockViewModel(this);
        Router.Navigate.Execute(dockViewModel).Subscribe();
    }
}
