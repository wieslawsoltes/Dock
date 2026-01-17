using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Documents;

public partial class ProjectFileEditorDocumentView : ReactiveUserControl<ProjectFileEditorDocumentViewModel>
{
    public ProjectFileEditorDocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
