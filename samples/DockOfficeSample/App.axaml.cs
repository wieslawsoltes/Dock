using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels;
using DockOfficeSample.ViewModels.Documents;
using DockOfficeSample.ViewModels.Tools;
using DockOfficeSample.ViewModels.Workspaces;
using DockOfficeSample.Views;
using DockOfficeSample.Views.Documents;
using DockOfficeSample.Views.Tools;
using DockOfficeSample.Views.Workspaces;
using ReactiveUI;
using Splat;

namespace DockOfficeSample;

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

        Locator.CurrentMutable.Register<IViewFor<WordWorkspaceViewModel>>(() => new WordWorkspaceView());
        Locator.CurrentMutable.Register<IViewFor<ExcelWorkspaceViewModel>>(() => new ExcelWorkspaceView());
        Locator.CurrentMutable.Register<IViewFor<PowerPointWorkspaceViewModel>>(() => new PowerPointWorkspaceView());

        Locator.CurrentMutable.Register<IViewFor<WordDocumentViewModel>>(() => new WordDocumentView());
        Locator.CurrentMutable.Register<IViewFor<ExcelDocumentViewModel>>(() => new ExcelDocumentView());
        Locator.CurrentMutable.Register<IViewFor<PowerPointDocumentViewModel>>(() => new PowerPointDocumentView());
        Locator.CurrentMutable.Register<IViewFor<OfficeDocumentPageViewModel>>(() => new OfficeDocumentPageView());

        Locator.CurrentMutable.Register<IViewFor<OfficeRibbonToolViewModel>>(() => new OfficeRibbonToolView());
        Locator.CurrentMutable.Register<IViewFor<OfficeToolPanelViewModel>>(() => new OfficeToolPanelView());
        Locator.CurrentMutable.Register<IViewFor<OfficeInspectorToolViewModel>>(() => new OfficeInspectorToolView());
        Locator.CurrentMutable.Register<IViewFor<InspectorSectionViewModel>>(() => new InspectorSectionView());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var viewModel = new MainWindowViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }
}
