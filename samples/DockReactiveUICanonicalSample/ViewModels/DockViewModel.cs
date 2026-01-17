using Dock.Model.Controls;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

public class DockViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly DockFactory _factory;
    private IRootDock? _layout;

    public DockViewModel(IScreen hostScreen, DockFactory factory)
    {
        HostScreen = hostScreen;
        _factory = factory;

        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
        }

        Layout = layout;
    }

    public string UrlPathSegment { get; } = "dock";

    public IScreen HostScreen { get; }

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }
}
