using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Workspaces;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Workspaces;

public partial class WordWorkspaceView : ReactiveUserControl<WordWorkspaceViewModel>
{
    public WordWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
