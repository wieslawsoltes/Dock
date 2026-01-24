using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Tools;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views.Tools;

public partial class OfficeToolPanelView : ReactiveUserControl<OfficeToolPanelViewModel>
{
    public OfficeToolPanelView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
