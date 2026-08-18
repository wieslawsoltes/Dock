using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI.Avalonia.Reactive;

namespace DockFigmaSample.Views.Tools;

public partial class InspectorDesignView : ReactiveUserControl<InspectorDesignViewModel>
{
    public InspectorDesignView()
    {
        InitializeComponent();
    }
}
