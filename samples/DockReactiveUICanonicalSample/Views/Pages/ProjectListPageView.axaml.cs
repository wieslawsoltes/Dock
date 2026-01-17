using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Pages;

public partial class ProjectListPageView : ReactiveUserControl<ProjectListPageViewModel>
{
    public ProjectListPageView()
    {
        InitializeComponent();
        this.WhenActivated(_ => { });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
