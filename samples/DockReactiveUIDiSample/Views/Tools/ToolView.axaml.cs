using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DockReactiveUIDiSample.ViewModels.Tools;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace DockReactiveUIDiSample.Views.Tools;

public partial class ToolView : ReactiveUserControl<ToolViewModel>
{
    public ToolView()
    {
        InitializeComponent();
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

