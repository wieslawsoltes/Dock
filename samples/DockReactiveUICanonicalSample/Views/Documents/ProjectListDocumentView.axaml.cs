using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using Dock.Model.ReactiveUI.Services.Avalonia.Controls;

namespace DockReactiveUICanonicalSample.Views.Documents;

public partial class ProjectListDocumentView : DockReactiveUserControl<ProjectListDocumentViewModel>
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
