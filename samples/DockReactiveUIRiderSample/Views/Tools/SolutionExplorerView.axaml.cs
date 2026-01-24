using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DockReactiveUIRiderSample.ViewModels.Tools;

namespace DockReactiveUIRiderSample.Views.Tools;

public partial class SolutionExplorerView : UserControl
{
    public SolutionExplorerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnTreeViewDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is SolutionExplorerViewModel viewModel)
        {
            viewModel.OpenSelected();
        }
    }
}
