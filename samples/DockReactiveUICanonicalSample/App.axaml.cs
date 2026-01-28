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
            (item, existing) =>
            {
                if (existing is ReactiveUI.Avalonia.ViewModelViewHost existingHost)
                {
                    if (!ReferenceEquals(existingHost.ViewModel, item))
                    {
                        existingHost.ViewModel = item;
                    }
                    return existingHost;
                }

                return new ReactiveUI.Avalonia.ViewModelViewHost { ViewModel = item };
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
