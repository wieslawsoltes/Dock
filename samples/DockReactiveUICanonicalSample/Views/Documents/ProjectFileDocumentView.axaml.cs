using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Documents;

public partial class ProjectFileDocumentView : ReactiveUserControl<ProjectFileDocumentViewModel>
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
