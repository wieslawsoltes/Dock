using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia.Headless.XUnit;
using Orientation = Avalonia.Layout.Orientation;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DefaultCommandActionContractTests
{
    [AvaloniaTheory]
    [InlineData("Close")]
    [InlineData("Float")]
    [InlineData("DockAsDocument")]
    [InlineData("Activate")]
    [InlineData("CloseAll")]
    [InlineData("CloseAllButThis")]
    [InlineData("NewVerticalTabGroup")]
    [InlineData("NewHorizontalTabGroup")]
    [InlineData("MoveToNextTabGroup")]
    [InlineData("MoveToPreviousTabGroup")]
    public void Unattached_Document_Default_Actions_Retain_Their_Context_Failures(string name)
    {
        DocumentProbe item = new();
        item.Initialize();
        ICommand command = name switch
        {
            "Close" => item.CloseCommand!, "Float" => item.FloatCommand!,
            "DockAsDocument" => item.DockAsDocumentCommand!, "Activate" => item.ActivateCommand!,
            "CloseAll" => item.CloseAllCommand!, "CloseAllButThis" => item.CloseAllButThisCommand!,
            "NewVerticalTabGroup" => item.NewVerticalTabGroupCommand!, "NewHorizontalTabGroup" => item.NewHorizontalTabGroupCommand!,
            "MoveToNextTabGroup" => item.MoveToNextTabGroupCommand!, _ => item.MoveToPreviousTabGroupCommand!
        };
        Assert.False(command.CanExecute(null));
        Assert.Throws<NullReferenceException>(() => command.Execute(new object()));
        item.IsVisible = false;
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData("Hide")]
    [InlineData("AutoHide")]
    [InlineData("Dock")]
    [InlineData("Close")]
    public void Unattached_Tool_Actions_Distinguish_Guarded_Hide_Operations_From_Context_Failures(string name)
    {
        ToolProbe item = new();
        item.Initialize();
        if (name == "Close") Assert.Throws<NullReferenceException>(item.InvokeClose);
        else
        {
            ICommand command = name == "Hide" ? item.HideCommand! : name == "AutoHide" ? item.AutoHideCommand! : item.DockCommand!;
            Assert.False(command.CanExecute(null));
            if (name == "Dock") Assert.Throws<NullReferenceException>(() => command.Execute(null));
            else command.Execute(null);
        }
        item.IsVisible = false;
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false, "capability")]
    [InlineData(true, "capability")]
    [InlineData(false, "orientation")]
    [InlineData(true, "orientation")]
    [InlineData(false, "single")]
    [InlineData(true, "single")]
    public void Direct_Split_Executes_When_Its_Availability_Query_Is_False(bool horizontal, string restriction)
    {
        LayoutDocument document = new() { CanMove = restriction != "capability" };
        LayoutDocumentPane pane = new(document);
        if (restriction != "single") pane.Children.Add(new LayoutDocument());
        LayoutDocumentPaneGroup group = new(pane) { Orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical };
        group.Children.Add(new LayoutDocumentPane(new LayoutDocument()));
        DockingManager manager = CreateManager(group);
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        ICommand split = horizontal ? item.NewHorizontalTabGroupCommand! : item.NewVerticalTabGroupCommand!;
        Assert.False(split.CanExecute(null));

        split.Execute(null);

        LayoutDocumentPane destination = Assert.IsType<LayoutDocumentPane>(document.Parent);
        Assert.NotSame(pane, destination);
        Assert.Equal(horizontal ? Orientation.Vertical : Orientation.Horizontal, group.Orientation);
        Assert.Same(document, Assert.Single(destination.Children));
        Assert.Same(destination.DockModel, document.DockModel.Owner);
        Assert.Same(manager.DockFactory, document.DockModel.Factory);
        Assert.Same(document, manager.Layout.ActiveContent);
        Assert.Equal(restriction != "capability", document.CanMove);
        Dispatcher.UIThread.RunJobs();
        Assert.Same(item, manager.GetLayoutItemFromModel(document));
        manager.Layout = new();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Split_Moves_Content_Before_Publishing_A_Nonempty_New_Pane(bool horizontal)
    {
        LayoutDocument document = new();
        LayoutDocumentPane pane = new(document);
        pane.Children.Add(new LayoutDocument());
        LayoutDocumentPaneGroup group = new(pane);
        DockingManager manager = CreateManager(group);
        List<string> events = new();
        pane.Children.CollectionChanged += (_, e) =>
        {
            if (e.Action != NotifyCollectionChangedAction.Remove) return;
            Assert.Same(pane, Assert.Single(group.Children));
            events.Add("remove");
        };
        group.Children.CollectionChanged += (_, e) =>
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            LayoutDocumentPane destination = Assert.IsType<LayoutDocumentPane>(Assert.Single(e.NewItems!));
            Assert.Same(document, Assert.Single(destination.Children));
            Assert.Same(destination.DockModel, document.DockModel.Owner);
            Assert.Same(manager.DockFactory, document.DockModel.Factory);
            events.Add("insert");
        };
        manager.PreviewDock += (_, _) => events.Add("preview");
        manager.Docked += (_, _) => events.Add("docked");
        manager.DockFactory.DockableMoved += (_, _) => events.Add("native:move");
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        (horizontal ? item.NewHorizontalTabGroupCommand! : item.NewVerticalTabGroupCommand!).Execute(null);
        Assert.Equal(new[] { "remove", "insert" }, events);
        manager.Layout = new();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Split_Removal_Error_Prevents_The_Later_Pane_Insertion(bool horizontal)
    {
        LayoutDocument document = new();
        LayoutDocumentPane pane = new(document);
        pane.Children.Add(new LayoutDocument());
        LayoutDocumentPaneGroup group = new(pane);
        DockingManager manager = CreateManager(group);
        InvalidOperationException error = new("source removal");
        pane.Children.CollectionChanged += (_, e) => { if (e.Action == NotifyCollectionChangedAction.Remove) throw error; };
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            (horizontal ? item.NewHorizontalTabGroupCommand! : item.NewVerticalTabGroupCommand!).Execute(null)));
        Assert.Same(pane, Assert.Single(group.Children));
        Assert.DoesNotContain(pane.Children, child => ReferenceEquals(child, document));
        Assert.Null(document.Parent);
        manager.Layout = new();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(-1)]
    [InlineData(1)]
    public void Adjacent_Group_Move_Is_Available_When_Document_CanMove_Is_False(int direction)
    {
        LayoutDocument document = new() { CanMove = false };
        LayoutDocumentPane source = new(document);
        source.Children.Add(new LayoutDocument());
        LayoutDocument existing = new();
        LayoutDocumentPane destination = new(existing);
        LayoutDocumentPaneGroup group = new(direction < 0 ? destination : source);
        group.Children.Add(direction < 0 ? source : destination);
        DockingManager manager = CreateManager(group);
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        ICommand command = direction < 0 ? item.MoveToPreviousTabGroupCommand! : item.MoveToNextTabGroupCommand!;
        Assert.True(command.CanExecute(null));
        command.Execute(null);
        Assert.Equal(new LayoutContent[] { document, existing }, destination.Children);
        Assert.Same(destination.DockModel, document.DockModel.Owner);
        Assert.Same(document, destination.SelectedContent);
        Assert.Same(document, manager.Layout.ActiveContent);
        Assert.False(document.CanMove);
        Dispatcher.UIThread.RunJobs();
        Assert.Same(item, manager.GetLayoutItemFromModel(document));
        manager.Layout = new();
    }

    [AvaloniaTheory]
    [InlineData(-1, "edge")]
    [InlineData(1, "edge")]
    [InlineData(-1, "group")]
    [InlineData(1, "group")]
    [InlineData(-1, "root")]
    [InlineData(1, "root")]
    public void Direct_Adjacent_Move_Retains_Invalid_Destination_Failures(int direction, string context)
    {
        LayoutDocument document = new();
        LayoutDocumentPane source = new(document);
        LayoutDocumentPaneGroup group = new(source);
        if (context == "group") group.Children.Insert(direction < 0 ? 0 : 1, new LayoutDocumentPaneGroup(new LayoutDocumentPane(new LayoutDocument())));
        DockingManager manager = context == "root" ? CreateManager(source) : CreateManager(group);
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        ICommand command = direction < 0 ? item.MoveToPreviousTabGroupCommand! : item.MoveToNextTabGroupCommand!;
        Assert.False(command.CanExecute(null));
        if (context == "edge") Assert.Throws<ArgumentOutOfRangeException>(() => command.Execute(null));
        else Assert.Throws<NullReferenceException>(() => command.Execute(null));
        Assert.Same(source, document.Parent);
        manager.Layout = new();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void CloseAll_Fails_When_A_Preceding_Callback_Removes_A_Later_Container(bool excludeCurrent)
    {
        LayoutDocument selected = new();
        LayoutDocument first = new();
        LayoutDocument later = new();
        LayoutDocumentPane pane = new(selected);
        pane.Children.Add(first);
        pane.Children.Add(later);
        DockingManager manager = CreateManager(pane);
        int calls = 0;
        manager.GetLayoutItemFromModel(selected)!.CloseCommand = new Command(() => { });
        manager.GetLayoutItemFromModel(first)!.CloseCommand = new Command(() => { calls++; manager.Layout = new(); });
        manager.GetLayoutItemFromModel(later)!.CloseCommand = new Command(() => calls++);
        LayoutItem item = manager.GetLayoutItemFromModel(selected)!;
        Assert.Throws<NullReferenceException>(() => (excludeCurrent ? item.CloseAllButThisCommand! : item.CloseAllCommand!).Execute(null));
        Assert.Equal(1, calls);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void CloseAll_Uses_Current_Command_Availability_Or_Model_Fallback_After_The_Content_Capability(bool excludeCurrent, bool custom)
    {
        LayoutDocument selected = new() { CanClose = false };
        LayoutDocument document = new();
        LayoutDocumentPane pane = new(selected);
        pane.Children.Add(document);
        DockingManager manager = CreateManager(pane);
        ((IDock)pane.DockModel).DockCapabilityPolicy = new DockCapabilityPolicy { CanClose = false };
        int calls = 0;
        manager.GetLayoutItemFromModel(selected)!.CloseCommand = new Command(() => throw new InvalidOperationException("protected document"));
        manager.GetLayoutItemFromModel(document)!.CloseCommand = custom ? new Command(() => calls++) : null;
        LayoutItem item = manager.GetLayoutItemFromModel(selected)!;
        (excludeCurrent ? item.CloseAllButThisCommand! : item.CloseAllCommand!).Execute(null);
        Assert.Equal(custom ? 1 : 0, calls);
        Assert.Equal(custom, document.Root is not null);
        Assert.Same(manager.Layout, selected.Root);
        manager.Layout = new();
    }

    private static DockingManager CreateManager(ILayoutPanelElement child) => new() { Layout = new() { RootPanel = new(child) } };

    private sealed class DocumentProbe : LayoutDocumentItem
    {
        internal void Initialize()
        {
            InitDefaultCommands();
            Assert.Throws<NullReferenceException>(SetDefaultBindings);
        }
    }

    private sealed class ToolProbe : LayoutAnchorableItem
    {
        internal void Initialize()
        {
            InitDefaultCommands();
            Assert.Throws<NullReferenceException>(SetDefaultBindings);
        }
        internal void InvokeClose() => base.Close();
    }

    private sealed class Command(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
    }
}
