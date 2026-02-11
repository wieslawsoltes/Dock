using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
