using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Tools;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Tools;

public partial class OfficeToolPanelView : ReactiveUserControl<OfficeToolPanelViewModel>
{
    public OfficeToolPanelView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
