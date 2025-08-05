using System;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolSettingsViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public bool IsEnabled { get; set; }
    public string ConfigValue { get; set; }
    public ReactiveCommand<Unit, IDisposable>? GoBack { get; private set; }

    public ToolSettingsViewModel(IScreen host, string title)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = title;
        IsEnabled = true;
        ConfigValue = "Default Value";
        
        GoBack = ReactiveCommand.Create(() =>
            HostScreen.Router.NavigateBack.Execute().Subscribe(_ => { }));
    }
}