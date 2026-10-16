using System.Reactive;
using DockAvalonDockSample.Services;
using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace DockAvalonDockSample.ViewModels;

/// <summary>Orchestrates sample commands without a dependency on the UI framework.</summary>
public partial class MainWindowViewModel : ReactiveObject
{
    [Reactive]
    private string _status = "Drag tabs to rearrange the workspace. Save and load to restore the layout.";

    /// <summary>Creates sample commands using the supplied workspace service.</summary>
    public MainWindowViewModel(ILayoutWorkspace workspace)
    {
        SaveLayout = ReactiveCommand.Create(() => { Status = workspace.Save(); });
        LoadLayout = ReactiveCommand.Create(() => { Status = workspace.Load(); });
        HideJournal = ReactiveCommand.Create(() => { Status = workspace.HideJournal(); });
        ShowJournal = ReactiveCommand.Create(() => { Status = workspace.ShowJournal(); });
    }
    /// <summary>Gets the save layout command.</summary>
    public ReactiveCommand<Unit, Unit> SaveLayout { get; }
    /// <summary>Gets the load layout command.</summary>
    public ReactiveCommand<Unit, Unit> LoadLayout { get; }
    /// <summary>Gets the hide journal command.</summary>
    public ReactiveCommand<Unit, Unit> HideJournal { get; }
    /// <summary>Gets the show journal command.</summary>
    public ReactiveCommand<Unit, Unit> ShowJournal { get; }
}
