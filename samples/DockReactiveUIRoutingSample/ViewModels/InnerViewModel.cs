using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels;

public class InnerViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Text { get; }

    public InnerViewModel(IScreen host, string text)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Text = text;
    }
}
