using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Tools;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Tools;

public partial class OfficeRibbonToolView : ReactiveUserControl<OfficeRibbonToolViewModel>
{
    public OfficeRibbonToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
