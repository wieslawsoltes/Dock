using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace DockOfficeSample.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new RoutingState();

    public MainWindowViewModel()
    {
        var home = new HomeViewModel(this);
        Router.Navigate.Execute(home).Subscribe(_ => { });
    }
}
