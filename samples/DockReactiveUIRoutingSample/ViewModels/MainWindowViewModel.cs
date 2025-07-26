using System;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    private readonly DockFactory _factory;
    private IRootDock? _layout;

    public RoutingState Router { get; } = new RoutingState();

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public MainWindowViewModel()
    {
        _factory = new DockFactory(this);
        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
            Router.Navigate.Execute((IRoutableViewModel)layout).Subscribe(new Action<object>(_ => { }));
        }
        Layout = layout;
    }
}
