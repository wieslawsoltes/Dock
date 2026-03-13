using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using Dock.Model.ReactiveUI.Services.Avalonia.Controls;

namespace DockReactiveUICanonicalSample.Views.Pages;

public partial class ProjectFilePageView : DockReactiveUserControl<ProjectFilePageViewModel>
{
    public ProjectFilePageView()
    {
        InitializeComponent();
    }
}
