using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using Dock.Model.ReactiveUI.Services.Avalonia.Controls;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class FileActionsPageView : DockReactiveUserControl<FileActionsPageViewModel>
{
    public FileActionsPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
