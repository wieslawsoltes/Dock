using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Tools;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Tools;

public partial class OfficeInspectorToolView : ReactiveUserControl<OfficeInspectorToolViewModel>
{
    public OfficeInspectorToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
