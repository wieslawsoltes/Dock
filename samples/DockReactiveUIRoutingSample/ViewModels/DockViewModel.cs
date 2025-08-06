using Dock.Model.Controls;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels;

public class DockViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly DockFactory _factory;
    private IRootDock? _layout;

    public string UrlPathSegment { get; } = "dock";
    public IScreen HostScreen { get; }

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public DockViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        _factory = new DockFactory(hostScreen);
        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
        }
        Layout = layout;
    }
}