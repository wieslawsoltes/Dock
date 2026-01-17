using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Documents;

public partial class ProjectFilesDocumentView : ReactiveUserControl<ProjectFilesDocumentViewModel>
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
