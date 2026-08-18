using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Workspaces;
using ReactiveUI.Avalonia.Reactive;

namespace DockOfficeSample.Views.Workspaces;

public partial class PowerPointWorkspaceView : ReactiveUserControl<PowerPointWorkspaceViewModel>
{
    public PowerPointWorkspaceView()
    {
        InitializeComponent();
    }
}
