using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Dock.Model.Services;
using DockReactiveUICanonicalSample.Models;
using Dock.Model.ReactiveUI.Services;
using DockReactiveUICanonicalSample.ViewModels.Dialogs;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class FileActionsPageViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;

    public FileActionsPageViewModel(
        IScreen hostScreen,
        Project project,
        ProjectFile file,
        IHostOverlayServicesProvider overlayServicesProvider)
    {
        HostScreen = hostScreen;
        Project = project;
        File = file;
        _overlayServicesProvider = overlayServicesProvider;

        Actions = new ObservableCollection<FileActionItemViewModel>
        {
            new FileActionItemViewModel(
                "Open with Preview",
                "Show a quick preview dialog for the current file.",
                ReactiveCommand.CreateFromTask(OpenPreviewAsync)),
            new FileActionItemViewModel(
                "Rename File",
                "Prompt for a new name and confirm the rename.",
                ReactiveCommand.CreateFromTask(RenameFileAsync)),
            new FileActionItemViewModel(
                "Duplicate",
                "Run a two-step dialog flow for duplication.",
                ReactiveCommand.CreateFromTask(DuplicateFileAsync)),
            new FileActionItemViewModel(
                "Reveal in Explorer",
                "Show the file path in a dialog.",
                ReactiveCommand.CreateFromTask(RevealPathAsync))
        };
    }

    public string UrlPathSegment { get; } = "file-actions";

    public IScreen HostScreen { get; }

    public Project Project { get; }

    public ProjectFile File { get; }

    public ObservableCollection<FileActionItemViewModel> Actions { get; }

    private IDockDialogService GetDialogService()
        => _overlayServicesProvider.GetServices(HostScreen).Dialogs;

    private IDockConfirmationService GetConfirmationService()
        => _overlayServicesProvider.GetServices(HostScreen).Confirmations;

    private async Task OpenPreviewAsync()
    {
        var dialog = GetDialogService();
        await dialog.ShowAsync<bool>(
            new MessageDialogViewModel($"Previewing {File.Name}..."),
            "Preview");
    }

    private async Task RenameFileAsync()
    {
        var dialog = GetDialogService();
        var confirmation = GetConfirmationService();

        var namePrompt = new TextPromptDialogViewModel(
            "Enter a new filename.",
            "New name",
            primaryText: "Rename",
            secondaryText: "Cancel",
            initialValue: File.Name);

        var newName = await dialog.ShowAsync<string>(namePrompt, "Rename File");
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        var approved = await confirmation.ConfirmAsync(
            "Confirm Rename",
            $"Rename {File.Name} to {newName}?",
            confirmText: "Rename",
            cancelText: "Keep");

        if (approved)
        {
            await dialog.ShowAsync<bool>(
                new MessageDialogViewModel($"Renamed to {newName}."),
                "Rename Complete");
        }
    }

    private async Task DuplicateFileAsync()
    {
        var dialog = GetDialogService();
        var confirmation = GetConfirmationService();

        var namePrompt = new TextPromptDialogViewModel(
            "Name the duplicate file.",
            "Duplicate name",
            primaryText: "Next",
            secondaryText: "Cancel",
            initialValue: $"{File.Name}.copy");

        var newName = await dialog.ShowAsync<string>(namePrompt, "Duplicate File");
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        var pathPrompt = new TextPromptDialogViewModel(
            "Choose the destination folder.",
            "Destination path",
            primaryText: "Next",
            secondaryText: "Cancel",
            initialValue: "/workspace/exports");

        var destination = await dialog.ShowAsync<string>(pathPrompt, "Destination");
        if (string.IsNullOrWhiteSpace(destination))
        {
            return;
        }

        var approved = await confirmation.ConfirmAsync(
            "Confirm Duplicate",
            $"Duplicate {File.Name} as {newName} in {destination}?",
            confirmText: "Duplicate",
            cancelText: "Cancel");

        if (approved)
        {
            await dialog.ShowAsync<bool>(
                new MessageDialogViewModel("Duplicate queued for processing."),
                "Duplicate Scheduled");
        }
    }

    private async Task RevealPathAsync()
    {
        var dialog = GetDialogService();
        await dialog.ShowAsync<bool>(
            new MessageDialogViewModel($"Path: {File.Path}"),
            "File Location");
    }
}
