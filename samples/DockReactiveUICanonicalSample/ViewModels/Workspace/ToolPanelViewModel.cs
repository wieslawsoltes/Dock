using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class ToolPanelViewModel : RoutableToolBase
{
    private readonly IDialogServiceProvider _dialogServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;

    public ToolPanelViewModel(
        IScreen hostScreen,
        string toolId,
        string title,
        string description,
        IDialogServiceProvider dialogServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
        : base(hostScreen, "tool-panel")
    {
        ToolId = toolId;
        Title = title;
        Description = description;
        _dialogServiceProvider = dialogServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;

        Router.Navigate.Execute(new ToolPanelPageViewModel(
                this,
                toolId,
                title,
                description,
                _dialogServiceProvider,
                _confirmationServiceProvider))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public string ToolId { get; }

    public string Description { get; }
}
