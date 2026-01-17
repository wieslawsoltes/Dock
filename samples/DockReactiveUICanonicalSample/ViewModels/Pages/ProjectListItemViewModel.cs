using System;
using System.Reactive;
using DockReactiveUICanonicalSample.Models;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectListItemViewModel : ReactiveObject
{
    public ProjectListItemViewModel(
        Project project,
        Action<Project> openFiles,
        Action<Project> openFilesTab,
        Action<Project> openFilesFloating)
    {
        Project = project;
        OpenFiles = ReactiveCommand.Create(() => openFiles(project));
        OpenFilesTab = ReactiveCommand.Create(() => openFilesTab(project));
        OpenFilesFloating = ReactiveCommand.Create(() => openFilesFloating(project));
    }

    public Project Project { get; }

    public string Name => Project.Name;

    public string Description => Project.Description;

    public ReactiveCommand<Unit, Unit> OpenFiles { get; }

    public ReactiveCommand<Unit, Unit> OpenFilesTab { get; }

    public ReactiveCommand<Unit, Unit> OpenFilesFloating { get; }
}
