using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Pages;

namespace DockReactiveUICanonicalSample.Views.Pages;

public partial class ProjectListPageView : DockReactiveUserControl<ProjectListPageViewModel>
{
    public ProjectListPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
