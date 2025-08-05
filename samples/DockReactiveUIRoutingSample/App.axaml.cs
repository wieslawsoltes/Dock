using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DockReactiveUIRoutingSample.ViewModels;
using DockReactiveUIRoutingSample.Views;
using ReactiveUI;
using Splat;

namespace DockReactiveUIRoutingSample;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterViews();
    }

    private void RegisterViews()
    {
        // Register views with their corresponding view models using Splat
        Locator.CurrentMutable.Register<IViewFor<DockViewModel>>(() => new Views.DockView());
        Locator.CurrentMutable.Register<IViewFor<ViewModels.Documents.DocumentViewModel>>(() => new Views.Documents.DocumentView());
        Locator.CurrentMutable.Register<IViewFor<ViewModels.Inner.InnerViewModel>>(() => new Views.Inner.InnerView());
        Locator.CurrentMutable.Register<IViewFor<ViewModels.Tools.ToolViewModel>>(() => new Views.Tools.ToolView());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var vm = new MainWindowViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }
}
