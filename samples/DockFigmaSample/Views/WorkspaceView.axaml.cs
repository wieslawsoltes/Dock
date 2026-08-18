using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels;
using ReactiveUI.Avalonia.Reactive;

namespace DockFigmaSample.Views;

public partial class WorkspaceView : ReactiveUserControl<WorkspaceViewModel>
{
    public WorkspaceView()
    {
        InitializeComponent();
    }
}
