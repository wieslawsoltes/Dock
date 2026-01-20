using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Documents;

namespace DockReactiveUICanonicalSample.Views.Documents;

public partial class ProjectFileDocumentView : DockReactiveUserControl<ProjectFileDocumentViewModel>
{
    public ProjectFileDocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
