using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Pages;

public partial class ProjectFilePageView : ReactiveUserControl<ProjectFilePageViewModel>
{
    public ProjectFilePageView()
    {
        InitializeComponent();
        this.WhenActivated(_ => { });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
