using System.Reactive;
using System.Threading.Tasks;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Navigation.Services;
using Dock.Model.ReactiveUI.Services;
using Dock.Model.Services;
using DockReactiveUICanonicalSample.ViewModels.Dialogs;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class ToolPanelPageViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;

    public ToolPanelPageViewModel(
        IScreen hostScreen,
        string toolId,
        string title,
        string description,
        IHostOverlayServicesProvider overlayServicesProvider)
    {
        HostScreen = hostScreen;
        ToolId = toolId;
        Title = title;
        Description = description;
        _overlayServicesProvider = overlayServicesProvider;

        OpenPanelDialog = ReactiveCommand.CreateFromTask(OpenPanelDialogAsync);
        OpenNestedDialog = ReactiveCommand.CreateFromTask(OpenNestedDialogAsync);
        ClosePanel = ReactiveCommand.CreateFromTask(ClosePanelAsync);
    }

    public string UrlPathSegment { get; } = "tool-panel";

    public IScreen HostScreen { get; }

    public string ToolId { get; }

    public string Title { get; }

    public string Description { get; }

    public bool CanClosePanel => ToolId == "history";

    public ReactiveCommand<Unit, Unit> OpenPanelDialog { get; }

    public ReactiveCommand<Unit, Unit> OpenNestedDialog { get; }

    public ReactiveCommand<Unit, Unit> ClosePanel { get; }

    private IDockDialogService GetDialogService()
        => _overlayServicesProvider.GetServices(HostScreen).Dialogs;

    private IDockConfirmationService GetConfirmationService()
        => _overlayServicesProvider.GetServices(HostScreen).Confirmations;

    private async Task OpenPanelDialogAsync()
    {
        var dialog = GetDialogService();
        await dialog.ShowAsync<bool>(
            new MessageDialogViewModel($"Panel '{Title}' opened a detail view."),
            $"{Title} Details");
    }

    private async Task OpenNestedDialogAsync()
    {
        var dialog = GetDialogService();

        var prompt = new TextPromptDialogViewModel(
            $"Filter {Title} results.",
            "Filter value",
            primaryText: "Next",
            secondaryText: "Cancel");

        var filter = await dialog.ShowAsync<string>(prompt, "Filter");
        if (string.IsNullOrWhiteSpace(filter))
        {
            return;
        }

        await dialog.ShowAsync<bool>(
            new MessageDialogViewModel($"Applied filter '{filter}'."),
            "Filter Applied");
    }

    private async Task ClosePanelAsync()
    {
        if (!CanClosePanel)
        {
            return;
        }

        var confirmation = GetConfirmationService();
        var approved = await confirmation.ConfirmAsync(
            "Close Panel",
            $"Close the {Title} panel?",
            confirmText: "Close",
            cancelText: "Keep");

        if (!approved)
        {
            return;
        }

        if (HostScreen is IDockable dockable)
        {
            DockNavigationHelpers.TryCloseDockable(dockable);
        }
    }
}
