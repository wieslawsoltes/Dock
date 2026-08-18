using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Workspaces;
using ReactiveUI.Avalonia.Reactive;

namespace DockOfficeSample.Views.Workspaces;

public partial class WordWorkspaceView : ReactiveUserControl<WordWorkspaceViewModel>
{
    public WordWorkspaceView()
    {
        InitializeComponent();
    }
}
