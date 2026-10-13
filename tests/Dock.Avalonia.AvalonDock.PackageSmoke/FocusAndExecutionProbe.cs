using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class FocusAndExecutionProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("direct disabled action", () =>
        {
            ItemProbe item = new();
            item.Initialize();
            int calls = 0;
            item.Closing = () => calls++;
            if (item.CloseCommand!.CanExecute(null)) throw new Exception("An unattached command was available.");
            item.CloseCommand.Execute(new object());
            if (calls != 1) throw new Exception("Direct execution queried availability.");
        });
        Check("synchronous original error", () =>
        {
            ItemProbe item = new();
            item.Initialize();
            InvalidOperationException error = new("application failure");
            item.Closing = () => throw error;
            Exception? actual = Capture(() => item.CloseCommand!.Execute(null));
            if (!ReferenceEquals(error, actual)) throw new Exception("The command did not return its original error to the caller.");
            Dispatcher.UIThread.RunJobs();
        });
        Check("unused protected Float hook", () =>
        {
            ItemProbe item = new();
            item.Initialize();
            if (Capture(() => item.FloatCommand!.Execute(null)) is not NullReferenceException || item.FloatCalls != 0)
                throw new Exception("The default float action did not retain its missing-context failure independently of the protected hook.");
        });
        Check("owned close focus", () => CheckCloseFocus(true));
        Check("manager close focus", () => CheckCloseFocus(false));
        Check("first focusable hide target", () =>
        {
            using Workspace workspace = new(tools: true, owned: false);
            workspace.First.IsEnabled = false;
            ((ToolHost)workspace.Host).HideWindowCommand.Execute(null);
            if (!ReferenceEquals(workspace.Focused, workspace.Outside)) throw new Exception("Hide skipped the first focusable descendant.");
        });
        Check("missing child command", () =>
        {
            using Workspace workspace = new(tools: true, owned: false);
            workspace.FirstTool.HideCommand = null;
            int later = 0;
            workspace.SecondTool.HideCommand = new Command(() => later++);
            if (Capture(() => ((ToolHost)workspace.Host).HideWindowCommand.Execute(null)) is not NullReferenceException || later != 0)
                throw new Exception("A missing current command did not stop execution.");
        });
        Check("direct unavailable split", () =>
        {
            LayoutDocument document = new() { CanMove = false };
            LayoutDocumentPane pane = new(document);
            pane.Children.Add(new LayoutDocument());
            DockingManager manager = CreateManager(pane);
            manager.GetLayoutItemFromModel(document)!.NewVerticalTabGroupCommand!.Execute(null);
            if (ReferenceEquals(document.Parent, pane) || !ReferenceEquals(manager.Layout.ActiveContent, document))
                throw new Exception("Direct splitting remained blocked or lost activation.");
            manager.Layout = new();
        });
        Check("populated split insertion", () =>
        {
            LayoutDocument document = new();
            LayoutDocumentPane pane = new(document);
            pane.Children.Add(new LayoutDocument());
            LayoutDocumentPaneGroup group = new(pane);
            DockingManager manager = CreateManager(group);
            bool observed = false;
            bool populated = false;
            group.Children.CollectionChanged += (_, e) =>
            {
                if (e.Action != NotifyCollectionChangedAction.Add) return;
                observed = true;
                populated = e.NewItems?[0] is LayoutDocumentPane { ChildrenCount: 1 } destination &&
                    ReferenceEquals(destination.Children[0], document) && !pane.Children.Contains(document);
            };
            manager.GetLayoutItemFromModel(document)!.NewVerticalTabGroupCommand!.Execute(null);
            if (!observed || !populated) throw new Exception("The group published an empty destination before transferring content.");
            manager.Layout = new();
        });
        Check("move availability independent of CanMove", () =>
        {
            LayoutDocument document = new() { CanMove = false };
            LayoutDocumentPane source = new(document);
            LayoutDocumentPane destination = new(new LayoutDocument());
            LayoutDocumentPaneGroup group = new(source);
            group.Children.Add(destination);
            DockingManager manager = CreateManager(group);
            ICommand command = manager.GetLayoutItemFromModel(document)!.MoveToNextTabGroupCommand!;
            if (!command.CanExecute(null)) throw new Exception("An adjacent pane move was blocked by CanMove.");
            command.Execute(null);
            if (!ReferenceEquals(document.Parent, destination)) throw new Exception("The move did not transfer native ownership.");
            manager.Layout = new();
        });
        Check("close-all application availability", () =>
        {
            LayoutDocument selected = new() { CanClose = false };
            LayoutDocument document = new();
            LayoutDocumentPane pane = new(selected);
            pane.Children.Add(document);
            DockingManager manager = CreateManager(pane);
            ((IDock)pane.DockModel).DockCapabilityPolicy = new DockCapabilityPolicy { CanClose = false };
            int calls = 0;
            manager.GetLayoutItemFromModel(document)!.CloseCommand = new Command(() => calls++);
            manager.GetLayoutItemFromModel(selected)!.CloseAllCommand!.Execute(null);
            if (calls != 1) throw new Exception("Close-all bypassed the current application's availability decision.");
            manager.Layout = new();
        });
        Check("unattached activation failure", () =>
        {
            ItemProbe item = new();
            item.Initialize();
            if (Capture(() => item.ActivateCommand!.Execute(null)) is not NullReferenceException)
                throw new Exception("Direct activation suppressed its missing-context failure.");
        });
        if (failures.Count != 0) throw new Exception("Packaged focus/execution failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static void CheckCloseFocus(bool owned)
    {
        using Workspace workspace = new(tools: false, owned);
        workspace.Host.Closed += (_, _) => workspace.Outside.Focus();
        workspace.Host.Close();
        if (!ReferenceEquals(workspace.Focused, owned ? (InputElement)workspace.View : workspace.First))
            throw new Exception("Closure did not focus the owner or manager after Closed.");
    }

    private static Exception? Capture(Action action)
    {
        try { action(); return null; }
        catch (Exception error) { return error; }
    }

    private static DockingManager CreateManager(ILayoutPanelElement child) => new() { Layout = new() { RootPanel = new(child) } };

    private sealed class ItemProbe : LayoutDocumentItem
    {
        internal Action? Closing { get; set; }
        internal int FloatCalls { get; private set; }
        internal void Initialize()
        {
            InitDefaultCommands();
            if (Capture(SetDefaultBindings) is not NullReferenceException) throw new Exception("Unattached binding setup unexpectedly succeeded.");
        }
        protected override void Close() => Closing?.Invoke();
        protected override void Float() => FloatCalls++;
    }

    private sealed class Command(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
    }

    private sealed class DocumentHost(ILayoutElement current) : LayoutDocumentFloatingWindowControl(new() { RootDocument = new() })
    {
        public override ILayoutElement Model => current;
        protected override bool CanClose(object? parameter = null) => true;
    }

    private sealed class ToolHost(ILayoutElement current) : LayoutAnchorableFloatingWindowControl(new() { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) })
    {
        public override ILayoutElement Model => current;
        protected override bool CanClose(object? parameter = null) => true;
    }

    private sealed class Workspace : IDisposable
    {
        internal TextBox First { get; } = new();
        internal TextBox Outside { get; } = new();
        internal LayoutAnchorableItem FirstTool { get; }
        internal LayoutAnchorableItem SecondTool { get; }
        internal Window View { get; }
        internal LayoutFloatingWindowControl Host { get; }
        internal IInputElement? Focused => View.FocusManager!.GetFocusedElement();

        internal Workspace(bool tools, bool owned)
        {
            LayoutAnchorable first = new();
            LayoutAnchorable second = new();
            LayoutAnchorablePane pane = new(first);
            pane.Children.Add(second);
            DockingManager manager = CreateManager(pane);
            manager.Template = new FuncControlTemplate<DockingManager>((_, _) => new StackPanel { Children = { new Border { Child = First }, new TextBox() } });
            FirstTool = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(first)!;
            SecondTool = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(second)!;
            FirstTool.HideCommand = SecondTool.HideCommand = new Command(() => { });
            View = new() { Content = new StackPanel { Children = { manager, Outside } }, Width = 600, Height = 400, Focusable = true };
            View.Show();
            View.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Host = tools ? new ToolHost(manager.Layout.RootPanel) : new DocumentHost(manager.Layout.RootPanel);
            if (owned) Host.Show(View);
            else Host.Show();
            Dispatcher.UIThread.RunJobs();
            if (!Outside.Focus()) throw new Exception("The focus fixture did not initialize.");
        }

        public void Dispose()
        {
            Host.Close();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
