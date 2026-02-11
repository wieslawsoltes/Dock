using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Tools;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Tools;

public partial class InspectorSectionView : ReactiveUserControl<InspectorSectionViewModel>
{
    public InspectorSectionView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
