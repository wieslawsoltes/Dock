using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Tools;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views.Tools;

public partial class OfficeRibbonToolView : ReactiveUserControl<OfficeRibbonToolViewModel>
{
    public OfficeRibbonToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
