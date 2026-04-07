using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Workspaces;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views.Workspaces;

public partial class ExcelWorkspaceView : ReactiveUserControl<ExcelWorkspaceViewModel>
{
    public ExcelWorkspaceView()
    {
        InitializeComponent();
    }
}
