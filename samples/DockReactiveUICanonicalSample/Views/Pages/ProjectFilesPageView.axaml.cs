using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Pages;

public partial class ProjectFilesPageView : ReactiveUserControl<ProjectFilesPageViewModel>
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
