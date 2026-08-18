using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolDocumentPreviewView : ReactiveUserControl<ToolDocumentPreviewViewModel>
{
    public ToolDocumentPreviewView()
    {
        InitializeComponent();
    }
}
