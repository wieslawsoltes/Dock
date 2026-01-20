using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Documents;

namespace DockReactiveUICanonicalSample.Views.Documents;

public partial class ProjectFilesDocumentView : DockReactiveUserControl<ProjectFilesDocumentViewModel>
{
    public ProjectFilesDocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
