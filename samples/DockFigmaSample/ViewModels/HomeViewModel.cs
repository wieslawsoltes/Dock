using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DockFigmaSample.Models;
using ReactiveUI;

namespace DockFigmaSample.ViewModels;

public class HomeViewModel : ReactiveObject, IRoutableViewModel
{
    public HomeViewModel(IScreen host)
    {
        HostScreen = host;
        OpenWorkspace = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(new WorkspaceViewModel(host)).Subscribe(_ => { }));
    }

    public string UrlPathSegment { get; } = "home";
    public IScreen HostScreen { get; }

    public ObservableCollection<RecentFileItem> RecentFiles { get; } = SampleData.CreateRecentFiles();

    public ReactiveCommand<Unit, IDisposable> OpenWorkspace { get; }
}
