using System;
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
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Navigation.ViewModels;
using Dock.Model.ReactiveUI.Services.Avalonia;
using ReactiveUI.Reactive;
using ReactiveUI;
using Splat;

namespace DockReactiveUICanonicalSample;

public partial class App : Application
{
    public static IViewLocator ViewLocator { get; } = new DefaultViewLocator();

    public override void Initialize()
    {
#if DOCK_USE_GENERATED_APP_INITIALIZE_COMPONENT
        InitializeComponent();
#else
        AvaloniaXamlLoader.Load(this);
#endif
        RegisterDockableTemplate();
        RegisterAppServices();
        RegisterViews();
    }

    private void RegisterDockableTemplate()
    {
        var viewLocator = ViewLocator;

        DataTemplates.Insert(0, new FuncDataTemplate<IDockable>(
            (item, existing) =>
            {
                if (item is null)
                {
                    return null;
                }

                if (existing is ReactiveUI.Avalonia.Reactive.ViewModelViewHost existingHost)
                {
                    existingHost.ViewLocator = viewLocator;
                    if (!ReferenceEquals(existingHost.ViewModel, item))
                    {
                        existingHost.ViewModel = item;
                    }
                    return existingHost;
                }

                return new ReactiveUI.Avalonia.Reactive.ViewModelViewHost
                {
                    ViewLocator = viewLocator,
                    ViewModel = item
                };
            },
            supportsRecycling: true));
    }

    private static void RegisterAppServices()
    {
        var services = Locator.CurrentMutable;

        services.RegisterLazySingleton<IHostServiceResolver>(() => new AvaloniaHostServiceResolver());
        services.RegisterDockOverlayServices();
        services.RegisterLazySingleton<IProjectRepository>(() => new ProjectRepository());
        services.RegisterLazySingleton(() => new ProjectFileWorkspaceFactory(
            Locator.Current.GetService<IHostOverlayServicesProvider>()!,
            Locator.Current.GetService<Func<IHostOverlayServices>>()!,
            Locator.Current.GetService<IWindowLifecycleService>()!));
        services.RegisterLazySingleton<IDockNavigationService>(() => new DockNavigationService(
            Locator.Current.GetService<IProjectRepository>()!,
            Locator.Current.GetService<ProjectFileWorkspaceFactory>()!,
            Locator.Current.GetService<IHostOverlayServicesProvider>()!,
            Locator.Current.GetService<IDockDispatcher>()!));

        services.RegisterLazySingleton(() => new MainWindowViewModel(
            Locator.Current.GetService<IProjectRepository>()!,
            Locator.Current.GetService<IDockNavigationService>()!,
            Locator.Current.GetService<ProjectFileWorkspaceFactory>()!,
            Locator.Current.GetService<IHostOverlayServicesProvider>()!,
            Locator.Current.GetService<Func<IHostOverlayServices>>()!,
            Locator.Current.GetService<IWindowLifecycleService>()!,
            Locator.Current.GetService<IDockDispatcher>()!));
    }

    private static void RegisterViews()
    {
        var viewLocator = (DefaultViewLocator)ViewLocator;

        viewLocator.Map<DockViewModel>(() => new DockView());

        viewLocator.Map<ProjectListDocumentViewModel>(() => new ProjectListDocumentView());
        viewLocator.Map<ProjectFilesDocumentViewModel>(() => new ProjectFilesDocumentView());
        viewLocator.Map<ProjectFileDocumentViewModel>(() => new ProjectFileDocumentView());
        viewLocator.Map<ProjectFileEditorDocumentViewModel>(() => new ProjectFileEditorDocumentView());

        viewLocator.Map<ProjectListPageViewModel>(() => new ProjectListPageView());
        viewLocator.Map<ProjectFilesPageViewModel>(() => new ProjectFilesPageView());
        viewLocator.Map<ProjectFilePageViewModel>(() => new ProjectFilePageView());

        viewLocator.Map<RibbonToolViewModel>(() => new RibbonToolView());
        viewLocator.Map<RibbonPageViewModel>(() => new RibbonPageView());
        viewLocator.Map<FileActionsToolViewModel>(() => new FileActionsToolView());
        viewLocator.Map<FileActionsPageViewModel>(() => new FileActionsPageView());
        viewLocator.Map<ToolPanelViewModel>(() => new ToolPanelView());
        viewLocator.Map<ToolPanelPageViewModel>(() => new ToolPanelPageView());
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
    }
}
