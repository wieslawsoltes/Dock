using System;
using System.Linq;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Selectors;
using Dock.Avalonia.Themes.Fluent;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class AutomationPeerLeakTests
{
    [ReleaseFact]
    public void DockAutomationPeers_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var selectorItem = new DockSelectorItem(context.Document, 1, true, false, false, false, false);
            var selectorItem2 = new DockSelectorItem(context.Tool, 2, false, true, false, false, false);

            var controls = new Control[]
            {
                new DockCommandBarHost
                {
                    MenuBars = new Control[] { new TextBlock { Text = "menu" } },
                    ToolBars = new Control[] { new TextBlock { Text = "toolbar" } }
                },
                new RootDockControl { DataContext = context.Root },
                new DockTarget(),
                new DocumentTabStripItem { DataContext = context.Document },
                new ToolTabStripItem { DataContext = context.Tool },
                new DocumentControl { DataContext = context.DocumentDock },
                new ToolControl { DataContext = context.ToolDock },
                new MdiDocumentControl { DataContext = context.DocumentDock },
                new DocumentTabStrip
                {
                    DataContext = context.DocumentDock,
                    ItemsSource = context.DocumentDock.VisibleDockables
                },
                new ToolTabStrip
                {
                    DataContext = context.ToolDock,
                    ItemsSource = context.ToolDock.VisibleDockables
                },
                new ToolChromeControl { DataContext = context.ToolDock, ToolFlyout = new Flyout() },
                new MdiDocumentWindow { DataContext = context.Document },
                new PinnedDockControl { DataContext = context.Root },
                new ToolPinnedControl { ItemsSource = new[] { context.Tool } },
                new ToolPinItemControl { DataContext = context.Tool },
                new HostWindowTitleBar(),
                new DragPreviewControl
                {
                    Status = "Dock",
                    ShowContent = true
                },
                new DockSelectorOverlay
                {
                    Mode = DockSelectorMode.Documents,
                    Items = new[] { selectorItem, selectorItem2 },
                    SelectedItem = selectorItem,
                    IsOpen = true
                }
            };

            var host = new StackPanel();
            foreach (var control in controls)
            {
                host.Children.Add(control);
            }

            var window = new Window { Content = host };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            foreach (var control in controls)
            {
                control.ApplyTemplate();
                control.UpdateLayout();
                ControlAutomationPeer.CreatePeerForElement(control);

                switch (control)
                {
                    case DocumentTabStripItem documentTabStripItem:
                        documentTabStripItem.IsSelected = true;
                        documentTabStripItem.IsSelected = false;
                        break;
                    case ToolTabStripItem toolTabStripItem:
                        toolTabStripItem.IsSelected = true;
                        toolTabStripItem.IsSelected = false;
                        break;
                    case DockSelectorOverlay overlay:
                        overlay.IsOpen = false;
                        overlay.IsOpen = true;
                        overlay.SelectedItem = selectorItem2;
                        overlay.Mode = DockSelectorMode.Tools;
                        overlay.Mode = DockSelectorMode.Documents;
                        break;
                    case DragPreviewControl dragPreviewControl:
                        dragPreviewControl.Status = "Float";
                        break;
                }
            }

            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRefs = controls.Select(control => new WeakReference(control)).ToArray();

            return new AutomationPeerLeakResult(
                controlRefs,
                window,
                context.Root,
                context.DocumentDock,
                context.ToolDock,
                context.Document,
                context.Tool);
        });

        AssertCollected(result.ControlRefs);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.RootKeepAlive);
        GC.KeepAlive(result.DocumentDockKeepAlive);
        GC.KeepAlive(result.ToolDockKeepAlive);
        GC.KeepAlive(result.DocumentKeepAlive);
        GC.KeepAlive(result.ToolKeepAlive);
    }

    private sealed record AutomationPeerLeakResult(
        WeakReference[] ControlRefs,
        Window WindowKeepAlive,
        Dock.Model.Avalonia.Controls.RootDock RootKeepAlive,
        Dock.Model.Avalonia.Controls.DocumentDock DocumentDockKeepAlive,
        Dock.Model.Avalonia.Controls.ToolDock ToolDockKeepAlive,
        Dock.Model.Avalonia.Controls.Document DocumentKeepAlive,
        Dock.Model.Avalonia.Controls.Tool ToolKeepAlive);
}
