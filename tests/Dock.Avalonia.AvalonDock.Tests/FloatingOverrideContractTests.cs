using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingOverrideContractTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void IsMaximized_Retains_The_Writable_Property_Contract_Without_Changing_Native_State(bool binding)
    {
        using Workspace workspace = new(tools: false);
        LayoutFloatingWindowControl window = workspace.Probe.Window;
        if (binding)
        {
            Border source = new();
            using IDisposable subscription = window.Bind(LayoutFloatingWindowControl.IsMaximizedProperty, source.GetObservable(Border.IsEnabledProperty));
            Assert.True(window.IsMaximized);
            source.IsEnabled = false;
            Assert.False(window.IsMaximized);
            source.IsEnabled = true;
            Assert.True(window.IsMaximized);
        }
        else
        {
            window.SetValue(LayoutFloatingWindowControl.IsMaximizedProperty, true);
            Assert.True(window.IsMaximized);
            window.ClearValue(LayoutFloatingWindowControl.IsMaximizedProperty);
            Assert.False(window.IsMaximized);
        }
        Assert.Equal(WindowState.Normal, window.WindowState);
        Assert.False(workspace.Original.IsMaximized);
    }

    [AvaloniaTheory]
    [InlineData("default")]
    [InlineData("can-hide-clear")]
    [InlineData("hide-clear")]
    [InlineData("throw")]
    public void Closing_Marks_Cancellation_Before_Hide_Hooks_And_Preserves_Their_Changes(string behavior)
    {
        using ClosingProbe host = new(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() }, behavior);
        CancelEventArgs args = new();

        if (behavior == "throw") Assert.Throws<InvalidOperationException>(() => host.Check(args));
        else host.Check(args);

        Assert.Equal(new[] { "close:False", "can-hide:True", "hide:" + (behavior != "can-hide-clear") }, host.Events);
        Assert.Equal(behavior is "default" or "throw", args.Cancel);
    }

    [AvaloniaTheory]
    [InlineData(false, "root")]
    [InlineData(false, "panel")]
    [InlineData(false, "pane")]
    [InlineData(false, "content")]
    [InlineData(false, "null")]
    [InlineData(false, "detached")]
    [InlineData(false, "unowned")]
    [InlineData(true, "root")]
    [InlineData(true, "panel")]
    [InlineData(true, "pane")]
    [InlineData(true, "content")]
    [InlineData(true, "null")]
    [InlineData(true, "detached")]
    [InlineData(true, "unowned")]
    public void Capability_Hooks_Read_The_Overridden_Model_And_Filter_Descendants_By_Host_Type(bool tools, string target)
    {
        using Workspace workspace = new(tools);
        bool attached = target is "panel" or "pane" or "content";
        workspace.Original.CanClose = !attached;
        if (workspace.Original is LayoutAnchorable originalTool) originalTool.CanHide = !attached;
        workspace.Probe.Redirect = true;
        workspace.Probe.Current = target switch
        {
            "root" => workspace.CurrentManager.Layout,
            "panel" => workspace.CurrentManager.Layout.RootPanel,
            "pane" => tools ? workspace.Tools : workspace.Documents,
            "content" => tools ? workspace.First : workspace.Document,
            "detached" => new LayoutPanel(),
            "unowned" => new LayoutRoot(),
            _ => null
        };
        // Opposite content kinds must not participate in a typed host's policy.
        workspace.Document.CanClose = !tools;
        workspace.First.CanClose = workspace.Second.CanClose = tools;
        object parameter = new();
        LayoutItem item = workspace.Item(tools ? workspace.Second : workspace.Document);
        item.CloseCommand = new Command(value => ReferenceEquals(value, parameter), _ => { });

        Assert.Equal(attached, workspace.Probe.CheckClose(parameter));
        if (tools) Assert.Equal(attached, workspace.Probe.CheckHide(parameter));
        if (!attached || target == "content") return;

        Assert.False(workspace.Probe.CheckClose(new object()));
        item.CloseCommand = null;
        Assert.False(workspace.Probe.CheckClose(parameter));
        item.CloseCommand = new Command(_ => true, _ => { });
        Assert.True(workspace.Probe.CheckClose(parameter));
        (tools ? (LayoutContent)workspace.Second : workspace.Document).CanClose = false;
        Assert.False(workspace.Probe.CheckClose(parameter));
        if (tools)
        {
            workspace.Second.CanHide = false;
            Assert.False(workspace.Probe.CheckHide(parameter));
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Tool_Commands_Snapshot_The_Current_Model_Before_Callbacks_Change_It_And_Resolve_Commands_Live(bool hide)
    {
        using Workspace workspace = new(tools: true);
        ToolProbe host = (ToolProbe)workspace.Probe;
        host.Redirect = true;
        host.Current = workspace.CurrentManager.Layout.RootPanel;
        object parameter = new();
        List<string> calls = new();
        Command replacement = new(_ => false, value =>
        {
            Assert.Same(parameter, value);
            calls.Add("replacement");
        });
        Command first = new(_ => false, value =>
        {
            Assert.Same(parameter, value);
            calls.Add("first");
            host.Current = workspace.OriginalManager.Layout.RootPanel;
            SetCommand(workspace, workspace.Second, hide, replacement);
        });
        SetCommand(workspace, workspace.First, hide, first);
        SetCommand(workspace, workspace.Second, hide, new Command(_ => true, _ => calls.Add("retired")));
        ICommand command = hide ? host.HideWindowCommand : host.CloseWindowCommand;
        Assert.False(command.CanExecute(parameter));

        command.Execute(parameter);

        Assert.Equal(new[] { "first", "replacement" }, calls);
    }

    [AvaloniaFact]
    public void DoHide_Uses_The_Overridden_Model_And_A_Null_Command_Parameter()
    {
        using Workspace workspace = new(tools: true);
        ToolProbe host = (ToolProbe)workspace.Probe;
        host.Redirect = true;
        host.Current = workspace.Tools;
        List<object?> parameters = new();
        Command command = new(_ => false, parameters.Add);
        SetCommand(workspace, workspace.First, true, command);
        SetCommand(workspace, workspace.Second, true, command);

        host.HideHook();

        Assert.Equal(new object?[] { null, null }, parameters);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Command_Queries_Observe_The_Current_Model_And_Release_Retired_Command_Subscriptions(bool hide)
    {
        using Workspace workspace = new(tools: true);
        ToolProbe host = (ToolProbe)workspace.Probe;
        host.Redirect = true;
        host.Current = workspace.Tools;
        Command current = new(_ => true, _ => { }) { Enabled = false };
        SetCommand(workspace, workspace.Second, hide, current);
        ICommand command = hide ? host.HideWindowCommand : host.CloseWindowCommand;
        Assert.False(command.CanExecute(null));
        int changes = 0;
        command.CanExecuteChanged += (_, _) => changes++;

        current.Enabled = true;

        Assert.True(changes > 0);
        Assert.True(command.CanExecute(null));
        host.Current = null;
        Assert.False(command.CanExecute(null));
        changes = 0;
        current.Enabled = false;
        Assert.Equal(0, changes);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Typed_Caption_Activation_And_Document_Item_Use_The_Constructor_Model(bool tools)
    {
        using Workspace workspace = new(tools);
        workspace.Probe.Redirect = true;
        workspace.Probe.Current = workspace.CurrentManager.Layout;
        workspace.OriginalManager.Layout.ActiveContent = workspace.MainDocument;
        Assert.False(workspace.Original.IsActive);

        workspace.Probe.CaptionPress();

        Assert.True(workspace.Original.IsActive);
        Assert.Same(workspace.Original, workspace.OriginalManager.Layout.ActiveContent);
        if (workspace.Probe is DocumentProbe document)
            Assert.Same(workspace.OriginalManager.GetLayoutItemFromModel(workspace.Original), document.RootDocumentLayoutItem);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Internal_Closure_Completes_When_The_Overridden_Model_Is_Null(bool tools)
    {
        using Workspace workspace = new(tools);
        workspace.Probe.Redirect = true;
        workspace.Probe.Current = null;
        int closed = 0;
        workspace.Probe.Window.Closed += (_, _) => closed++;

        ((IHostWindow)workspace.Probe.Window).Exit();

        Assert.Equal(1, closed);
        if (workspace.Probe is ToolProbe tool)
        {
            Assert.False(tool.CloseWindowCommand.CanExecute(null));
            Assert.False(tool.HideWindowCommand.CanExecute(null));
        }
    }

    private static void SetCommand(Workspace workspace, LayoutAnchorable tool, bool hide, ICommand command)
    {
        LayoutAnchorableItem item = (LayoutAnchorableItem)workspace.Item(tool);
        if (hide) item.HideCommand = command;
        else item.CloseCommand = command;
    }

    private sealed class Command(Func<object?, bool> canExecute, Action<object?> execute) : ICommand
    {
        private bool _enabled = true;
        public event EventHandler? CanExecuteChanged;
        public bool Enabled
        {
            get => _enabled;
            set { _enabled = value; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
        }
        public bool CanExecute(object? parameter) => Enabled && canExecute(parameter);
        public void Execute(object? parameter) => execute(parameter);
    }

    private interface IProbe
    {
        LayoutFloatingWindowControl Window { get; }
        bool Redirect { get; set; }
        ILayoutElement? Current { get; set; }
        bool CheckClose(object? parameter);
        bool CheckHide(object? parameter);
        void CaptionPress();
    }

    private sealed class ClosingProbe(ILayoutElement model, string behavior) : LayoutFloatingWindowControl(model), IDisposable
    {
        private CancelEventArgs? _arguments;
        public override ILayoutElement Model { get; } = model;
        public List<string> Events { get; } = new();
        public void Check(CancelEventArgs args)
        {
            _arguments = args;
            try { base.OnClosing(args); }
            finally { _arguments = null; }
        }
        protected override bool CanClose(object? parameter = null)
        {
            Events.Add("close:" + _arguments!.Cancel);
            return false;
        }
        protected override bool CanHide(object? parameter = null)
        {
            Events.Add("can-hide:" + _arguments!.Cancel);
            if (behavior == "can-hide-clear") _arguments.Cancel = false;
            return true;
        }
        protected override void DoHide()
        {
            Events.Add("hide:" + _arguments!.Cancel);
            if (behavior == "hide-clear") _arguments.Cancel = false;
            if (behavior == "throw") throw new InvalidOperationException("Hide callback failure");
        }
        public void Dispose() => ((IHostWindow)this).Exit();
    }

    private sealed class DocumentProbe(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model), IProbe
    {
        LayoutFloatingWindowControl IProbe.Window => this;
        public bool Redirect { get; set; }
        public ILayoutElement? Current { get; set; }
        public override ILayoutElement Model => Redirect ? Current! : base.Model;
        public bool CheckClose(object? parameter) => base.CanClose(parameter);
        public bool CheckHide(object? parameter) => base.CanHide(parameter);
        public void CaptionPress() { bool handled = false; FilterMessage(IntPtr.Zero, 0xA1, (IntPtr)2, IntPtr.Zero, ref handled); }
    }

    private sealed class ToolProbe(LayoutAnchorableFloatingWindow model) : LayoutAnchorableFloatingWindowControl(model), IProbe
    {
        LayoutFloatingWindowControl IProbe.Window => this;
        public bool Redirect { get; set; }
        public ILayoutElement? Current { get; set; }
        public override ILayoutElement Model => Redirect ? Current! : base.Model;
        public bool CheckClose(object? parameter) => base.CanClose(parameter);
        public bool CheckHide(object? parameter) => base.CanHide(parameter);
        public void HideHook() => base.DoHide();
        public void CaptionPress() { bool handled = false; FilterMessage(IntPtr.Zero, 0xA1, (IntPtr)2, IntPtr.Zero, ref handled); }
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager OriginalManager { get; } = new();
        internal DockingManager CurrentManager { get; } = new();
        internal LayoutDocument MainDocument { get; } = new() { Content = "Main" };
        internal LayoutContent Original { get; }
        internal LayoutDocument Document { get; } = new() { Content = "Current document" };
        internal LayoutAnchorable First { get; } = new() { Content = "Current first tool", CanClose = true, CanHide = true };
        internal LayoutAnchorable Second { get; } = new() { Content = "Current second tool", CanClose = true, CanHide = true };
        internal LayoutDocumentPane Documents { get; }
        internal LayoutAnchorablePane Tools { get; }
        internal IProbe Probe { get; }

        internal Workspace(bool tools)
        {
            OriginalManager.Layout = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(MainDocument)) };
            Documents = new(Document);
            Tools = new(First);
            Tools.Children.Add(Second);
            LayoutPanel current = new(Documents);
            current.Children.Add(Tools);
            CurrentManager.Layout = new() { RootPanel = current };
            Original = tools ? new LayoutAnchorable { Content = "Original tool", CanClose = false, CanHide = false }
                : new LayoutDocument { Content = "Original document", CanClose = false };
            LayoutFloatingWindow model = tools
                ? new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane((LayoutAnchorable)Original)) }
                : new LayoutDocumentFloatingWindow { RootDocument = (LayoutDocument)Original };
            OriginalManager.Layout.FloatingWindows.Add(model);
            Probe = tools ? new ToolProbe((LayoutAnchorableFloatingWindow)model) : new DocumentProbe((LayoutDocumentFloatingWindow)model);
        }

        internal LayoutItem Item(LayoutContent content) => CurrentManager.GetLayoutItemFromModel(content)!;

        public void Dispose()
        {
            Probe.Redirect = false;
            ((IHostWindow)Probe.Window).Exit();
            OriginalManager.Layout.FloatingWindows.Clear();
            CurrentManager.Layout.FloatingWindows.Clear();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
