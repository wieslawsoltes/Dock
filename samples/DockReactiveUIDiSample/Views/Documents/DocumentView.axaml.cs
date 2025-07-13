using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels.Documents;
using ReactiveUI;

namespace DockReactiveUIDiSample.Views.Documents;

public partial class DocumentView : UserControl, IViewFor<DocumentViewModel>
{
    public DocumentView()
    {
        InitializeComponent();
    }

    private DocumentViewModel? _viewModel;
    public DocumentViewModel? ViewModel
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
        set => ViewModel = (DocumentViewModel?)value;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

