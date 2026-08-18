using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolComparisonView : ReactiveUserControl<ToolComparisonViewModel>
{
    public ToolComparisonView()
    {
        InitializeComponent();
    }
}
