using System.Reactive;
using System.Threading.Tasks;
using Dock.Model.Core;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Dialogs;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class ToolPanelPageViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IDialogServiceProvider _dialogServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;

    public ToolPanelPageViewModel(
        IScreen hostScreen,
        string toolId,
        string title,
        string description,
        IDialogServiceProvider dialogServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
    {
        HostScreen = hostScreen;
        ToolId = toolId;
        Title = title;
        Description = description;
        _dialogServiceProvider = dialogServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;

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
        => _dialogServiceProvider.GetDialogService(HostScreen);

    private IDockConfirmationService GetConfirmationService()
        => _confirmationServiceProvider.GetConfirmationService(HostScreen);

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
            CloseDockable(dockable);
        }
    }

    private static void CloseDockable(IDockable dockable)
    {
        var factory = FindFactory(dockable);
        if (factory is null)
        {
            return;
        }

        factory.CloseDockable(dockable);
    }

    private static IFactory? FindFactory(IDockable dockable)
    {
        IDockable? current = dockable;
        while (current is not null)
        {
            if (current is IDock dock && dock.Factory is { } factory)
            {
                return factory;
            }

            current = current.Owner;
        }

        return null;
    }
}
