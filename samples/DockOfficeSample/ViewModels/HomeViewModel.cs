using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DockOfficeSample.Models;
using DockOfficeSample.ViewModels.Workspaces;
using ReactiveUI;

namespace DockOfficeSample.ViewModels;

public class HomeViewModel : ReactiveObject, IRoutableViewModel
{
    public HomeViewModel(IScreen host)
    {
        HostScreen = host;
        OpenWord = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(new WordWorkspaceViewModel(host)).Subscribe(_ => { }));
        OpenExcel = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(new ExcelWorkspaceViewModel(host)).Subscribe(_ => { }));
        OpenPowerPoint = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(new PowerPointWorkspaceViewModel(host)).Subscribe(_ => { }));
    }

    public string UrlPathSegment { get; } = "home";
    public IScreen HostScreen { get; }

    public ObservableCollection<OfficeRecentFile> RecentFiles { get; } = new(OfficeSampleData.RecentFiles);

    public ReactiveCommand<Unit, IDisposable> OpenWord { get; }
    public ReactiveCommand<Unit, IDisposable> OpenExcel { get; }
    public ReactiveCommand<Unit, IDisposable> OpenPowerPoint { get; }
}
