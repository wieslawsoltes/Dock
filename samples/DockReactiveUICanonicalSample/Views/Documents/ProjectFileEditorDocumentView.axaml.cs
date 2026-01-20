using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;

namespace DockReactiveUICanonicalSample.Views.Documents;

public partial class ProjectFileEditorDocumentView : DockReactiveUserControl<ProjectFileEditorDocumentViewModel>
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
