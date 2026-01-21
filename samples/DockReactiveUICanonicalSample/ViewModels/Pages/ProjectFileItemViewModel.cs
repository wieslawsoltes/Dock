using System;
using System.Reactive;
using DockReactiveUICanonicalSample.Models;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectFileItemViewModel : ReactiveObject
{
    public ProjectFileItemViewModel(
        ProjectFile file,
        Action<ProjectFile> openFile,
        Action<ProjectFile> openFileTab,
        Action<ProjectFile> openFileFloating)
    {
        File = file;
        OpenFile = ReactiveCommand.Create(() => openFile(file));
        OpenFileTab = ReactiveCommand.Create(() => openFileTab(file));
        OpenFileFloating = ReactiveCommand.Create(() => openFileFloating(file));
    }

    public ProjectFile File { get; }

    public string Name => File.Name;

    public string Path => File.Path;

    public ReactiveCommand<Unit, Unit> OpenFile { get; }

    public ReactiveCommand<Unit, Unit> OpenFileTab { get; }

    public ReactiveCommand<Unit, Unit> OpenFileFloating { get; }
}
