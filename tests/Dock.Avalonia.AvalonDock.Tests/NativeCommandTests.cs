using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using ReactiveUI.Reactive;
using Xunit;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.AvalonDock.Tests;

public class NativeCommandTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Close_Buttons_Invoke_Compiled_Container_Style_Commands(bool floating)
    {
        using Workspace workspace = new();
        ContainerStyleFixture resources = new();
        StyledContentViewModel documentContent = new("Document command");
        StyledContentViewModel toolContent = new("Tool command");
        workspace.Document.Content = documentContent;
        workspace.Tool.Content = toolContent;
        workspace.OtherDocument.Content = new StyledContentViewModel("Other document");
        workspace.OtherTool.Content = new StyledContentViewModel("Other tool");
        workspace.Manager.LayoutItemContainerStyle = (ControlTheme)resources.Resources["CommandStyle"]!;
        if (floating)
        {
            workspace.Document.Float();
            workspace.Tool.Float();
        }
        workspace.Update();
        Button documentClose = DocumentClose(Scope(workspace, workspace.Document), workspace.Document);
        Button toolClose = ToolButton(Scope(workspace, workspace.Tool), "PART_CloseButton");
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Document), documentClose.DataContext);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Tool), toolClose.DataContext);
        Click(documentClose);
        Click(toolClose);
        Assert.Equal(1, documentContent.CloseRequests);
        Assert.Equal(1, toolContent.CloseRequests);
        Assert.NotNull(workspace.Document.Root);
        Assert.NotNull(workspace.Tool.Root);
    }

    [AvaloniaFact]
    public void Native_Close_Button_Tracks_Command_Replacement_And_CanExecute()
    {
        using Workspace workspace = new();
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        System.Windows.Input.ICommand defaultClose = item.CloseCommand!;
        using BehaviorSubject<bool> enabled = new(true);
        int requests = 0;
        using var command = ReactiveCommand.Create<object?>(parameter =>
        {
            Assert.Null(parameter);
            requests++;
        }, enabled, ImmediateScheduler.Instance);
        item.CloseCommand = command;
        Button close = DocumentClose(workspace.Manager, workspace.Document);
        workspace.Update();
        Click(close);
        Assert.Equal(1, requests);
        enabled.OnNext(false);
        workspace.Update();
        Assert.False(close.IsEffectivelyEnabled);
        Assert.False(close.IsVisible);
        enabled.OnNext(true);
        workspace.Update();
        Click(close);
        Assert.Equal(2, requests);
        item.CloseCommand = defaultClose;
        workspace.Update();
        Assert.NotSame(command, close.Command);
        Click(close);
        Assert.Null(workspace.Document.Root);
    }

    [AvaloniaFact]
    public void Native_Tool_Close_Uses_Hide_When_Cannot_Close_And_Observes_Active_Tool()
    {
        using Workspace workspace = new();
        workspace.Tool.CanClose = false;
        Button close = ToolButton(workspace.Manager, "PART_CloseButton");
        LayoutAnchorableItem item = Assert.IsType<LayoutAnchorableItem>(close.DataContext);
        Assert.Same(item.HideCommand, close.Command);
        bool cancel = true;
        workspace.Tool.Hiding += (_, e) => e.Cancel = cancel;
        Click(close);
        Assert.False(workspace.Tool.IsHidden);
        cancel = false;
        Click(close);
        Assert.True(workspace.Tool.IsHidden);
        workspace.OtherTool.IsActive = true;
        workspace.Update();
        close = ToolButton(workspace.Manager, "PART_CloseButton");
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.OtherTool), close.DataContext);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.OtherTool)!.CloseCommand, close.Command);
        Click(close);
        Assert.Null(workspace.OtherTool.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Context_Menus_Use_The_Selected_Item_And_Observe_Command_Changes(bool tool)
    {
        using Workspace workspace = new();
        LayoutContent content = tool ? workspace.Tool : workspace.Document;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(content)!;
        int requests = 0;
        using var command = ReactiveCommand.Create(() => requests++, outputScheduler: ImmediateScheduler.Instance);
        item.CloseCommand = command;
        Control tab = Tab(workspace.Manager, content);
        ContextMenu menu = tool ? ((ToolTabStripItem)tab).TabContextMenu! : ((DocumentTabStripItem)tab).DocumentContextMenu!;
        Click(tab, MouseButton.Right);
        Assert.True(menu.IsOpen);
        Assert.Same(item, menu.DataContext);
        MenuItem close = menu.Items.OfType<MenuItem>().Single(entry => ReferenceEquals(entry.Command, command));
        Click(close);
        Assert.Equal(1, requests);
        Assert.NotNull(content.Root);
        menu.Close();
    }

    [AvaloniaFact]
    public void Native_Tool_Flyout_And_Pin_Button_Invoke_Current_Commands()
    {
        using Workspace workspace = new();
        LayoutAnchorableItem item = (LayoutAnchorableItem)workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!;
        int closeRequests = 0;
        int pinRequests = 0;
        using var closeCommand = ReactiveCommand.Create(() => closeRequests++, outputScheduler: ImmediateScheduler.Instance);
        using var pinCommand = ReactiveCommand.Create(() => pinRequests++, outputScheduler: ImmediateScheduler.Instance);
        item.CloseCommand = closeCommand;
        item.AutoHideCommand = pinCommand;
        Click(ToolButton(workspace.Manager, "PART_PinButton"));
        Assert.Equal(1, pinRequests);
        Button button = ToolButton(workspace.Manager, "PART_MenuButton");
        MenuFlyout menu = Assert.IsType<MenuFlyout>(button.Flyout);
        Click(button);
        Assert.True(menu.IsOpen);
        MenuItem close = menu.Items.OfType<MenuItem>().Single(entry => ReferenceEquals(entry.Command, closeCommand));
        Assert.Same(item, close.DataContext);
        Click(close);
        Assert.Equal(1, closeRequests);
        menu.Hide();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Tab_Middle_Click_Uses_Custom_Close_And_Disabled_Commands_Do_Not_Fall_Through(bool tool)
    {
        using Workspace workspace = new();
        LayoutContent content = tool ? workspace.Tool : workspace.Document;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(content)!;
        using BehaviorSubject<bool> enabled = new(true);
        int requests = 0;
        using var command = ReactiveCommand.Create(() => requests++, enabled, ImmediateScheduler.Instance);
        item.CloseCommand = command;
        Control tab = Tab(workspace.Manager, content);
        Click(tab, MouseButton.Middle);
        Assert.Equal(1, requests);
        Assert.NotNull(content.Root);
        enabled.OnNext(false);
        Click(tab, MouseButton.Middle);
        Assert.Equal(1, requests);
        Assert.NotNull(content.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Double_Click_Uses_Custom_Float_And_Respects_Disabled_Commands(bool tool)
    {
        using Workspace workspace = new();
        LayoutContent content = tool ? workspace.Tool : workspace.Document;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(content)!;
        using BehaviorSubject<bool> enabled = new(true);
        int requests = 0;
        using var command = ReactiveCommand.Create(() => requests++, enabled, ImmediateScheduler.Instance);
        item.FloatCommand = command;
        Control tab = Tab(workspace.Manager, content);
        Click(tab);
        Click(tab);
        Assert.Equal(1, requests);
        Assert.False(content.IsFloating);
        enabled.OnNext(false);
        Click(tab);
        Click(tab);
        Assert.Equal(1, requests);
        Assert.False(content.IsFloating);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Default_Tab_Double_Click_Floats_Through_The_Native_Factory(bool tool)
    {
        using Workspace workspace = new();
        LayoutContent content = tool ? workspace.Tool : workspace.Document;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(content)!;
        Control tab = Tab(workspace.Manager, content);
        Click(tab);
        Click(tab);
        Assert.True(content.IsFloating);
        Assert.True(Assert.IsAssignableFrom<HostWindow>(Scope(workspace, content)).IsVisible);
        Assert.True(content.IsActive);
        Assert.Same(item, workspace.Manager.GetLayoutItemFromModel(content));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Default_Close_On_A_Floating_Tool_Honors_Cancellation_And_Hide_Fallback(bool canClose)
    {
        using Workspace workspace = new();
        workspace.Tool.CanClose = canClose;
        workspace.Tool.Float();
        workspace.Update();
        HostWindow host = Assert.IsAssignableFrom<HostWindow>(Scope(workspace, workspace.Tool));
        Button close = ToolButton(host, "PART_CloseButton");
        bool cancel = true;
        workspace.Tool.Closing += (_, e) => e.Cancel = cancel;
        workspace.Tool.Hiding += (_, e) => e.Cancel = cancel;
        Click(close);
        Assert.True(host.IsVisible);
        Assert.Same(workspace.Manager.Layout, workspace.Tool.Root);
        Assert.False(workspace.Tool.IsHidden);
        cancel = false;
        Click(close);
        Assert.False(host.IsVisible);
        if (canClose) Assert.Null(workspace.Tool.Root);
        else Assert.True(workspace.Tool.IsHidden);
    }

    [AvaloniaFact]
    public void Native_Menus_Float_And_Redock_Documents_And_Restore_Auto_Hidden_Tools()
    {
        using Workspace workspace = new();
        LayoutItem documentItem = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        DocumentTabStripItem tab = (DocumentTabStripItem)Tab(workspace.Manager, workspace.Document);
        Click(tab, MouseButton.Right);
        Click(tab.DocumentContextMenu!.Items.OfType<MenuItem>().Single(item => ReferenceEquals(item.Command, documentItem.FloatCommand)));
        Assert.True(workspace.Document.IsFloating);
        workspace.Update();
        tab = (DocumentTabStripItem)Tab(Scope(workspace, workspace.Document), workspace.Document);
        Click(tab, MouseButton.Right);
        Assert.True(documentItem.DockAsDocumentCommand!.CanExecute(null), workspace.Document.Parent?.GetType().FullName);
        Click(tab.DocumentContextMenu!.Items.OfType<MenuItem>().Single(item => ReferenceEquals(item.Command, documentItem.DockAsDocumentCommand)));
        Assert.False(workspace.Document.IsFloating);
        Click(ToolButton(workspace.Manager, "PART_PinButton"));
        Assert.True(workspace.Tool.IsAutoHidden);
        ToolPinItemControl pin = workspace.Manager.GetVisualDescendants().OfType<ToolPinItemControl>()
            .Single(control => ReferenceEquals(control.DataContext, workspace.Tool.DockModel));
        Click(pin, MouseButton.Right);
        Assert.True(pin.PinContextMenu!.IsOpen);
        LayoutAnchorableItem toolItem = (LayoutAnchorableItem)workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!;
        Assert.Same(toolItem, pin.PinContextMenu.DataContext);
        Click(pin.PinContextMenu.Items.OfType<MenuItem>().Single(item => ReferenceEquals(item.Command, toolItem.AutoHideCommand)));
        Assert.False(workspace.Tool.IsAutoHidden);
        Assert.IsType<LayoutAnchorablePane>(workspace.Tool.Parent);
    }

    [AvaloniaFact]
    public void Open_Tool_Flyout_Tracks_The_Active_Content_And_Command_Replacement()
    {
        using Workspace workspace = new();
        Button button = ToolButton(workspace.Manager, "PART_MenuButton");
        MenuFlyout flyout = Assert.IsType<MenuFlyout>(button.Flyout);
        Click(button);
        LayoutItem first = workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!;
        MenuItem close = flyout.Items.OfType<MenuItem>().Single(entry => ReferenceEquals(entry.Command, first.CloseCommand));
        workspace.OtherTool.IsActive = true;
        workspace.Update();
        LayoutItem second = workspace.Manager.GetLayoutItemFromModel(workspace.OtherTool)!;
        Assert.Same(second, close.DataContext);
        int requests = 0;
        using var command = ReactiveCommand.Create(() => requests++, outputScheduler: ImmediateScheduler.Instance);
        second.CloseCommand = command;
        Assert.Same(command, close.Command);
        Click(close);
        Assert.Equal(1, requests);
        Assert.NotNull(workspace.OtherTool.Root);
    }

    [AvaloniaFact]
    public void Programmatic_Model_And_Factory_Operations_Do_Not_Execute_Application_UI_Commands()
    {
        using Workspace workspace = new();
        int requests = 0;
        using var command = ReactiveCommand.Create(() => requests++, outputScheduler: ImmediateScheduler.Instance);
        workspace.Manager.GetLayoutItemFromModel(workspace.Document)!.CloseCommand = command;
        workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!.CloseCommand = command;
        workspace.Document.Close();
        workspace.Manager.DockFactory.CloseDockable(workspace.Tool.DockModel);
        Assert.Null(workspace.Document.Root);
        Assert.Null(workspace.Tool.Root);
        Assert.Equal(0, requests);
    }

    [AvaloniaFact]
    public void Native_Capability_Policy_Still_Gates_Custom_Buttons_And_Gestures()
    {
        using Workspace workspace = new();
        using var command = ReactiveCommand.Create(() => throw new InvalidOperationException("Disabled UI must not execute this command."),
            outputScheduler: ImmediateScheduler.Instance);
        LayoutItem document = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        LayoutAnchorableItem tool = (LayoutAnchorableItem)workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!;
        document.CloseCommand = document.FloatCommand = command;
        tool.CloseCommand = tool.FloatCommand = tool.AutoHideCommand = command;
        Button documentClose = DocumentClose(workspace.Manager, workspace.Document);
        DockCapabilityPolicy policy = new() { CanClose = false, CanFloat = false, CanPin = false };
        ((IDock)workspace.Document.DockModel.Owner!).DockCapabilityPolicy = policy;
        ((IDock)workspace.Tool.DockModel.Owner!).DockCapabilityPolicy = policy;
        workspace.Update();
        Assert.False(documentClose.IsEffectivelyVisible);
        Assert.False(ToolButton(workspace.Manager, "PART_CloseButton").IsVisible);
        Assert.False(ToolButton(workspace.Manager, "PART_PinButton").IsVisible);
        foreach (LayoutContent content in new LayoutContent[] { workspace.Document, workspace.Tool })
        {
            Control tab = Tab(workspace.Manager, content);
            Click(tab, MouseButton.Middle);
            Click(tab);
            Click(tab);
            Assert.False(content.IsFloating);
            Assert.NotNull(content.Root);
        }
        ((IDock)workspace.Document.DockModel.Owner!).DockCapabilityPolicy = null;
        ((IDock)workspace.Tool.DockModel.Owner!).DockCapabilityPolicy = null;
        workspace.Update();
        Assert.True(documentClose.IsEffectivelyVisible);
        Assert.True(ToolButton(workspace.Manager, "PART_CloseButton").IsVisible);
        Assert.True(ToolButton(workspace.Manager, "PART_PinButton").IsVisible);
    }

    [AvaloniaFact]
    public void DockAsDocument_Uses_Last_Focused_Main_Pane_Or_Creates_One_Using_Native_Models()
    {
        using Workspace workspace = new();
        LayoutDocumentPane destination = new(new LayoutDocument { Title = "Focused destination" });
        workspace.Manager.Layout.RootPanel.Children.Add(destination);
        destination.Children[0].IsActive = true;
        workspace.Tool.DockAsDocument();
        Assert.Same(destination, workspace.Tool.Parent);
        Assert.Same(destination.DockModel, workspace.Tool.DockModel.Owner);
        Assert.True(workspace.Tool.IsActive);
        Assert.True(workspace.Tool.IsSelected);

        LayoutAnchorable tool = new() { Title = "New document pane" };
        LayoutPanel original = new()
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                new LayoutAnchorablePane { Children = { tool, new LayoutAnchorable { Title = "Keep first pane" } } },
                new LayoutAnchorablePane(new LayoutAnchorable { Title = "Keep second pane" })
            }
        };
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = original } };
        tool.DockAsDocument();
        Assert.IsType<LayoutDocumentPane>(tool.Parent);
        Assert.NotSame(original, manager.Layout.RootPanel);
        Assert.Equal(Orientation.Horizontal, manager.Layout.RootPanel.Orientation);
        Assert.Same(original, manager.Layout.RootPanel.Children[0]);
        Assert.Equal(Orientation.Vertical, original.Orientation);
        Assert.Throws<InvalidOperationException>(() => new LayoutDocument().DockAsDocument());
    }

    [AvaloniaFact]
    public async Task Opened_Menus_And_Application_Commands_Do_Not_Retain_A_Closed_Manager()
    {
        StyledContentViewModel retained = new("Retained command");
        Styles retainedStyles = Application.Current!.Styles;
        WeakReference<DockingManager> reference = OpenMenusAndRelease(retained);
        // Avalonia's hold gesture timer retains the last pressed visual until
        // its deadline even when MouseUp has canceled the gesture.
        await Task.Delay(Application.Current!.PlatformSettings!.HoldWaitDuration + TimeSpan.FromMilliseconds(50));
        Dispatcher.UIThread.RunJobs();
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.False(reference.TryGetTarget(out _));
        GC.KeepAlive(retained);
        GC.KeepAlive(retainedStyles);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<DockingManager> OpenMenusAndRelease(StyledContentViewModel retained)
    {
        using Workspace workspace = new();
        workspace.Manager.GetLayoutItemFromModel(workspace.Document)!.CloseCommand = retained.CloseCommand;
        workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!.CloseCommand = retained.CloseCommand;
        DocumentTabStripItem document = (DocumentTabStripItem)Tab(workspace.Manager, workspace.Document);
        Click(document, MouseButton.Right);
        document.DocumentContextMenu!.Close();
        ToolTabStripItem tool = (ToolTabStripItem)Tab(workspace.Manager, workspace.Tool);
        Click(tool, MouseButton.Right);
        tool.TabContextMenu!.Close();
        Button button = ToolButton(workspace.Manager, "PART_MenuButton");
        Click(button);
        button.Flyout!.Hide();
        return new(workspace.Manager);
    }

    private static Control Scope(Workspace workspace, LayoutContent content) => content.IsFloating
        ? Assert.IsAssignableFrom<Control>(content.FindParent<LayoutFloatingWindow>()!.DockWindow.Host)
        : workspace.Manager;

    private static Button DocumentClose(Control scope, LayoutContent content) =>
        scope.GetVisualDescendants().OfType<Button>().Single(button => button.IsEffectivelyVisible && button.Classes.Contains("avalondock-close") &&
            button.DataContext is LayoutItem item && ReferenceEquals(item.LayoutElement, content));

    private static Button ToolButton(Control scope, string name) =>
        scope.GetVisualDescendants().OfType<Button>().Single(button => button.Name == name);

    private static Control Tab(Control scope, LayoutContent content) =>
        scope.GetVisualDescendants().OfType<Control>().Single(control =>
            control is DocumentTabStripItem or ToolTabStripItem && ReferenceEquals(control.DataContext, content.DockModel));

    private static void Click(Control control, MouseButton button = MouseButton.Left)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(control)!;
        topLevel.UpdateLayout();
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), topLevel)!.Value;
        topLevel.MouseMove(point);
        IInputElement? hit = topLevel.InputHitTest(point);
        Assert.True(ReferenceEquals(hit, control) || hit is Visual visual && visual.GetVisualAncestors().Contains(control),
            $"Click on {control.GetType().Name} at {point} hit {hit?.GetType().Name}: " +
            string.Join("/", (hit as Visual)?.GetVisualAncestors().Select(v => v.GetType().Name) ?? Array.Empty<string>()));
        topLevel.MouseDown(point, button);
        topLevel.MouseUp(point, button);
        Dispatcher.UIThread.RunJobs();
        topLevel.UpdateLayout();
    }

    private sealed class Workspace : IDisposable
    {
        public LayoutDocument Document { get; } = new() { Title = "First document", Content = "First document" };
        public LayoutDocument OtherDocument { get; } = new() { Title = "Second document", Content = "Second document" };
        public LayoutAnchorable Tool { get; } = new() { Title = "First tool", Content = "First tool", CanClose = true };
        public LayoutAnchorable OtherTool { get; } = new() { Title = "Second tool", Content = "Second tool", CanClose = true };
        public DockingManager Manager { get; }
        public Window Window { get; }

        public Workspace()
        {
            Manager = new DockingManager
            {
                Layout = new LayoutRoot
                {
                    RootPanel = new LayoutPanel
                    {
                        Children =
                        {
                            new LayoutDocumentPane { Children = { Document, OtherDocument } },
                            new LayoutAnchorablePane { Children = { Tool, OtherTool } }
                        }
                    }
                }
            };
            Document.IsActive = true;
            Tool.IsSelected = true;
            Window = new Window { Content = Manager, Width = 1100, Height = 600 };
            Window.Show();
            Update();
        }

        public void Update()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            foreach (LayoutFloatingWindow floating in Manager.Layout.FloatingWindows)
                if (floating.DockWindow.Host is Window window) window.UpdateLayout();
        }

        public void Dispose()
        {
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
