using Dock.Model.ReactiveUI.Controls;
using DockReactiveUICanonicalSample.Models;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class ProjectFileEditorDocumentViewModel : Document
{
    public ProjectFileEditorDocumentViewModel(Project project, ProjectFile file)
    {
        Project = project;
        File = file;
        Id = $"Editor-{project.Id}-{file.Id}";
        Title = file.Name;
        CanClose = false;
        Content = $"// Project: {project.Name}\n// File: {file.Path}\n\npublic class Sample\n{{\n    public void Run()\n    {{\n        // TODO: Implement editor content.\n    }}\n}}\n";
    }

    public Project Project { get; }

    public ProjectFile File { get; }

    public string Content { get; }
}
