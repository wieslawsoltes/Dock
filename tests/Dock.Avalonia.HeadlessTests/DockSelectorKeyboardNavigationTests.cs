using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Selectors;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockSelectorKeyboardNavigationTests
{
    [AvaloniaFact]
    public void DocumentSelector_Gesture_And_Enter_Commit_Selects_Next_Document()
    {
        var context = CreateLayoutContext();
        var dockControl = CreateDockControl(context);
        var window = new Window { Width = 960, Height = 600, Content = dockControl };
        window.Show();
        dockControl.ApplyTemplate();
        window.UpdateLayout();
        dockControl.UpdateLayout();

        try
        {
            var overlay = GetSelectorOverlay(dockControl);
            Assert.False(overlay.IsOpen);

            var gesture = DockSettings.DocumentSelectorKeyGesture;
            RaiseKeyDown(dockControl, gesture.Key, gesture.KeyModifiers);

            Assert.True(overlay.IsOpen);
            Assert.Equal(DockSelectorMode.Documents, overlay.Mode);
            Assert.Equal("Document B", overlay.SelectedItem?.Title);

            RaiseKeyDown(dockControl, Key.Enter, KeyModifiers.None);
            Assert.False(overlay.IsOpen);
            Assert.Same(context.DocumentB, context.DocumentDock.ActiveDockable);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Selector_Escape_Cancels_Selection_Without_Changing_Active_Dockable()
    {
        var context = CreateLayoutContext();
        var dockControl = CreateDockControl(context);
        var window = new Window { Width = 960, Height = 600, Content = dockControl };
        window.Show();
        dockControl.ApplyTemplate();
        window.UpdateLayout();
        dockControl.UpdateLayout();

        try
        {
            var overlay = GetSelectorOverlay(dockControl);
            var gesture = DockSettings.DocumentSelectorKeyGesture;

            RaiseKeyDown(dockControl, gesture.Key, gesture.KeyModifiers);
            Assert.True(overlay.IsOpen);
            Assert.Equal("Document B", overlay.SelectedItem?.Title);

            RaiseKeyDown(dockControl, Key.Escape, KeyModifiers.None);
            Assert.False(overlay.IsOpen);
            Assert.Same(context.DocumentA, context.DocumentDock.ActiveDockable);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Selector_Tab_ShiftTab_And_Arrow_Navigation_Updates_Selected_Item()
    {
        var context = CreateLayoutContext();
        var dockControl = CreateDockControl(context);
        var window = new Window { Width = 960, Height = 600, Content = dockControl };
        window.Show();
        dockControl.ApplyTemplate();
        window.UpdateLayout();
        dockControl.UpdateLayout();

        try
        {
            var overlay = GetSelectorOverlay(dockControl);
            var gesture = DockSettings.DocumentSelectorKeyGesture;

            RaiseKeyDown(dockControl, gesture.Key, gesture.KeyModifiers);
            Assert.True(overlay.IsOpen);
            Assert.Equal("Document B", overlay.SelectedItem?.Title);

            RaiseKeyDown(dockControl, Key.Tab, KeyModifiers.Shift);
            Assert.Equal("Document A", overlay.SelectedItem?.Title);

            RaiseKeyDown(dockControl, Key.Right, KeyModifiers.None);
            Assert.Equal("Document B", overlay.SelectedItem?.Title);

            RaiseKeyDown(dockControl, Key.Left, KeyModifiers.None);
            Assert.Equal("Document A", overlay.SelectedItem?.Title);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Selector_Modifier_Release_Commits_Current_Selection()
    {
        var context = CreateLayoutContext();
        var dockControl = CreateDockControl(context);
        var window = new Window { Width = 960, Height = 600, Content = dockControl };
        window.Show();
        dockControl.ApplyTemplate();
        window.UpdateLayout();
        dockControl.UpdateLayout();

        try
        {
            var overlay = GetSelectorOverlay(dockControl);
            var gesture = DockSettings.DocumentSelectorKeyGesture;

            RaiseKeyDown(dockControl, gesture.Key, gesture.KeyModifiers);
            Assert.True(overlay.IsOpen);
            Assert.Equal("Document B", overlay.SelectedItem?.Title);

            RaiseKeyUp(dockControl, Key.LeftCtrl, KeyModifiers.None);
            Assert.False(overlay.IsOpen);
            Assert.Same(context.DocumentB, context.DocumentDock.ActiveDockable);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolSelector_Gesture_Opens_Tool_Mode_And_Commits_With_Enter()
    {
        var context = CreateLayoutContext();
        context.Root.ActiveDockable = context.ToolDock;
        context.Root.FocusedDockable = context.ToolA;
        context.ToolDock.ActiveDockable = context.ToolA;

        var dockControl = CreateDockControl(context);
        var window = new Window { Width = 960, Height = 600, Content = dockControl };
        window.Show();
        dockControl.ApplyTemplate();
        window.UpdateLayout();
        dockControl.UpdateLayout();

        try
        {
            var overlay = GetSelectorOverlay(dockControl);
            var gesture = DockSettings.ToolSelectorKeyGesture;

            RaiseKeyDown(dockControl, gesture.Key, gesture.KeyModifiers);
            Assert.True(overlay.IsOpen);
            Assert.Equal(DockSelectorMode.Tools, overlay.Mode);
            Assert.Equal("Tool B", overlay.SelectedItem?.Title);

            RaiseKeyDown(dockControl, Key.Enter, KeyModifiers.None);
            Assert.False(overlay.IsOpen);
            Assert.Same(context.ToolB, context.ToolDock.ActiveDockable);
        }
        finally
        {
            window.Close();
        }
    }

    private static DockControl CreateDockControl(LayoutContext context)
    {
        return new DockControl
        {
            Factory = context.Factory,
            Layout = context.Root
        };
    }

    private static DockSelectorOverlay GetSelectorOverlay(DockControl dockControl)
    {
        var overlay = dockControl.GetVisualDescendants().OfType<DockSelectorOverlay>().FirstOrDefault();
        Assert.NotNull(overlay);
        return overlay!;
    }

    private static void RaiseKeyDown(Control control, Key key, KeyModifiers modifiers)
    {
        control.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Route = RoutingStrategies.Tunnel,
            Source = control,
            Key = key,
            KeyModifiers = modifiers
        });
    }

    private static void RaiseKeyUp(Control control, Key key, KeyModifiers modifiers)
    {
        control.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyUpEvent,
            Route = RoutingStrategies.Tunnel,
            Source = control,
            Key = key,
            KeyModifiers = modifiers
        });
    }

    private static LayoutContext CreateLayoutContext()
    {
        var factory = new Factory();

        var root = factory.CreateRootDock();
        root.Id = "Root";
        root.Factory = factory;

        var documentDock = factory.CreateDocumentDock();
        documentDock.Id = "Documents";
        documentDock.Factory = factory;
        documentDock.Owner = root;

        var toolDock = factory.CreateToolDock();
        toolDock.Id = "Tools";
        toolDock.Factory = factory;
        toolDock.Owner = root;

        var documentA = new Document { Id = "DocA", Title = "Document A", Factory = factory, Owner = documentDock };
        var documentB = new Document { Id = "DocB", Title = "Document B", Factory = factory, Owner = documentDock };
        var toolA = new Tool { Id = "ToolA", Title = "Tool A", Factory = factory, Owner = toolDock };
        var toolB = new Tool { Id = "ToolB", Title = "Tool B", Factory = factory, Owner = toolDock };

        documentDock.VisibleDockables = factory.CreateList<IDockable>(documentA, documentB);
        documentDock.ActiveDockable = documentA;
        documentDock.FocusedDockable = documentA;

        toolDock.VisibleDockables = factory.CreateList<IDockable>(toolA, toolB);
        toolDock.ActiveDockable = toolA;
        toolDock.FocusedDockable = toolA;

        root.VisibleDockables = factory.CreateList<IDockable>(toolDock, documentDock);
        root.ActiveDockable = documentDock;
        root.FocusedDockable = documentA;

        return new LayoutContext(factory, root, documentDock, toolDock, documentA, documentB, toolA, toolB);
    }

    private sealed record LayoutContext(
        Factory Factory,
        IRootDock Root,
        IDocumentDock DocumentDock,
        IToolDock ToolDock,
        Document DocumentA,
        Document DocumentB,
        Tool ToolA,
        Tool ToolB);
}
