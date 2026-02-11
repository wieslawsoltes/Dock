using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views;

public partial class WorkspaceView : ReactiveUserControl<WorkspaceViewModel>
{
    public WorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
