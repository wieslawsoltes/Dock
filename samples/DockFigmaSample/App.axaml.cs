using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels;
using DockFigmaSample.ViewModels.Documents;
using DockFigmaSample.ViewModels.Tools;
using DockFigmaSample.Views;
using DockFigmaSample.Views.Documents;
using DockFigmaSample.Views.Tools;
using ReactiveUI;
using Splat;

namespace DockFigmaSample;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterViews();
    }

    private static void RegisterViews()
    {
        Locator.CurrentMutable.Register<IViewFor<MainWindowViewModel>>(() => new MainWindow());
        Locator.CurrentMutable.Register<IViewFor<HomeViewModel>>(() => new HomeView());
        Locator.CurrentMutable.Register<IViewFor<WorkspaceViewModel>>(() => new WorkspaceView());

        Locator.CurrentMutable.Register<IViewFor<CanvasDocumentViewModel>>(() => new CanvasDocumentView());
        Locator.CurrentMutable.Register<IViewFor<DesignCanvasViewModel>>(() => new DesignCanvasView());
        Locator.CurrentMutable.Register<IViewFor<PrototypeCanvasViewModel>>(() => new PrototypeCanvasView());
        Locator.CurrentMutable.Register<IViewFor<InspectCanvasViewModel>>(() => new InspectCanvasView());

        Locator.CurrentMutable.Register<IViewFor<ToolbarToolViewModel>>(() => new ToolbarToolView());
        Locator.CurrentMutable.Register<IViewFor<LayersToolViewModel>>(() => new LayersToolView());
        Locator.CurrentMutable.Register<IViewFor<AssetsToolViewModel>>(() => new AssetsToolView());
        Locator.CurrentMutable.Register<IViewFor<InspectorToolViewModel>>(() => new InspectorToolView());
        Locator.CurrentMutable.Register<IViewFor<InspectorDesignViewModel>>(() => new InspectorDesignView());
        Locator.CurrentMutable.Register<IViewFor<InspectorPrototypeViewModel>>(() => new InspectorPrototypeView());
        Locator.CurrentMutable.Register<IViewFor<InspectorInspectViewModel>>(() => new InspectorInspectView());
        Locator.CurrentMutable.Register<IViewFor<CommentsToolViewModel>>(() => new CommentsToolView());
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
