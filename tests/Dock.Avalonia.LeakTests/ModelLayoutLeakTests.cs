using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ModelLayoutLeakTests
{
    [ReleaseFact]
    public void AvaloniaModel_DockControl_ChromeAndContextMenus_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var factory = new Factory();
            var layout = CreateLayout(factory);

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = layout
            };

            var window = new Window { Content = dockControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            ExerciseDockControlInteractions(dockControl);
            ExerciseInputInteractions(dockControl, includeDoubleTap: true, includeMiddlePress: true);

            var leakResult = new ModelLeakResult(
                new WeakReference(dockControl),
                new WeakReference(layout));

            CleanupWindow(window);
            return leakResult;
        });

        AssertCollected(result.ControlRef, result.LayoutRef);
    }

    [ReleaseFact]
    public void MvvmLayout_WithViews_InputInteractions_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var factory = new Dock.Model.Mvvm.Factory();
            var layout = CreateLayout(factory);

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = layout
            };

            var window = new Window { Content = dockControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            ExerciseDockControlInteractions(dockControl);
            ExerciseInputInteractions(dockControl, includeDoubleTap: true, includeMiddlePress: true);

            var leakResult = new ModelLeakResult(
                new WeakReference(dockControl),
                new WeakReference(layout));

            CleanupWindow(window);
            return leakResult;
        });

        AssertCollected(result.ControlRef, result.LayoutRef);
    }

    [ReleaseFact]
    public void ReactiveUiLayout_WithViews_InputInteractions_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var factory = new Dock.Model.ReactiveUI.Factory();
            var layout = CreateLayout(factory);

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = layout
            };

            var window = new Window { Content = dockControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            ExerciseDockControlInteractions(dockControl);
            ExerciseInputInteractions(dockControl, includeDoubleTap: true, includeMiddlePress: true);

            var leakResult = new ModelLeakResult(
                new WeakReference(dockControl),
                new WeakReference(layout));

            CleanupWindow(window);
            return leakResult;
        });

        AssertCollected(result.ControlRef, result.LayoutRef);
    }

    private static readonly InputInteractionMask ButtonInteractionMask =
        InputInteractionMask.PointerEnterExit | InputInteractionMask.PointerMove | InputInteractionMask.PointerPressRelease;

    private static readonly NoOpCommand NoOpCommandInstance = new();

    private static void ExerciseDockControlInteractions(DockControl dockControl)
    {
        var documentTabStrip = FindVisualDescendant<DocumentTabStrip>(dockControl);
        var createButton = documentTabStrip is not null
            ? FindVisualDescendant<Button>(documentTabStrip, button => button.Name == "PART_ButtonCreate")
            : null;
        ApplyNoOpCommand(createButton);
        ExerciseButtonInteractions(createButton);

        var documentTabItem = FindVisualDescendant<DocumentTabStripItem>(dockControl);
        if (documentTabItem is not null)
        {
            var closeButton = FindVisualDescendant<Button>(documentTabItem);
            ApplyNoOpCommand(closeButton);
            ExerciseButtonInteractions(closeButton);
            OpenAndCloseContextMenu(documentTabItem, documentTabItem.DocumentContextMenu);
        }

        var toolTabItem = FindVisualDescendant<ToolTabStripItem>(dockControl);
        if (toolTabItem is not null)
        {
            ExerciseInputInteractions(toolTabItem, includeDoubleTap: true, includeMiddlePress: true, interactionMask: ButtonInteractionMask);
            OpenAndCloseContextMenu(toolTabItem, toolTabItem.TabContextMenu);
        }

        var toolChrome = FindVisualDescendant<ToolChromeControl>(dockControl);
        if (toolChrome is not null)
        {
            var menuButton = FindVisualDescendant<Button>(toolChrome, button => button.Name == "PART_MenuButton");
            var pinButton = FindVisualDescendant<Button>(toolChrome, button => button.Name == "PART_PinButton");
            var maximizeButton = FindVisualDescendant<Button>(toolChrome, button => button.Name == "PART_MaximizeRestoreButton");
            var closeButton = FindVisualDescendant<Button>(toolChrome, button => button.Name == "PART_CloseButton");

            ApplyNoOpCommand(pinButton);
            ApplyNoOpCommand(closeButton);
            ExerciseButtonInteractions(menuButton);
            ExerciseButtonInteractions(pinButton);
            ExerciseButtonInteractions(maximizeButton);
            ExerciseButtonInteractions(closeButton);

            if (toolChrome.ToolFlyout is not null)
            {
                var flyoutTarget = (Control?)menuButton ?? toolChrome;
                OpenAndCloseFlyout(flyoutTarget, toolChrome.ToolFlyout);
            }
        }

        var toolPinItem = FindVisualDescendant<ToolPinItemControl>(dockControl);
        if (toolPinItem is not null)
        {
            OpenAndCloseContextMenu(toolPinItem, toolPinItem.PinContextMenu);
            var pinButton = FindVisualDescendant<Button>(toolPinItem);
            ApplyNoOpCommand(pinButton);
            ExerciseButtonInteractions(pinButton);
        }
    }

    private static void ApplyNoOpCommand(Button? button)
    {
        if (button is null)
        {
            return;
        }

        button.SetCurrentValue(Button.CommandProperty, NoOpCommandInstance);
    }

    private static void ExerciseButtonInteractions(Button? button)
    {
        if (button is null)
        {
            return;
        }

        ExerciseInputInteractions(button, interactionMask: ButtonInteractionMask);
    }

    private static IRootDock CreateLayout(IFactory factory)
    {
        var root = (IRootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables ??= factory.CreateList<IDockable>();
        root.LeftPinnedDockables ??= factory.CreateList<IDockable>();
        root.RightPinnedDockables ??= factory.CreateList<IDockable>();
        root.TopPinnedDockables ??= factory.CreateList<IDockable>();
        root.BottomPinnedDockables ??= factory.CreateList<IDockable>();
        root.Windows ??= factory.CreateList<IDockWindow>();

        var tool = factory.CreateTool();
        tool.Factory = factory;

        var document = factory.CreateDocument();
        document.Factory = factory;

        var toolDock = factory.CreateToolDock();
        toolDock.Factory = factory;
        toolDock.VisibleDockables = factory.CreateList<IDockable>(tool);
        toolDock.ActiveDockable = tool;
        tool.Owner = toolDock;

        var documentDock = factory.CreateDocumentDock();
        documentDock.Factory = factory;
        documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
        documentDock.ActiveDockable = document;
        documentDock.CanCreateDocument = false;
        documentDock.CloseButtonShowMode = DocumentCloseButtonShowMode.Always;
        documentDock.CreateDocument = null;
        document.Owner = documentDock;

        root.VisibleDockables.Add(toolDock);
        root.VisibleDockables.Add(documentDock);
        root.ActiveDockable = documentDock;
        root.DefaultDockable = documentDock;
        root.PinnedDock = toolDock;
        root.LeftPinnedDockables?.Add(tool);

        return root;
    }

    private sealed record ModelLeakResult(WeakReference ControlRef, WeakReference LayoutRef);

    private sealed class NoOpCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }
    }
}
