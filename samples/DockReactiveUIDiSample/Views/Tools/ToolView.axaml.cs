using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels.Tools;
using ReactiveUI;

namespace DockReactiveUIDiSample.Views.Tools;

public partial class ToolView : UserControl, IViewFor<ToolViewModel>
{
    public ToolView()
    {
        InitializeComponent();
    }

    private ToolViewModel? _viewModel;
    public ToolViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            DataContext = value;
        }
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (ToolViewModel?)value;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

