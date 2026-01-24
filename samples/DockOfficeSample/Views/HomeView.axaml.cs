using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
