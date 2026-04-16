using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer;

namespace DockCodeOnlySample;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new DockFluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DockControl dockControl = new();
            DockCodeOnlyFactory factory = new();
            IRootDock layout = factory.CreateLayout();
            factory.InitLayout(layout);
            dockControl.Factory = factory;
            dockControl.Layout = layout;

            DockWorkspaceManager workspaceManager = new(new DockSerializer());
            DockWorkspace? workspaceA = null;
            DockWorkspace? workspaceB = null;

            void SaveWorkspace(string id, ref DockWorkspace? slot)
            {
                if (dockControl.Layout is IDock layout)
                {
                    slot = workspaceManager.Capture(id, layout, includeState: true, name: $"Workspace {id}");
                }
            }

            void RestoreWorkspace(DockWorkspace? workspace)
            {
                if (workspace is null)
                {
                    return;
                }

                IDock? restored = workspaceManager.Restore(workspace);
                if (restored is not IRootDock root)
                {
                    return;
                }

                factory.InitLayout(root);
                dockControl.Layout = root;
            }

            Button saveWorkspaceA = new() { Content = "Save Workspace A" };
            saveWorkspaceA.Click += (_, _) => SaveWorkspace("A", ref workspaceA);

            Button loadWorkspaceA = new() { Content = "Load Workspace A" };
            loadWorkspaceA.Click += (_, _) => RestoreWorkspace(workspaceA);

            Button saveWorkspaceB = new() { Content = "Save Workspace B" };
            saveWorkspaceB.Click += (_, _) => SaveWorkspace("B", ref workspaceB);

            Button loadWorkspaceB = new() { Content = "Load Workspace B" };
            loadWorkspaceB.Click += (_, _) => RestoreWorkspace(workspaceB);

            CheckBox lockLayout = new() { Content = "Lock layout" };
            lockLayout.IsCheckedChanged += (_, _) => dockControl.IsDockingEnabled = lockLayout.IsChecked != true;

            StackPanel toolbar = new()
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(8)
            };
            toolbar.Children.Add(saveWorkspaceA);
            toolbar.Children.Add(loadWorkspaceA);
            toolbar.Children.Add(saveWorkspaceB);
            toolbar.Children.Add(loadWorkspaceB);
            toolbar.Children.Add(lockLayout);

            DockPanel content = new();
            DockPanel.SetDock(toolbar, Avalonia.Controls.Dock.Top);
            content.Children.Add(toolbar);
            content.Children.Add(dockControl);

            desktop.MainWindow = new Window
            {
                Title = "Dock Code-Only Sample",
                Width = 1000,
                Height = 720,
                Content = content
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
