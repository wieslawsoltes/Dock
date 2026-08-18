using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels.Tools;
using ReactiveUI.Avalonia.Reactive;

namespace DockReactiveUIDiSample.Views.Tools;

public partial class ToolView : ReactiveUserControl<ToolViewModel>
{
    public ToolView()
    {
        InitializeComponent();
    }
}
