using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Controls.Overlays;
using Dock.Avalonia.Selectors;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Core;
using Dock.Settings;
using static Dock.Avalonia.LeakTests.LeakTestCaseHelpers;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;

namespace Dock.Avalonia.LeakTests;

internal static partial class LeakTestCases
{
    internal static readonly ControlLeakCase[] ControlCases =
    [
        new ControlLeakCase("DockControl", context =>
        {
            var control = new DockControl { Factory = context.Factory, Layout = context.Root };
            return new ControlSetup(control, new object?[] { context.Factory });
        }),
        new ControlLeakCase("DockCommandBarHost", _ =>
        {
            var command = new NoOpCommand();
            var menuItem = new MenuItem { Header = "File" };
            var openItem = new MenuItem { Header = "Open", Command = command };
            var exitItem = new MenuItem { Header = "Exit", Command = command };
            menuItem.ItemsSource = new object[]
            {
                openItem,
                new Separator(),
                exitItem
            };

            var menu = new Menu
            {
                ItemsSource = new object[]
                {
                    menuItem
                }
            };

            var toolButton = new Button { Content = "Action", Command = command };
            var toolBar = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal };
            toolBar.Children.Add(toolButton);

            var ribbonButton = new Button { Content = "Ribbon", Command = command };
            var ribbonBar = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal };
            ribbonBar.Children.Add(ribbonButton);

            var host = new DockCommandBarHost
            {
                MenuBars = new Control[] { menu },
                ToolBars = new Control[] { toolBar },
                RibbonBars = new Control[] { ribbonBar },
                IsVisible = true
            };

            return new ControlSetup(host, new object?[] { command }, AfterShow: dockControl =>
            {
                ExerciseButtonInteractions(toolButton);
                ExerciseButtonInteractions(ribbonButton);
            }, InteractionMask: LeakTestHelpers.InputInteractionMask.All & ~LeakTestHelpers.InputInteractionMask.PointerPressRelease);
        }),
        new ControlLeakCase("DockDockControl", context =>
            new ControlSetup(new DockDockControl { DataContext = context.DockDock }, Array.Empty<object?>())),
        new ControlLeakCase("DockSelectorOverlay", context =>
        {
            var item = new DockSelectorItem(context.Document, 1, true, false, false, false, false);
            return new ControlSetup(new DockSelectorOverlay { Items = new[] { item } }, Array.Empty<object?>());
        }),
        new ControlLeakCase("DockTarget", _ =>
            new ControlSetup(new DockTarget(), Array.Empty<object?>())),
        new ControlLeakCase("GlobalDockTarget", _ =>
            new ControlSetup(new GlobalDockTarget(), Array.Empty<object?>())),
        new ControlLeakCase("DockableControl", context =>
        {
            var control = new DockableControl
            {
                TrackingMode = TrackingMode.Visible,
                DataContext = context.Document
            };
            return new ControlSetup(control, new object?[] { context.Factory });
        }),
        new ControlLeakCase("DocumentContentControl", context =>
            new ControlSetup(new DocumentContentControl { DataContext = context.Document }, new object?[] { context.Factory })),
        new ControlLeakCase("ToolContentControl", context =>
            new ControlSetup(new ToolContentControl { DataContext = context.Tool }, new object?[] { context.Factory })),
        new ControlLeakCase("DocumentControl", context =>
        {
            context.DocumentDock.CanCreateDocument = true;
            context.DocumentDock.CreateDocument = new NoOpCommand();

            var control = new DocumentControl { DataContext = context.DocumentDock };
            return new ControlSetup(control, Array.Empty<object?>(), AfterShow: dockControl =>
            {
                var documentControl = (DocumentControl)dockControl;
                var tabStrip = FindVisualDescendant<DocumentTabStrip>(documentControl);
                if (tabStrip is not null)
                {
                    var createButton = FindVisualDescendant<Button>(
                        tabStrip,
                        button => button.Name == "PART_ButtonCreate");
                    ApplyNoOpCommand(createButton);
                    ExerciseButtonInteractions(createButton);
                }

                var tabItem = FindVisualDescendant<DocumentTabStripItem>(documentControl);
                if (tabItem is not null)
                {
                    var closeButton = FindVisualDescendant<Button>(tabItem);
                    ApplyNoOpCommand(closeButton);
                    ExerciseButtonInteractions(closeButton);
                    OpenAndCloseContextMenu(tabItem, tabItem.DocumentContextMenu);
                }
            });
        }),
        new ControlLeakCase("ToolControl", context =>
            new ControlSetup(new ToolControl { DataContext = context.ToolDock }, Array.Empty<object?>(), AfterShow: control =>
            {
                var toolControl = (ToolControl)control;
                var tabStrip = FindVisualDescendant<ToolTabStrip>(toolControl);
                var tabItem = tabStrip?.ItemContainerGenerator.ContainerFromIndex(0) as ToolTabStripItem
                              ?? FindVisualDescendant<ToolTabStripItem>(toolControl);
                if (tabItem is not null)
                {
                    ExerciseInputInteractions(tabItem, includeDoubleTap: true, includeMiddlePress: true, clearInputState: false);
                    OpenAndCloseContextMenu(tabItem, tabItem.TabContextMenu);
                }
            })),
        new ControlLeakCase("ToolChromeControl", context =>
        {
            var control = new ToolChromeControl { DataContext = context.ToolDock };
            return new ControlSetup(control, Array.Empty<object?>(), AfterShow: dockControl =>
            {
                var chrome = (ToolChromeControl)dockControl;
                var menuButton = FindTemplateChild<Button>(chrome, "PART_MenuButton");
                var pinButton = FindTemplateChild<Button>(chrome, "PART_PinButton");
                var maximizeButton = FindTemplateChild<Button>(chrome, "PART_MaximizeRestoreButton");
                var closeButton = FindTemplateChild<Button>(chrome, "PART_CloseButton");

                ApplyNoOpCommand(pinButton);
                ApplyNoOpCommand(closeButton);

                ExerciseButtonInteractions(menuButton);
                ExerciseButtonInteractions(pinButton);
                ExerciseButtonInteractions(maximizeButton);
                ExerciseButtonInteractions(closeButton);

                if (chrome.ToolFlyout is not null)
                {
                    var flyoutTarget = (Control?)menuButton ?? chrome;
                    OpenAndCloseFlyout(flyoutTarget, chrome.ToolFlyout);
                }
            });
        }),
        new ControlLeakCase("DocumentDockControl", context =>
            new ControlSetup(new DocumentDockControl { DataContext = context.DocumentDock }, Array.Empty<object?>())),
        new ControlLeakCase("ToolDockControl", context =>
            new ControlSetup(new ToolDockControl { DataContext = context.ToolDock }, Array.Empty<object?>())),
        new ControlLeakCase("GridDockControl", context =>
            new ControlSetup(new GridDockControl { DataContext = context.GridDock }, Array.Empty<object?>())),
        new ControlLeakCase("ProportionalDockControl", context =>
            new ControlSetup(new ProportionalDockControl { DataContext = context.ProportionalDock }, Array.Empty<object?>())),
        new ControlLeakCase("StackDockControl", context =>
            new ControlSetup(new StackDockControl { DataContext = context.StackDock }, Array.Empty<object?>())),
        new ControlLeakCase("WrapDockControl", context =>
            new ControlSetup(new WrapDockControl { DataContext = context.WrapDock }, Array.Empty<object?>())),
        new ControlLeakCase("ProportionalStackPanel", _ =>
        {
            var panel = new ProportionalStackPanel();
            panel.Children.Add(new Border());
            panel.Children.Add(new ProportionalStackPanelSplitter());
            panel.Children.Add(new Border());
            return new ControlSetup(panel, Array.Empty<object?>(), AfterShow: control =>
            {
                var stackPanel = (ProportionalStackPanel)control;
                foreach (var child in stackPanel.Children)
                {
                    if (child is ProportionalStackPanelSplitter splitter)
                    {
                        ExerciseInputInteractions(splitter, clearInputState: false);
                        break;
                    }
                }
            });
        }),
        new ControlLeakCase("ProportionalStackPanelSplitter", _ =>
            new ControlSetup(new ProportionalStackPanelSplitter(), Array.Empty<object?>())),
        new ControlLeakCase("UniformGridDockControl", context =>
            new ControlSetup(new UniformGridDockControl { DataContext = context.UniformGridDock }, Array.Empty<object?>())),
        new ControlLeakCase("SplitViewDockControl", context =>
            new ControlSetup(new SplitViewDockControl { DataContext = context.SplitViewDock }, Array.Empty<object?>())),
        new ControlLeakCase("RootDockControl", context =>
            new ControlSetup(new RootDockControl { DataContext = context.Root }, Array.Empty<object?>(), AfterShow: control =>
            {
                var mainContent = FindTemplateChild<ContentControl>(control, "PART_MainContent");
                if (mainContent is not null)
                {
                    RaisePointerPressed(mainContent, MouseButton.Left);
                }
            })),
        new ControlLeakCase("PinnedDockControl", context =>
            new ControlSetup(new PinnedDockControl { DataContext = context.Root }, new object?[] { context.Root },
                BeforeCleanup: control => LeakTestHelpers.ClearPinnedDockControlState(control as PinnedDockControl),
                AfterShow: control =>
                {
                    if (TopLevel.GetTopLevel(control) is Control ownerWindow)
                    {
                        RaisePointerPressed(ownerWindow, MouseButton.Left);
                    }
                })),
        new ControlLeakCase("PinnedDockHostPanel", context =>
            new ControlSetup(new PinnedDockHostPanel { DataContext = context.Root }, new object?[] { context.Root })),
        new ControlLeakCase("DocumentTabStrip", context =>
        {
            context.DocumentDock.CanCreateDocument = true;
            context.DocumentDock.CreateDocument = new NoOpCommand();

            var control = new DocumentTabStrip
            {
                DataContext = context.DocumentDock,
                ItemsSource = context.DocumentDock.VisibleDockables,
                CanCreateItem = true
            };

            return new ControlSetup(control, Array.Empty<object?>(), AfterShow: dockControl =>
            {
                var createButton = FindTemplateChild<Button>(dockControl, "PART_ButtonCreate");
                ApplyNoOpCommand(createButton);
                ExerciseButtonInteractions(createButton);

                var tabStrip = (DocumentTabStrip)dockControl;
                var tabItem = tabStrip.ItemContainerGenerator.ContainerFromIndex(0) as DocumentTabStripItem
                              ?? FindVisualDescendant<DocumentTabStripItem>(tabStrip);
                if (tabItem is not null)
                {
                    var closeButton = FindVisualDescendant<Button>(tabItem);
                    ApplyNoOpCommand(closeButton);
                    ExerciseButtonInteractions(closeButton);
                    ExerciseInputInteractions(tabItem, includeDoubleTap: true, includeMiddlePress: true, clearInputState: false);
                    OpenAndCloseContextMenu(tabItem, tabItem.DocumentContextMenu);
                }
            });
        }),
        new ControlLeakCase("ToolTabStrip", context =>
            new ControlSetup(new ToolTabStrip { ItemsSource = new[] { context.Tool } }, Array.Empty<object?>(), AfterShow: control =>
            {
                var tabStrip = (ToolTabStrip)control;
                var tabItem = tabStrip.ItemContainerGenerator.ContainerFromIndex(0) as ToolTabStripItem
                              ?? FindVisualDescendant<ToolTabStripItem>(tabStrip);
                if (tabItem is not null)
                {
                    ExerciseInputInteractions(tabItem, includeDoubleTap: true, includeMiddlePress: true, clearInputState: false);
                    OpenAndCloseContextMenu(tabItem, tabItem.TabContextMenu);

                    var button = FindVisualDescendant<Button>(tabItem);
                    ApplyNoOpCommand(button);
                    ExerciseButtonInteractions(button);
                }
            })),
        new ControlLeakCase("DocumentTabStripItem", context =>
            new ControlSetup(new DocumentTabStripItem { DataContext = context.Document }, Array.Empty<object?>(), AfterShow: control =>
            {
                if (control.DataContext is IDockable dockable)
                {
                    dockable.CanClose = false;
                    dockable.CanFloat = false;
                }

                ExerciseInputInteractions(control, includeDoubleTap: true, includeMiddlePress: true, clearInputState: false);

                var item = (DocumentTabStripItem)control;
                OpenAndCloseContextMenu(item, item.DocumentContextMenu);

                var closeButton = FindVisualDescendant<Button>(item);
                ApplyNoOpCommand(closeButton);
                ExerciseButtonInteractions(closeButton);
            })),
        new ControlLeakCase("ToolTabStripItem", context =>
            new ControlSetup(new ToolTabStripItem { DataContext = context.Tool }, Array.Empty<object?>(), AfterShow: control =>
            {
                if (control.DataContext is IDockable dockable)
                {
                    dockable.CanClose = false;
                    dockable.CanFloat = false;
                }

                ExerciseInputInteractions(control, includeDoubleTap: true, includeMiddlePress: true, clearInputState: false);

                var item = (ToolTabStripItem)control;
                OpenAndCloseContextMenu(item, item.TabContextMenu);

                var button = FindVisualDescendant<Button>(item);
                ApplyNoOpCommand(button);
                ExerciseButtonInteractions(button);
            })),
        new ControlLeakCase("ToolPinnedControl", context =>
            new ControlSetup(new ToolPinnedControl { ItemsSource = new[] { context.Tool } }, Array.Empty<object?>())),
        new ControlLeakCase("ToolPinItemControl", context =>
        {
            var control = new ToolPinItemControl { DataContext = context.Tool };
            return new ControlSetup(control, Array.Empty<object?>(), AfterShow: pinControl =>
            {
                var toolPinItem = (ToolPinItemControl)pinControl;
                OpenAndCloseContextMenu(toolPinItem, toolPinItem.PinContextMenu);

                var button = FindVisualDescendant<Button>(toolPinItem);
                ApplyNoOpCommand(button);
                ExerciseButtonInteractions(button);
            });
        }),
        new ControlLeakCase("DragPreviewControl", _ =>
            new ControlSetup(new DragPreviewControl { PreviewContent = new Border() }, Array.Empty<object?>())),
        new ControlLeakCase("HostWindowTitleBar", _ =>
            new ControlSetup(new HostWindowTitleBar(), Array.Empty<object?>(), AfterShow: control =>
            {
                var titleBar = (HostWindowTitleBar)control;
                var captionButtons = FindTemplateChild<Control>(titleBar, "PART_CaptionButtons");
                if (captionButtons is not null)
                {
                    ExerciseInputInteractions(captionButtons, interactionMask:
                        LeakTestHelpers.InputInteractionMask.PointerEnterExit | LeakTestHelpers.InputInteractionMask.PointerMove | LeakTestHelpers.InputInteractionMask.PointerPressRelease);

                    foreach (var visual in captionButtons.GetVisualDescendants())
                    {
                        if (visual is Button button)
                        {
                            ApplyNoOpCommand(button);
                            ExerciseButtonInteractions(button);
                        }
                    }
                }
            })),
        new ControlLeakCase("MdiLayoutPanel", _ =>
            new ControlSetup(new MdiLayoutPanel(), Array.Empty<object?>())),
        new ControlLeakCase("MdiDocumentControl", context =>
            new ControlSetup(new MdiDocumentControl { DataContext = context.DocumentDock }, new object?[] { context.DocumentDock },
                BeforeCleanup: control =>
                {
                    var dock = control.DataContext as IDock;
                    control.DataContext = null;
                    DrainDispatcher();
                    if (dock is not null)
                    {
                        LeakTestHelpers.ClearFactoryCaches(dock.Factory);
                    }
                })),
        new ControlLeakCase("MdiDocumentWindow", context =>
            new ControlSetup(new MdiDocumentWindow { DataContext = context.Document }, new object?[] { context.Document, context.DocumentDock },
                BeforeCleanup: control =>
                {
                    var dockable = control.DataContext as IDockable;
                    control.DataContext = null;
                    DrainDispatcher();
                    if (dockable?.Factory is not null)
                    {
                        LeakTestHelpers.ClearFactoryCaches(dockable.Factory);
                    }
                    LeakTestHelpers.ClearFactoryCaches(context.DocumentDock.Factory);
                },
                AfterShow: control =>
            {
                DockProperties.SetIsDragEnabled(control, false);
                var header = FindTemplateChild<Control>(control, "PART_Header");
                if (header is not null)
                {
                    RaisePointerPressed(header, MouseButton.Left);
                    RaiseDoubleTapped(header);
                }

                var content = FindTemplateChild<Control>(control, "PART_ContentBorder");
                if (content is not null)
                {
                    RaisePointerPressed(content, MouseButton.Left);
                }

                var minimizeButton = FindTemplateChild<Button>(control, "PART_MinimizeButton");
                var maximizeButton = FindTemplateChild<Button>(control, "PART_MaximizeRestoreButton");
                var closeButton = FindTemplateChild<Button>(control, "PART_CloseButton");

                ApplyNoOpCommand(closeButton);
                ExerciseButtonInteractions(minimizeButton);
                ExerciseButtonInteractions(maximizeButton);
                ExerciseButtonInteractions(closeButton);

                var toolMenuButton = FindTemplateChild<Button>(control, "PART_ToolMenuButton");
                var toolPinButton = FindTemplateChild<Button>(control, "PART_ToolPinButton");
                var toolCloseButton = FindTemplateChild<Button>(control, "PART_ToolCloseButton");

                ApplyNoOpCommand(toolPinButton);
                ApplyNoOpCommand(toolCloseButton);
                ExerciseButtonInteractions(toolMenuButton);
                ExerciseButtonInteractions(toolPinButton);
                ExerciseButtonInteractions(toolCloseButton);

                if (toolMenuButton?.Flyout is FlyoutBase toolFlyout)
                {
                    OpenAndCloseFlyout(toolMenuButton, toolFlyout);
                }

                if (control is MdiDocumentWindow mdiWindow)
                {
                    OpenAndCloseContextMenu(header ?? mdiWindow, mdiWindow.DocumentContextMenu);
                }
            })),
        new ControlLeakCase("ManagedWindowLayer", context =>
            new ControlSetup(
                new ManagedWindowLayer { Dock = context.ManagedWindowDock },
                new object?[] { context.ManagedWindowDock },
                control => ((ManagedWindowLayer)control).Dock = null)),
        new ControlLeakCase("BusyOverlayControl", _ =>
        {
            var busy = new StubBusyService();
            var global = new StubGlobalBusyService();
            var control = new BusyOverlayControl { BusyService = busy, GlobalBusyService = global };
            return new ControlSetup(control, new object?[] { busy, global });
        }),
        new ControlLeakCase("DialogOverlayControl", _ =>
        {
            var dialog = new StubDialogService();
            var global = new StubGlobalDialogService();
            var control = new DialogOverlayControl { DialogService = dialog, GlobalDialogService = global };
            return new ControlSetup(control, new object?[] { dialog, global });
        }),
        new ControlLeakCase("ConfirmationOverlayControl", _ =>
        {
            var confirmation = new StubConfirmationService();
            var global = new StubGlobalConfirmationService();
            var control = new ConfirmationOverlayControl { ConfirmationService = confirmation, GlobalConfirmationService = global };
            return new ControlSetup(control, new object?[] { confirmation, global });
        }),
        new ControlLeakCase("ConfirmationDialogControl", _ =>
            new ControlSetup(new ConfirmationDialogControl(), Array.Empty<object?>())),
        new ControlLeakCase("DialogShellControl", _ =>
            new ControlSetup(new DialogShellControl(), Array.Empty<object?>())),
        new ControlLeakCase("OverlayHost", _ =>
            new ControlSetup(new OverlayHost { Content = new Border() }, Array.Empty<object?>()))
    ];
}
