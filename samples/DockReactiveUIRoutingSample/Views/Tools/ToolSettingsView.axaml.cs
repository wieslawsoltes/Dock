using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolSettingsView : ReactiveUserControl<ToolSettingsViewModel>
{
    public ToolSettingsView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
