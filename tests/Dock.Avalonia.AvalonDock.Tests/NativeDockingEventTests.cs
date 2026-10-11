using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvalonDock;
using AvalonDock.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class NativeCommandTests
{
    [AvaloniaFact]
    public void Pointer_Menu_Actions_Raise_Public_Docking_Events_Through_The_Window()
    {
        using Workspace workspace = new();
        List<string> events = new();
        void Record(object? sender, RoutedEventArgs e)
        {
            Assert.Same(workspace.Window, sender);
            Assert.Same(workspace.Document, e.Source);
            events.Add(e.RoutedEvent!.Name);
        }
        workspace.Window.AddHandler(DockingManager.PreviewFloatEvent, Record);
        workspace.Window.AddHandler(DockingManager.FloatedEvent, Record);
        workspace.Window.AddHandler(DockingManager.PreviewDockEvent, Record);
        workspace.Window.AddHandler(DockingManager.DockedEvent, Record);
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        Click(Tab(workspace.Manager, workspace.Document), MouseButton.Right);
        Click(workspace.Manager.DocumentContextMenu!.Items.OfType<MenuItem>()
            .Single(entry => ReferenceEquals(entry.Command, item.FloatCommand)));
        Assert.True(workspace.Document.IsFloating);
        Click(Tab(Scope(workspace, workspace.Document), workspace.Document), MouseButton.Right);
        Click(workspace.Manager.DocumentContextMenu.Items.OfType<MenuItem>()
            .Single(entry => ReferenceEquals(entry.Command, item.DockAsDocumentCommand)));
        Assert.False(workspace.Document.IsFloating);
        Assert.Equal(new[] { "PreviewFloat", "Floated", "PreviewDock", "Docked" }, events);
    }
}
