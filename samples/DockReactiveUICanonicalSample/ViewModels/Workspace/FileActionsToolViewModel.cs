using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class FileActionsToolViewModel : RoutableToolBase
{
    private readonly IDialogServiceProvider _dialogServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;

    public FileActionsToolViewModel(
        IScreen hostScreen,
        Project project,
        ProjectFile file,
        IDialogServiceProvider dialogServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
        : base(hostScreen, "file-actions")
    {
        _dialogServiceProvider = dialogServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;

        Router.Navigate.Execute(new FileActionsPageViewModel(
                this,
                project,
                file,
                _dialogServiceProvider,
                _confirmationServiceProvider))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
