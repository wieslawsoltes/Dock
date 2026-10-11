using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingWindowCommandTests
{
    [AvaloniaFact]
    public void Unprefixed_Compiled_Command_Bindings_Follow_Enablement_And_Execute_With_Headless_Input()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.FloatTools();
        XamlFixture resources = new();
        Control view = ((IDataTemplate)resources.Resources["FloatingWindowCommands"]!).Build(host)!;
        view.DataContext = host;
        Window window = new() { Width = 300, Height = 150, Content = view };
        int closed = 0;
        int hidden = 0;
        ProbeCommand close = new(value => ReferenceEquals(value, host.Model), _ => closed++);
        ProbeCommand hide = new(value => ReferenceEquals(value, host.Model), _ => hidden++);
        workspace.Item(workspace.First).CloseCommand = workspace.Item(workspace.Second).CloseCommand = close;
        workspace.Item(workspace.First).HideCommand = workspace.Item(workspace.Second).HideCommand = hide;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Button closeButton = view.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("window-close"));
            Button hideButton = view.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("window-hide"));
            Assert.Same(host.CloseWindowCommand, closeButton.Command);
            Assert.Same(host.HideWindowCommand, hideButton.Command);
            Click(closeButton);
            Click(hideButton);
            Assert.Equal(2, closed);
            Assert.Equal(2, hidden);
            close.Enabled = false;
            Assert.False(closeButton.IsEffectivelyEnabled);
            Click(closeButton);
            Assert.Equal(2, closed);
            close.Enabled = true;
            Assert.True(closeButton.IsEffectivelyEnabled);
            Click(closeButton);
            Assert.Equal(4, closed);
        }
        finally { window.Close(); resources.Close(); }
    }

    private static void Click(Button button)
    {
        TopLevel window = TopLevel.GetTopLevel(button)!;
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Point point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Window_Commands_Check_All_Current_Items_And_Forward_The_Caller_Parameter(bool hide)
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.FloatTools();
        object parameter = new();
        List<LayoutAnchorable> executed = new();
        ProbeCommand first = new(value => ReferenceEquals(value, parameter), _ => executed.Add(workspace.First));
        ProbeCommand second = new(value => ReferenceEquals(value, parameter), _ => executed.Add(workspace.Second));
        LayoutAnchorableItem firstItem = workspace.Item(workspace.First);
        LayoutAnchorableItem secondItem = workspace.Item(workspace.Second);
        if (hide) { firstItem.HideCommand = first; secondItem.HideCommand = second; }
        else { firstItem.CloseCommand = first; secondItem.CloseCommand = second; }
        ICommand command = hide ? host.HideWindowCommand : host.CloseWindowCommand;
        Assert.False(command.CanExecute(new object()));
        Assert.True(command.CanExecute(parameter));
        command.Execute(parameter);
        Assert.Equal(new[] { workspace.First, workspace.Second }, executed);
        Assert.True(host.IsVisible);
        int changes = 0;
        command.CanExecuteChanged += (_, _) => changes++;
        second.Enabled = false;
        Assert.True(changes > 0);
        Assert.False(command.CanExecute(parameter));
        Assert.Equal(2, executed.Count);
        second.Enabled = true;
        if (hide) workspace.Second.CanHide = false;
        else workspace.Second.CanClose = false;
        Assert.False(command.CanExecute(parameter));
        if (hide) { workspace.Second.CanHide = true; secondItem.HideCommand = null; }
        else { workspace.Second.CanClose = true; secondItem.CloseCommand = null; }
        Assert.False(command.CanExecute(parameter));
        Assert.Equal(0, second.Subscribers);
    }

    [AvaloniaFact]
    public void Direct_Window_Command_Execution_Delegates_Even_When_CanExecute_Is_False()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.FloatTools();
        object parameter = new();
        int calls = 0;
        ProbeCommand command = new(_ => false, value => { Assert.Same(parameter, value); calls++; });
        workspace.Item(workspace.First).CloseCommand = command;
        workspace.Item(workspace.Second).CloseCommand = command;
        Assert.False(host.CloseWindowCommand.CanExecute(parameter));
        host.CloseWindowCommand.Execute(parameter);
        Assert.Equal(2, calls);
        Assert.True(host.IsVisible);
    }

    [AvaloniaFact]
    public void Closing_A_Snapshot_Resolves_Each_Command_Again_After_Earlier_Commands_Move_Content()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.FloatTools();
        object parameter = new();
        List<string> executed = new();
        ProbeCommand replacement = new(value => ReferenceEquals(value, parameter), _ =>
        {
            executed.Add("replacement");
            workspace.Second.Close();
        });
        ProbeCommand original = new(_ => true, _ => executed.Add("original"));
        workspace.Item(workspace.Second).CloseCommand = original;
        ProbeCommand first = new(_ => true, value =>
        {
            Assert.Same(parameter, value);
            executed.Add("first");
            workspace.Item(workspace.Second).CloseCommand = replacement;
            workspace.First.Close();
        });
        workspace.Item(workspace.First).CloseCommand = first;
        ICommand command = host.CloseWindowCommand;
        command.Execute(parameter);
        Assert.Equal(new[] { "first", "replacement" }, executed);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Null(workspace.First.Parent);
        Assert.Null(workspace.Second.Parent);
        Assert.False(command.CanExecute(parameter));
        Assert.Equal(0, first.Subscribers);
        Assert.Equal(0, original.Subscribers);
        Assert.Equal(0, replacement.Subscribers);
    }

    [AvaloniaFact]
    public void Window_Commands_Update_For_Added_And_Removed_Tools()
    {
        using Workspace workspace = new();
        workspace.First.Float();
        LayoutAnchorableFloatingWindowControl host = (LayoutAnchorableFloatingWindowControl)workspace.First.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
        ICommand close = host.CloseWindowCommand;
        Assert.True(close.CanExecute(null));
        LayoutAnchorable third = new() { CanClose = false };
        LayoutAnchorablePane pane = (LayoutAnchorablePane)workspace.First.Parent!;
        pane.Children.Add(third);
        Assert.False(close.CanExecute(null));
        third.CanClose = true;
        Assert.True(close.CanExecute(null));
        third.CanClose = false;
        pane.Children.Remove(third);
        Assert.True(close.CanExecute(null));
    }

    [AvaloniaFact]
    public void Native_Document_Close_Uses_The_Current_Command_And_Respects_Its_Enablement()
    {
        using Workspace workspace = new();
        workspace.Document.Float();
        LayoutFloatingWindowControl host = (LayoutFloatingWindowControl)workspace.Document.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        int calls = 0;
        ProbeCommand command = new(value => value is null, _ => calls++);
        item.CloseCommand = command;
        command.Enabled = false;
        host.Close();
        Assert.Equal(0, calls);
        Assert.True(host.IsVisible);
        command.Enabled = true;
        host.Close();
        Assert.Equal(1, calls);
        Assert.True(host.IsVisible);
        Assert.Same(workspace.Manager.Layout, workspace.Document.Root);
        item.CloseCommand = new ProbeCommand(_ => true, _ => workspace.Document.Close());
        host.Close();
        Assert.False(host.IsVisible);
        Assert.Empty(workspace.Manager.FloatingWindows);
    }

    [AvaloniaFact]
    public void Native_Tool_Close_Checks_Factory_Veto_Before_Falling_Back_To_Current_Hide_Commands()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.FloatTools();
        int closed = 0;
        int hidden = 0;
        ProbeCommand close = new(_ => false, _ => closed++);
        workspace.Item(workspace.First).CloseCommand = close;
        workspace.Item(workspace.First).HideCommand = new ProbeCommand(value => value is null, _ => { hidden++; workspace.First.Hide(); });
        workspace.Item(workspace.Second).HideCommand = new ProbeCommand(value => value is null, _ => { hidden++; workspace.Second.Hide(); });
        EventHandler<Dock.Model.Core.Events.WindowClosingEventArgs> veto = (_, args) => args.Cancel = true;
        workspace.Manager.DockFactory.WindowClosing += veto;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.Equal(0, hidden);
        workspace.Manager.DockFactory.WindowClosing -= veto;
        host.Close();
        Assert.Equal(0, closed);
        Assert.Equal(2, hidden);
        Assert.False(host.IsVisible);
        Assert.Equal(new[] { workspace.First, workspace.Second }, workspace.Manager.Layout.Hidden);
        Assert.False(host.HideWindowCommand.CanExecute(null));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Default_Window_Commands_Preserve_Content_Cancellation(bool hide)
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.FloatTools();
        if (hide) workspace.First.Hiding += (_, args) => args.Cancel = true;
        else workspace.First.Closing += (_, args) => args.Cancel = true;
        ICommand command = hide ? host.HideWindowCommand : host.CloseWindowCommand;
        command.Execute(null);
        Assert.True(host.IsVisible);
        Assert.Same(host.Model, workspace.First.FindParent<LayoutFloatingWindow>());
        Assert.False(workspace.First.IsHidden);
        if (hide) Assert.Contains(workspace.Second, workspace.Manager.Layout.Hidden);
        else Assert.Null(workspace.Second.Parent);
    }

    [AvaloniaFact]
    public void Retained_Window_And_Application_Commands_Release_Closed_Managers()
    {
        (ICommand command, ProbeCommand application, WeakReference manager) = CreateClosedWorkspace();
        for (int i = 0; i < 8 && manager.IsAlive; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.False(command.CanExecute(null));
        Assert.Equal(0, application.Subscribers);
        Assert.False(manager.IsAlive);
        GC.KeepAlive(command);
        GC.KeepAlive(application);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ICommand, ProbeCommand, WeakReference) CreateClosedWorkspace()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.FloatTools();
        ProbeCommand application = new(_ => true, _ => { });
        workspace.Item(workspace.First).CloseCommand = application;
        return (host.CloseWindowCommand, application, new WeakReference(workspace.Manager));
    }

    private sealed class ProbeCommand(Func<object?, bool> canExecute, Action<object?> execute) : ICommand
    {
        private EventHandler? _changed;
        private bool _enabled = true;
        public int Subscribers { get; private set; }
        public event EventHandler? CanExecuteChanged
        {
            add { _changed += value; Subscribers++; }
            remove { _changed -= value; Subscribers--; }
        }
        public bool Enabled
        {
            get => _enabled;
            set { _enabled = value; _changed?.Invoke(this, EventArgs.Empty); }
        }
        public bool CanExecute(object? parameter) => Enabled && canExecute(parameter);
        public void Execute(object? parameter) => execute(parameter);
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new();
        internal LayoutDocument Document { get; } = new() { Content = "Document" };
        internal LayoutAnchorable First { get; } = new() { CanClose = true, Content = "First tool" };
        internal LayoutAnchorable Second { get; } = new() { CanClose = true, Content = "Second tool" };
        private Window View { get; }

        internal Workspace()
        {
            LayoutAnchorablePane tools = new(First);
            tools.Children.Add(Second);
            LayoutPanel panel = new(new LayoutDocumentPane(Document));
            panel.Children.Add(tools);
            Manager.Layout = new() { RootPanel = panel };
            View = new() { Width = 800, Height = 500, Content = Manager };
            View.Show();
            Dispatcher.UIThread.RunJobs();
        }
        internal LayoutAnchorableItem Item(LayoutAnchorable tool) => (LayoutAnchorableItem)Manager.GetLayoutItemFromModel(tool)!;
        internal LayoutAnchorableFloatingWindowControl FloatTools()
        {
            Manager.DockFactory.FloatAllDockables(First.DockModel);
            Dispatcher.UIThread.RunJobs();
            return (LayoutAnchorableFloatingWindowControl)Assert.Single(Manager.FloatingWindows);
        }
        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
