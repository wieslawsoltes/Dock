using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DirectCommandExecutionTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Default_Command_Executes_Its_Action_Even_When_CanExecute_Is_False(bool useParameter)
    {
        ItemProbe item = new();
        item.Initialize();
        int calls = 0;
        item.Closing = () => calls++;
        Assert.False(item.CloseCommand!.CanExecute(null));
        item.CloseCommand.Execute(useParameter ? new object() : null);
        Assert.Equal(1, calls);
    }

    [AvaloniaFact]
    public void Default_Float_Execution_Does_Not_Call_The_Protected_NoOp_Hook()
    {
        ItemProbe item = new();
        item.Initialize();
        item.InvokeBaseFloat();
        Assert.Equal(0, item.FloatingCalls);
        Assert.False(item.FloatCommand!.CanExecute(null));
        Assert.Throws<NullReferenceException>(() => item.FloatCommand.Execute(null));
        Assert.Equal(0, item.FloatingCalls);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Default_Command_Propagates_The_Original_Action_Error_Synchronously_And_Remains_Usable(bool arbitraryParameter)
    {
        ItemProbe item = new();
        item.Initialize();
        InvalidOperationException failure = new("application close");
        item.Closing = () => ThrowFromApplication(failure);
        object? parameter = arbitraryParameter ? "ignored item command parameter" : null;
        Exception actual = Assert.Throws<InvalidOperationException>(() => item.CloseCommand!.Execute(parameter));
        Assert.Same(failure, actual);
        Assert.Contains(nameof(ThrowFromApplication), actual.StackTrace);
        int calls = 0;
        item.Closing = () => calls++;
        item.CloseCommand!.Execute(parameter);
        Assert.Equal(1, calls);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Nested_Default_Command_Invocations_Have_Independent_Error_Results()
    {
        ItemProbe item = new();
        item.Initialize();
        InvalidOperationException inner = new("inner");
        InvalidOperationException outer = new("outer");
        int depth = 0;
        List<string> calls = new();
        item.Closing = () =>
        {
            if (depth++ == 0)
            {
                Assert.Same(inner, Assert.Throws<InvalidOperationException>(() => item.CloseCommand!.Execute(null)));
                calls.Add("outer");
                throw outer;
            }
            calls.Add("inner");
            throw inner;
        };
        Assert.Same(outer, Assert.Throws<InvalidOperationException>(() => item.CloseCommand!.Execute(null)));
        Assert.Equal(new[] { "inner", "outer" }, calls);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void An_Error_After_Container_Detachment_Is_Still_Returned_To_The_Executing_Caller()
    {
        LayoutDocument document = new();
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        ICommand command = manager.GetLayoutItemFromModel(document)!.CloseCommand!;
        InvalidOperationException failure = new("detached during execution");
        manager.DocumentClosing += (_, _) => { manager.Layout = new(); throw failure; };
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => command.Execute(null)));
        Assert.False(command.CanExecute(null));
        command.Execute(null);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Direct_Close_And_Hide_Ignore_Capability_Flags_But_Retain_Content_Cancellation(bool hide)
    {
        LayoutDocument document = new() { CanClose = false };
        LayoutAnchorable tool = new() { CanHide = false };
        LayoutPanel panel = new(new LayoutDocumentPane(document));
        panel.Children.Add(new LayoutAnchorablePane(tool));
        DockingManager manager = new() { Layout = new() { RootPanel = panel } };
        ICommand command = hide ? ((LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!).HideCommand! : manager.GetLayoutItemFromModel(document)!.CloseCommand!;
        bool cancel = true;
        int callbacks = 0;
        document.Closing += (_, e) => { callbacks++; e.Cancel = cancel; };
        tool.Hiding += (_, e) => { callbacks++; e.Cancel = cancel; };
        Assert.False(command.CanExecute(null));
        command.Execute(null);
        Assert.Equal(1, callbacks);
        Assert.Same(manager.Layout, (hide ? (LayoutContent)tool : document).Root);
        Assert.False(tool.IsHidden);
        cancel = false;
        command.Execute(null);
        Assert.Equal(2, callbacks);
        if (hide) Assert.True(tool.IsHidden);
        else Assert.Null(document.Root);
        manager.Layout = new();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Floating_Command_Propagates_The_First_Error_And_Stops_Later_Tools(bool hide)
    {
        using Workspace workspace = new();
        InvalidOperationException failure = new("tool command");
        int later = 0;
        int queries = 0;
        workspace.Set(workspace.First, hide, new Command(() => { queries++; return false; }, _ => throw failure));
        workspace.Set(workspace.Second, hide, new Command(() => true, _ => later++));
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => workspace.Command(hide).Execute(new object())));
        Assert.Equal(0, queries);
        Assert.Equal(0, later);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void A_Floating_Command_Fails_If_A_Current_Child_Command_Is_Missing(bool hide, bool removedByEarlierCommand)
    {
        using Workspace workspace = new();
        int calls = 0;
        workspace.Set(workspace.First, hide, removedByEarlierCommand
            ? new Command(() => true, _ => { calls++; workspace.Set(workspace.Second, hide, null); }) : null);
        workspace.Set(workspace.Second, hide, new Command(() => true, _ => calls++));
        Assert.Throws<NullReferenceException>(() => workspace.Command(hide).Execute(null));
        Assert.Equal(removedByEarlierCommand ? 1 : 0, calls);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false, "null")]
    [InlineData(false, "rootless")]
    [InlineData(false, "unowned")]
    [InlineData(false, "layout-root")]
    [InlineData(true, "null")]
    [InlineData(true, "rootless")]
    [InlineData(true, "unowned")]
    [InlineData(true, "layout-root")]
    public void Direct_Floating_Execution_Retains_Missing_Context_Failures(bool hide, string context)
    {
        using Workspace workspace = new();
        workspace.Host.Current = context switch
        {
            "rootless" => new LayoutPanel(),
            "layout-root" => workspace.Manager.Layout,
            "unowned" => new LayoutRoot { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) }.RootPanel,
            _ => null
        };
        Assert.False(workspace.Command(hide).CanExecute(null));
        Assert.Throws<NullReferenceException>(() => workspace.Command(hide).Execute(null));
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Empty_Managerless_Layout_Does_Not_Dereference_A_Manager_During_Execution(bool hide)
    {
        using Workspace workspace = new();
        workspace.Host.Current = new LayoutRoot().RootPanel;
        Assert.False(workspace.Command(hide).CanExecute(null));
        workspace.Command(hide).Execute(null);
        Dispatcher.UIThread.RunJobs();
    }

    private static void ThrowFromApplication(Exception failure) => throw failure;

    private sealed class ItemProbe : LayoutDocumentItem
    {
        internal Action? Closing { get; set; }
        internal int FloatingCalls { get; private set; }
        internal void Initialize()
        {
            base.InitDefaultCommands();
            Assert.Throws<NullReferenceException>(() => base.SetDefaultBindings());
        }
        internal void InvokeBaseFloat() => base.Float();
        protected override void Float() => FloatingCalls++;
        protected override void Close() => Closing?.Invoke();
    }

    private sealed class Command(Func<bool> canExecute, Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => canExecute();
        public void Execute(object? parameter) => execute(parameter);
    }

    private sealed class ToolHost() : LayoutAnchorableFloatingWindowControl(new() { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) })
    {
        internal ILayoutElement? Current { get; set; }
        public override ILayoutElement Model => Current!;
    }

    private sealed class Workspace : IDisposable
    {
        internal LayoutAnchorable First { get; } = new();
        internal LayoutAnchorable Second { get; } = new();
        internal DockingManager Manager { get; }
        internal ToolHost Host { get; }

        internal Workspace()
        {
            Manager = new() { Layout = new() { RootPanel = new(new LayoutAnchorablePane { Children = { First, Second } }) } };
            Host = new() { Current = Manager.Layout.RootPanel };
        }
        internal ICommand Command(bool hide) => hide ? Host.HideWindowCommand : Host.CloseWindowCommand;
        internal void Set(LayoutAnchorable tool, bool hide, ICommand? command)
        {
            LayoutAnchorableItem item = (LayoutAnchorableItem)Manager.GetLayoutItemFromModel(tool)!;
            if (hide) item.HideCommand = command;
            else item.CloseCommand = command;
        }
        public void Dispose()
        {
            Host.Current = null;
            ((IHostWindow)Host).Exit();
            Manager.Layout = new();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
