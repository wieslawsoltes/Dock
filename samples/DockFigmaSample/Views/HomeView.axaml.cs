using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
