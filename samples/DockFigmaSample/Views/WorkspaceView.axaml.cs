using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views;

public partial class WorkspaceView : ReactiveUserControl<WorkspaceViewModel>
{
    public WorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
