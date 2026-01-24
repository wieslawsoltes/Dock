using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using Dock.Settings;

namespace DockCodeOnlySample;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseManagedWindows();
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
            var dockControl = new DockControl();

            var factory = new Factory();

            var documentDock = new DocumentDock
            {
                Id = "Documents",
                IsCollapsable = false,
                CanCreateDocument = true
            };

            documentDock.AllowedDropOperations = DockOperationMask.Fill;

            documentDock.DocumentFactory = () =>
            {
                var index = documentDock.VisibleDockables?.Count ?? 0;
                return new Document
                {
                    Id = $"Doc{index + 1}",
                    Title = $"Document {index + 1}",
                    Content = new TextBox { Text = $"Document {index + 1}", AcceptsReturn = true }
                };
            };

            var document = new Document 
            { 
                Id = "Doc1", 
                Title = "Document 1",
                // Content = new TextBox { Text = "Document 1", AcceptsReturn = true }
            };
            documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
            documentDock.ActiveDockable = document;

            var leftTool = new Tool { Id = "Tool1", Title = "Tool 1" };
            var bottomTool = new Tool { Id = "Tool2", Title = "Output" };

            leftTool.AllowedDockOperations = DockOperationMask.Left | DockOperationMask.Fill | DockOperationMask.Window;
            bottomTool.AllowedDockOperations = DockOperationMask.Bottom | DockOperationMask.Fill | DockOperationMask.Window;

            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = factory.CreateList<IDockable>(
                    new ToolDock
                    {
                        Id = "LeftPane",
                        Alignment = Alignment.Left,
                        Proportion = 0.25,
                        VisibleDockables = factory.CreateList<IDockable>(leftTool),
                        ActiveDockable = leftTool
                    },
                    new ProportionalDockSplitter(),
                    documentDock,
                    new ProportionalDockSplitter(),
                    new ToolDock
                    {
                        Id = "BottomPane",
                        Alignment = Alignment.Bottom,
                        Proportion = 0.25,
                        VisibleDockables = factory.CreateList<IDockable>(bottomTool),
                        ActiveDockable = bottomTool
                    })
            };

            var root = factory.CreateRootDock();
            root.VisibleDockables = factory.CreateList<IDockable>(mainLayout);
            root.DefaultDockable = mainLayout;

            factory.InitLayout(root);
            dockControl.Factory = factory;
            dockControl.Layout  = root;

            var serializer = new DockSerializer();
            var workspaceManager = new DockWorkspaceManager(serializer);
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

                var restored = workspaceManager.Restore(workspace);
                if (restored is null)
                {
                    return;
                }

                factory.InitLayout(restored);
                dockControl.Layout = restored;
            }

            var saveWorkspaceA = new Button { Content = "Save Workspace A" };
            saveWorkspaceA.Click += (_, _) => SaveWorkspace("A", ref workspaceA);

            var loadWorkspaceA = new Button { Content = "Load Workspace A" };
            loadWorkspaceA.Click += (_, _) => RestoreWorkspace(workspaceA);

            var saveWorkspaceB = new Button { Content = "Save Workspace B" };
            saveWorkspaceB.Click += (_, _) => SaveWorkspace("B", ref workspaceB);

            var loadWorkspaceB = new Button { Content = "Load Workspace B" };
            loadWorkspaceB.Click += (_, _) => RestoreWorkspace(workspaceB);

            var lockLayout = new CheckBox { Content = "Lock layout" };
            lockLayout.Checked += (_, _) => dockControl.IsDockingEnabled = false;
            lockLayout.Unchecked += (_, _) => dockControl.IsDockingEnabled = true;

            var toolbar = new StackPanel
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

            var content = new DockPanel();
            DockPanel.SetDock(toolbar, Avalonia.Controls.Dock.Top);
            content.Children.Add(toolbar);
            content.Children.Add(dockControl);

            desktop.MainWindow = new Window
            {
                Width = 800,
                Height = 600,
                Content = content
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
