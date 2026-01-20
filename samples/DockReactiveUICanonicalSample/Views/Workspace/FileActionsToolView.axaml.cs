using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class FileActionsToolView : DockReactiveUserControl<FileActionsToolViewModel>
{
    public FileActionsToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
