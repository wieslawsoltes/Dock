using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryExplicitOperationTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Close_Core_Preserves_Cancellation_And_Hide_Settings(bool tools, bool hide)
    {
        ExplicitFactory factory = new() { HideToolsOnClose = hide, HideDocumentsOnClose = hide };
        RootDock root = CreateRoot(factory);
        IDock pane = tools ? new ToolDock() : new DocumentDock();
        pane.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, pane);
        IDockable content = tools ? new Tool() : new Document();
        content.CanClose = false;
        pane.CanCloseLastDockable = false;
        pane.DockCapabilityPolicy = new DockCapabilityPolicy { CanClose = false };
        factory.AddDockable(pane, content);
        bool cancel = true;
        List<string> events = new();
        factory.DockableClosing += (_, args) =>
        {
            Assert.Same(content, args.Dockable);
            Assert.False(content.CanClose);
            events.Add("closing");
            args.Cancel = cancel;
        };
        factory.DockableClosed += (_, _) => events.Add("closed");

        factory.CloseDockable(content);
        Assert.Empty(events);
        factory.CloseExplicitly(content);
        Assert.Contains(content, pane.VisibleDockables!);
        Assert.Equal(new[] { "closing" }, events);
        cancel = false;
        factory.CloseExplicitly(content);

        Assert.DoesNotContain(content, pane.VisibleDockables!);
        Assert.Equal(hide, root.HiddenDockables!.Contains(content));
        Assert.Equal(new[] { "closing", "closing", "closed" }, events);
        Assert.False(content.CanClose);
        Assert.False(pane.CanCloseLastDockable);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Close_Core_Removes_Generated_Content_From_Its_Source_Even_With_Hide_On_Close(bool tools)
    {
        ExplicitFactory factory = new() { HideToolsOnClose = true, HideDocumentsOnClose = true };
        RootDock root = CreateRoot(factory);
        ObservableCollection<string> source = new() { "first", "second" };
        IDock pane;
        if (tools)
        {
            ToolDock toolDock = new() { ToolTemplate = new ToolTemplate() };
            pane = toolDock;
            factory.AddDockable(root, pane);
            toolDock.ItemsSource = source;
        }
        else
        {
            DocumentDock documentDock = new() { DocumentTemplate = new DocumentTemplate() };
            pane = documentDock;
            factory.AddDockable(root, pane);
            documentDock.ItemsSource = source;
        }
        IDockable generated = pane.VisibleDockables![0];
        generated.CanClose = false;

        factory.CloseExplicitly(generated);

        Assert.Equal("second", Assert.Single(source));
        Assert.Equal("second", Assert.Single(pane.VisibleDockables!).Context);
        Assert.Empty(root.HiddenDockables!);
    }

    [AvaloniaTheory]
    [InlineData(Alignment.Left)]
    [InlineData(Alignment.Top)]
    [InlineData(Alignment.Right)]
    [InlineData(Alignment.Bottom)]
    public void Pin_Core_Preserves_Native_Lifecycle_And_Does_Not_Change_Public_Capability_Guards(Alignment alignment)
    {
        ExplicitFactory factory = new();
        RootDock root = CreateRoot(factory);
        ToolDock pane = new() { Alignment = alignment, VisibleDockables = factory.CreateList<IDockable>() };
        Tool tool = new() { CanPin = false };
        factory.AddDockable(root, pane);
        factory.AddDockable(pane, tool);
        pane.DockCapabilityPolicy = new DockCapabilityPolicy { CanPin = false };
        List<string> events = new();
        factory.DockablePinned += (_, _) => events.Add("pin");
        factory.DockableUnpinned += (_, _) => events.Add("unpin");

        factory.PinDockable(tool);
        Assert.Contains(tool, pane.VisibleDockables!);
        Assert.Empty(events);
        factory.TogglePinExplicitly(tool);
        Assert.DoesNotContain(tool, pane.VisibleDockables!);
        Assert.True(factory.IsDockablePinned(tool));
        factory.UnpinDockable(tool);
        Assert.True(factory.IsDockablePinned(tool));
        factory.TogglePinExplicitly(tool);

        Assert.Same(tool, Assert.Single(pane.VisibleDockables!));
        Assert.Same(tool, pane.ActiveDockable);
        Assert.False(factory.IsDockablePinned(tool));
        Assert.False(tool.CanPin);
        Assert.False(pane.DockCapabilityPolicy.CanPin);
        Assert.Equal(new[] { "pin", "unpin" }, events);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Closing_Hidden_Or_Pinned_Content_Removes_Its_Native_List_After_Cancellation(bool pinned, bool explicitClose)
    {
        ExplicitFactory factory = new() { HideToolsOnClose = false };
        RootDock root = CreateRoot(factory);
        ToolDock pane = new() { VisibleDockables = factory.CreateList<IDockable>() };
        Tool tool = new();
        factory.AddDockable(root, pane);
        factory.AddDockable(pane, tool);
        if (pinned) factory.PinDockable(tool);
        else factory.HideDockable(tool);
        tool.CanClose = !explicitClose;
        tool.CanPin = false;
        bool cancel = true;
        int closed = 0;
        factory.DockableClosing += (_, args) => args.Cancel = cancel;
        factory.DockableClosed += (_, _) => closed++;

        Close();
        Assert.Equal(pinned, factory.IsDockablePinned(tool));
        Assert.Equal(!pinned, root.HiddenDockables!.Contains(tool));
        Assert.Equal(0, closed);
        cancel = false;
        Close();

        Assert.False(factory.IsDockablePinned(tool));
        Assert.Empty(root.HiddenDockables!);
        Assert.DoesNotContain(tool, pane.VisibleDockables!);
        Assert.Equal(1, closed);
        Assert.False(tool.CanPin);

        void Close()
        {
            if (explicitClose) factory.CloseExplicitly(tool);
            else factory.CloseDockable(tool);
        }
    }

    private static RootDock CreateRoot(Factory factory) => new()
    {
        Factory = factory,
        VisibleDockables = factory.CreateList<IDockable>(),
        HiddenDockables = factory.CreateList<IDockable>(),
        Windows = factory.CreateList<IDockWindow>()
    };

    private sealed class ExplicitFactory : Factory
    {
        internal void CloseExplicitly(IDockable dockable) => CloseDockableCore(dockable);
        internal void TogglePinExplicitly(IDockable dockable) => PinDockableCore(dockable);
    }
}
