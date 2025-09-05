using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Diagnostics;
using Dock.Avalonia.Diagnostics.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using DockPrismSample.Themes;
using DockPrismSample.ViewModels;
using DockPrismSample.Views;
using Prism.DryIoc;
using Prism.Ioc;

namespace DockPrismSample;

[RequiresUnreferencedCode("Requires unreferenced code for MainWindowViewModel.")]
[RequiresDynamicCode("Requires unreferenced code for MainWindowViewModel.")]
public partial class App : PrismApplication
{
    public static IThemeManager? ThemeManager;

    public override void Initialize()
    {
        ThemeManager = new FluentThemeManager();
        AvaloniaXamlLoader.Load(this);

        // Required by Prism.Avalonia when overriding Initialize()
        base.Initialize();
    }

    /// <summary>Inform system which Window to use as the main app.</summary>
    /// <returns>Avalonia Window.</returns>
    protected override AvaloniaObject CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    /// <summary>Register you Services, Views, Dialogs, etc.</summary>
    /// <param name="containerRegistry">container.</param>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Services
        containerRegistry.RegisterSingleton<IFactory, DockFactory>();
        containerRegistry.RegisterSingleton<IDockSerializer, DockSerializer>();

        // Windows
        containerRegistry.Register<MainWindow>();

        // Views
        //// containerRegistry.RegisterForNavigation<DashboardView, DashboardViewModel>();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // DockManager.s_enableSplitToWindow = true;
        base.OnFrameworkInitializationCompleted();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktopLifetime:
            {
                var win = (MainWindow as Window);
                var vm = win?.DataContext as MainWindowViewModel;
                if (win is not null && vm is not null)
                {
#if DEBUG
                    win.AttachDockDebug(() => vm.Layout!, new KeyGesture(Key.F11));
                    win.AttachDockDebugOverlay(new KeyGesture(Key.F9));
#endif
                    win.Closing += (_, _) => { vm.CloseLayout(); };
                    desktopLifetime.Exit += (_, _) => { vm.CloseLayout(); };
                }

                break;
            }
        }

#if DEBUG
        this.AttachDevTools();
#endif
    }
}
