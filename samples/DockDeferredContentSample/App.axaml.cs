using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Controls.DeferredContentControl;
using DockDeferredContentSample.ViewModels;

namespace DockDeferredContentSample;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DeferredContentPresentationSettings.BudgetMode = DeferredContentPresentationBudgetMode.ItemCount;
        DeferredContentPresentationSettings.MaxPresentationsPerPass = 2;
        DeferredContentPresentationSettings.InitialDelay = TimeSpan.Zero;
        DeferredContentPresentationSettings.FollowUpDelay = TimeSpan.FromMilliseconds(40);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();

#if DEBUG
        this.AttachDevTools();
#endif
    }
}
