using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class FileActionsPageView : ReactiveUserControl<FileActionsPageViewModel>
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
