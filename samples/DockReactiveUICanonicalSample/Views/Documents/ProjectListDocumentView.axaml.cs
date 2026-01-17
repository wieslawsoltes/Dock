using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Documents;

public partial class ProjectListDocumentView : ReactiveUserControl<ProjectListDocumentViewModel>
{
    public ProjectListDocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
