using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class CapabilityPolicyThemeBindingTests
{
    private enum DockThemeKind
    {
        Fluent,
        Simple
    }

    private static (RootDock Root, DocumentDock Dock, Document Document) CreateDocumentHierarchy()
    {
        var root = new RootDock
        {
            VisibleDockables = new List<IDockable>()
        };

        var dock = new DocumentDock
        {
            Owner = root,
            VisibleDockables = new List<IDockable>(),
            DockCapabilityPolicy = new DockCapabilityPolicy
            {
                CanDrag = true,
                CanDrop = true
            }
        };

        var document = new Document
        {
            Owner = dock,
            CanDrag = true,
            CanDrop = true
        };

        root.VisibleDockables!.Add(dock);
        dock.VisibleDockables!.Add(document);
        root.ActiveDockable = dock;
        dock.ActiveDockable = document;

        return (root, dock, document);
    }

    private static (RootDock Root, ToolDock Dock, Tool Tool) CreateToolHierarchy()
    {
        var root = new RootDock
        {
            VisibleDockables = new List<IDockable>()
        };

        var dock = new ToolDock
        {
            Owner = root,
            VisibleDockables = new List<IDockable>(),
            CanDrag = true,
            CanDrop = true,
            DockCapabilityPolicy = new DockCapabilityPolicy
            {
                CanDrag = true,
                CanDrop = true
            }
        };

        var tool = new Tool
        {
            Owner = dock,
            CanDrag = true,
            CanDrop = true
        };

        root.VisibleDockables!.Add(dock);
        dock.VisibleDockables!.Add(tool);
        root.ActiveDockable = dock;
        dock.ActiveDockable = tool;

        return (root, dock, tool);
    }

    private static Window ShowWithTheme(Control control, DockThemeKind themeKind)
    {
        var window = new Window
        {
            Width = 900,
            Height = 700,
            Content = control
        };

        switch (themeKind)
        {
            case DockThemeKind.Fluent:
                window.Styles.Add(new DockFluentTheme());
                break;
            case DockThemeKind.Simple:
                window.Styles.Add(new DockSimpleTheme());
                break;
        }

        window.Show();
        window.UpdateLayout();
        control.ApplyTemplate();
        control.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void AssertDragDropCapabilityBinding(Control control, IDockable dockable, IDock policyDock, DockThemeKind themeKind)
    {
        var window = ShowWithTheme(control, themeKind);

        try
        {
            Assert.True(control.GetValue(DockProperties.IsDragEnabledProperty));
            Assert.True(control.GetValue(DockProperties.IsDropEnabledProperty));

            policyDock.DockCapabilityPolicy = new DockCapabilityPolicy
            {
                CanDrag = false,
                CanDrop = false
            };
            Dispatcher.UIThread.RunJobs();

            Assert.False(control.GetValue(DockProperties.IsDragEnabledProperty));
            Assert.False(control.GetValue(DockProperties.IsDropEnabledProperty));

            dockable.DockCapabilityOverrides = new DockCapabilityOverrides
            {
                CanDrag = true,
                CanDrop = true
            };
            Dispatcher.UIThread.RunJobs();

            Assert.True(control.GetValue(DockProperties.IsDragEnabledProperty));
            Assert.True(control.GetValue(DockProperties.IsDropEnabledProperty));

            dockable.DockCapabilityOverrides = null;
            dockable.CanDrag = false;
            dockable.CanDrop = false;
            policyDock.DockCapabilityPolicy = null;
            Dispatcher.UIThread.RunJobs();

            Assert.False(control.GetValue(DockProperties.IsDragEnabledProperty));
            Assert.False(control.GetValue(DockProperties.IsDropEnabledProperty));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentBasedControls_Should_Update_DragDrop_Bindings_For_All_Themes()
    {
        foreach (var themeKind in new[] { DockThemeKind.Fluent, DockThemeKind.Simple })
        {
            {
                var (_, dock, document) = CreateDocumentHierarchy();
                var control = new DocumentControl
                {
                    DataContext = document
                };
                AssertDragDropCapabilityBinding(control, document, dock, themeKind);
            }

            {
                var (_, dock, document) = CreateDocumentHierarchy();
                var control = new DocumentTabStripItem
                {
                    DataContext = document
                };
                AssertDragDropCapabilityBinding(control, document, dock, themeKind);
            }

            {
                var (_, dock, document) = CreateDocumentHierarchy();
                var control = new MdiDocumentWindow
                {
                    DataContext = document
                };
                AssertDragDropCapabilityBinding(control, document, dock, themeKind);
            }
        }
    }

    [AvaloniaFact]
    public void ToolBasedControls_Should_Update_DragDrop_Bindings_For_All_Themes()
    {
        foreach (var themeKind in new[] { DockThemeKind.Fluent, DockThemeKind.Simple })
        {
            {
                var (_, dock, tool) = CreateToolHierarchy();
                var control = new ToolTabStripItem
                {
                    DataContext = tool
                };
                AssertDragDropCapabilityBinding(control, tool, dock, themeKind);
            }

            {
                var (_, dock, tool) = CreateToolHierarchy();
                var control = new ToolPinItemControl
                {
                    DataContext = tool
                };
                AssertDragDropCapabilityBinding(control, tool, dock, themeKind);
            }

            {
                var (_, dock, _) = CreateToolHierarchy();
                var control = new ToolChromeControl
                {
                    DataContext = dock
                };
                AssertDragDropCapabilityBinding(control, dock, dock, themeKind);
            }
        }
    }
}
