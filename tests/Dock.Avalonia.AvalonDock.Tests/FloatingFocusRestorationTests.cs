using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingFocusRestorationTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Successful_Closure_Restores_Focus_After_Closed(bool tools, bool owned)
    {
        using Workspace workspace = new(tools, owned);
        bool observed = false;
        workspace.Host.Window.Closed += (_, _) =>
        {
            observed = true;
            Assert.True(workspace.Outside.Focus());
            Assert.Same(workspace.Outside, workspace.Focused);
        };
        workspace.Host.Window.Close();
        Assert.True(observed);
        Assert.Same(owned ? (InputElement)workspace.View : workspace.First, workspace.Focused);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Unparented_Root_Model_Does_Not_Provide_A_Manager_Focus_Context(bool tools)
    {
        using Workspace workspace = new(tools, owned: false);
        workspace.Host.Window.Closed += (_, _) =>
        {
            workspace.Host.Current = workspace.Manager.Layout;
            Assert.True(workspace.Outside.Focus());
        };

        workspace.Host.Window.Close();

        Assert.Same(workspace.Outside, workspace.Focused);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Owner_Focus_Does_Not_Read_A_Model_Redirected_By_Closed(bool tools)
    {
        using Workspace workspace = new(tools, owned: true);
        workspace.Host.Window.Closed += (_, _) => workspace.Host.ThrowModel = true;
        workspace.Host.Window.Close();
        Assert.Same(workspace.View, workspace.Focused);
    }

    [AvaloniaTheory]
    [InlineData("document-close")]
    [InlineData("tool-close")]
    [InlineData("hide-command")]
    [InlineData("hide-hook")]
    public void Focus_Uses_The_Current_Model_After_Operation_Callbacks(string operation)
    {
        using Workspace workspace = new(operation != "document-close", owned: false);
        if (operation.EndsWith("close", StringComparison.Ordinal))
            workspace.Host.Window.Closed += (_, _) => workspace.Host.Current = workspace.OtherManager.Layout.RootPanel;
        else
            workspace.ToolItem.HideCommand = new Command(() => workspace.Host.Current = workspace.OtherManager.Layout.RootPanel);
        workspace.Execute(operation);
        Assert.Same(workspace.OtherFirst, workspace.Focused);
    }

    [AvaloniaTheory]
    [InlineData("document-close", "disabled")]
    [InlineData("document-close", "hidden")]
    [InlineData("tool-close", "disabled")]
    [InlineData("tool-close", "hidden")]
    [InlineData("hide-command", "disabled")]
    [InlineData("hide-command", "hidden")]
    [InlineData("hide-hook", "disabled")]
    [InlineData("hide-hook", "hidden")]
    public void An_Unavailable_First_Focusable_Descendant_Does_Not_Select_A_Later_One(string operation, string state)
    {
        using Workspace workspace = new(operation != "document-close", owned: false);
        if (state == "disabled") workspace.First.IsEnabled = false;
        else workspace.First.IsVisible = false;
        if (operation.EndsWith("close", StringComparison.Ordinal))
            workspace.Host.Window.Closed += (_, _) => Assert.True(workspace.Outside.Focus());
        workspace.Execute(operation);
        Assert.Same(workspace.Outside, workspace.Focused);
    }

    [AvaloniaTheory]
    [InlineData("document-close")]
    [InlineData("hide-command")]
    public void A_Focus_Callback_Can_Redirect_Focus_Without_Trying_A_Second_Target(string operation)
    {
        using Workspace workspace = new(operation != "document-close", owned: false);
        int first = 0;
        int second = 0;
        workspace.First.GotFocus += (_, _) => { first++; workspace.Outside.Focus(); };
        workspace.Second.GotFocus += (_, _) => second++;
        workspace.Execute(operation);
        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Same(workspace.Outside, workspace.Focused);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Unfocusable_Owner_Does_Not_Fall_Back_To_The_Manager(bool tools)
    {
        using Workspace workspace = new(tools, owned: true);
        workspace.View.Focusable = false;
        workspace.Host.Window.Closed += (_, _) => Assert.True(workspace.Outside.Focus());
        workspace.Host.Window.Close();
        Assert.Same(workspace.Outside, workspace.Focused);
    }

    [AvaloniaFact]
    public void The_Manager_Itself_Is_Not_A_Fallback_Focus_Target()
    {
        using Workspace workspace = new(tools: false, owned: false);
        workspace.Manager.Focusable = true;
        workspace.First.Focusable = workspace.Second.Focusable = false;
        workspace.Host.Window.Closed += (_, _) => Assert.True(workspace.Outside.Focus());
        workspace.Host.Window.Close();
        Assert.Same(workspace.Outside, workspace.Focused);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Failing_Closed_Callback_Does_Not_Restore_Focus(bool tools)
    {
        using Workspace workspace = new(tools, owned: false);
        InvalidOperationException failure = new("close callback");
        bool fail = true;
        workspace.Host.Window.Closed += (_, _) => { if (fail) throw failure; };
        try
        {
            Assert.Same(failure, Assert.Throws<InvalidOperationException>(workspace.Host.InvokeClosed));
            Assert.Same(workspace.Outside, workspace.Focused);
        }
        finally { fail = false; }
    }

    [AvaloniaTheory]
    [InlineData("hide-command")]
    [InlineData("hide-hook")]
    public void A_Failing_Hide_Command_Does_Not_Restore_Focus(string operation)
    {
        using Workspace workspace = new(tools: true, owned: false);
        InvalidOperationException failure = new("hide callback");
        workspace.ToolItem.HideCommand = new Command(() => throw failure);
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => workspace.Execute(operation)));
        Assert.Same(workspace.Outside, workspace.Focused);
        Dispatcher.UIThread.RunJobs();
    }

    private interface IHost
    {
        LayoutFloatingWindowControl Window { get; }
        ILayoutElement? Current { get; set; }
        bool ThrowModel { get; set; }
        void InvokeClosed();
    }

    private sealed class DocumentHost() : LayoutDocumentFloatingWindowControl(new() { RootDocument = new() }), IHost
    {
        LayoutFloatingWindowControl IHost.Window => this;
        public ILayoutElement? Current { get; set; }
        public bool ThrowModel { get; set; }
        public override ILayoutElement Model => ThrowModel ? throw new InvalidOperationException("unexpected model read") : Current ?? base.Model;
        public void InvokeClosed() => base.OnClosed(EventArgs.Empty);
        protected override bool CanClose(object? parameter = null) => true;
    }

    private sealed class ToolHost() : LayoutAnchorableFloatingWindowControl(new() { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) }), IHost
    {
        LayoutFloatingWindowControl IHost.Window => this;
        public ILayoutElement? Current { get; set; }
        public bool ThrowModel { get; set; }
        public override ILayoutElement Model => ThrowModel ? throw new InvalidOperationException("unexpected model read") : Current ?? base.Model;
        public void InvokeClosed() => base.OnClosed(EventArgs.Empty);
        public void HideContents() => base.DoHide();
        protected override bool CanClose(object? parameter = null) => true;
    }

    private sealed class Command(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
    }

    private sealed class Workspace : IDisposable
    {
        internal TextBox First { get; } = new();
        internal TextBox Second { get; } = new();
        internal TextBox OtherFirst { get; } = new();
        internal TextBox Outside { get; } = new();
        internal DockingManager Manager { get; }
        internal DockingManager OtherManager { get; }
        internal LayoutAnchorableItem ToolItem { get; }
        internal Window View { get; }
        internal IHost Host { get; }
        internal IInputElement? Focused => View.FocusManager!.GetFocusedElement();

        internal Workspace(bool tools, bool owned)
        {
            LayoutAnchorable tool = new();
            LayoutPanel layout = new(new LayoutDocumentPane(new LayoutDocument()));
            layout.Children.Add(new LayoutAnchorablePane(tool));
            Manager = new() { Layout = new() { RootPanel = layout } };
            Manager.Template = new FuncControlTemplate<DockingManager>((_, _) =>
                new StackPanel { Children = { new Border { Child = First }, Second } });
            OtherManager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(new LayoutDocument())) } };
            OtherManager.Template = new FuncControlTemplate<DockingManager>((_, _) => new Border { Child = OtherFirst });
            ToolItem = (LayoutAnchorableItem)Manager.GetLayoutItemFromModel(tool)!;
            ToolItem.HideCommand = new Command(() => { });
            View = new() { Content = new StackPanel { Children = { Manager, OtherManager, Outside } }, Width = 600, Height = 450, Focusable = true };
            View.Show();
            View.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Host = tools ? new ToolHost() : new DocumentHost();
            Host.Current = Manager.Layout.RootPanel;
            if (owned) Host.Window.Show(View);
            else Host.Window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.True(Outside.Focus());
        }

        internal void Execute(string operation)
        {
            if (operation.EndsWith("close", StringComparison.Ordinal)) Host.Window.Close();
            else if (operation == "hide-command") ((ToolHost)Host).HideWindowCommand.Execute(null);
            else ((ToolHost)Host).HideContents();
        }

        public void Dispose()
        {
            Host.ThrowModel = false;
            Host.Window.Close();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
