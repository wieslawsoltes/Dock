using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using DockReactiveUICanonicalSample.Views;
using DockReactiveUICanonicalSample.Views.Documents;
using DockReactiveUICanonicalSample.Views.Pages;
using DockReactiveUICanonicalSample.Views.Workspace;
using Dock.Model.Core;
using ReactiveUI;
using Splat;

namespace DockReactiveUICanonicalSample;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterDockableTemplate();
        RegisterAppServices();
        RegisterViews();
    }

    private void RegisterDockableTemplate()
    {
        DataTemplates.Add(new FuncDataTemplate<IDockable>(
            (item, _) => new ReactiveUI.Avalonia.ViewModelViewHost { ViewModel = item },
            supportsRecycling: true));
    }

    private static void RegisterAppServices()
    {
        var services = Locator.CurrentMutable;

        services.RegisterLazySingleton<IProjectRepository>(() => new ProjectRepository());
        services.RegisterLazySingleton(() => new ProjectFileWorkspaceFactory(
            Locator.Current.GetService<IBusyServiceFactory>()!,
            Locator.Current.GetService<IGlobalBusyService>()!,
            Locator.Current.GetService<IDialogServiceFactory>()!,
            Locator.Current.GetService<IGlobalDialogService>()!,
            Locator.Current.GetService<IConfirmationServiceFactory>()!,
            Locator.Current.GetService<IGlobalConfirmationService>()!,
            Locator.Current.GetService<IDialogServiceProvider>()!,
            Locator.Current.GetService<IConfirmationServiceProvider>()!));
        services.RegisterLazySingleton<IGlobalBusyService>(() => new GlobalBusyService());
        services.RegisterLazySingleton<IGlobalDialogService>(() => new GlobalDialogService());
        services.RegisterLazySingleton<IGlobalConfirmationService>(() => new GlobalConfirmationService());
        services.RegisterLazySingleton<IBusyServiceFactory>(() => new BusyServiceFactory(
            Locator.Current.GetService<IGlobalBusyService>()!));
        services.RegisterLazySingleton<IDialogServiceFactory>(() => new DialogServiceFactory(
            Locator.Current.GetService<IGlobalDialogService>()!));
        services.RegisterLazySingleton<IConfirmationServiceFactory>(() => new ConfirmationServiceFactory(
            Locator.Current.GetService<IGlobalConfirmationService>()!));
        services.RegisterLazySingleton<IBusyServiceProvider>(() => new BusyServiceProvider(
            Locator.Current.GetService<IBusyServiceFactory>()!));
        services.RegisterLazySingleton<IDialogServiceProvider>(() => new DialogServiceProvider(
            Locator.Current.GetService<IDialogServiceFactory>()!));
        services.RegisterLazySingleton<IConfirmationServiceProvider>(() => new ConfirmationServiceProvider(
            Locator.Current.GetService<IConfirmationServiceFactory>()!));
        services.RegisterLazySingleton<IDockNavigationService>(() => new DockNavigationService(
            Locator.Current.GetService<IProjectRepository>()!,
            Locator.Current.GetService<ProjectFileWorkspaceFactory>()!,
            Locator.Current.GetService<IBusyServiceProvider>()!,
            Locator.Current.GetService<IConfirmationServiceProvider>()!));

        services.RegisterLazySingleton(() => new MainWindowViewModel(
            Locator.Current.GetService<IProjectRepository>()!,
            Locator.Current.GetService<IDockNavigationService>()!,
            Locator.Current.GetService<ProjectFileWorkspaceFactory>()!,
            Locator.Current.GetService<IBusyServiceFactory>()!,
            Locator.Current.GetService<IBusyServiceProvider>()!,
            Locator.Current.GetService<IConfirmationServiceProvider>()!,
            Locator.Current.GetService<IGlobalBusyService>()!,
            Locator.Current.GetService<IDialogServiceFactory>()!,
            Locator.Current.GetService<IGlobalDialogService>()!,
            Locator.Current.GetService<IConfirmationServiceFactory>()!,
            Locator.Current.GetService<IGlobalConfirmationService>()!));
    }

    private static void RegisterViews()
    {
        var services = Locator.CurrentMutable;

        services.Register<IViewFor<DockViewModel>>(() => new DockView());

        services.Register<IViewFor<ProjectListDocumentViewModel>>(() => new ProjectListDocumentView());
        services.Register<IViewFor<ProjectFilesDocumentViewModel>>(() => new ProjectFilesDocumentView());
        services.Register<IViewFor<ProjectFileDocumentViewModel>>(() => new ProjectFileDocumentView());
        services.Register<IViewFor<ProjectFileEditorDocumentViewModel>>(() => new ProjectFileEditorDocumentView());

        services.Register<IViewFor<ProjectListPageViewModel>>(() => new ProjectListPageView());
        services.Register<IViewFor<ProjectFilesPageViewModel>>(() => new ProjectFilesPageView());
        services.Register<IViewFor<ProjectFilePageViewModel>>(() => new ProjectFilePageView());

        services.Register<IViewFor<RibbonToolViewModel>>(() => new RibbonToolView());
        services.Register<IViewFor<RibbonPageViewModel>>(() => new RibbonPageView());
        services.Register<IViewFor<FileActionsToolViewModel>>(() => new FileActionsToolView());
        services.Register<IViewFor<FileActionsPageViewModel>>(() => new FileActionsPageView());
        services.Register<IViewFor<ToolPanelViewModel>>(() => new ToolPanelView());
        services.Register<IViewFor<ToolPanelPageViewModel>>(() => new ToolPanelPageView());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = Locator.Current.GetService<MainWindowViewModel>()!;

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }
}
