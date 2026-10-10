using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DockAvalonDockSample.Services;
using DockAvalonDockSample.ViewModels;
using Xceed.Wpf.AvalonDock;

namespace DockAvalonDockSample;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow window = new();
            DockingManager manager = window.FindControl<DockingManager>("DockManager")!;
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DockAvalonDockSample");
            ILayoutWorkspace workspace = new AvalonDockWorkspace(manager, Path.Combine(directory, "layout.xml"));
            window.DataContext = new MainWindowViewModel(workspace);
            desktop.MainWindow = window;
        }
        base.OnFrameworkInitializationCompleted();
    }
}
