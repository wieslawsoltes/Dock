using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Pages;

namespace DockReactiveUICanonicalSample.Views.Pages;

public partial class ProjectFilesPageView : DockReactiveUserControl<ProjectFilesPageViewModel>
{
    public ProjectFilesPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
