using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ActiveContentTests
{
    [AvaloniaTheory]
    [InlineData("layout")]
    [InlineData("manager")]
    [InlineData("content")]
    [InlineData("factory")]
    public void Floating_Activation_Uses_Its_Native_Root_And_Updates_Public_Focus(string activation)
    {
        Workspace workspace = new();
        List<object?> changes = new();
        workspace.Manager.ActiveContentChanged += (_, _) => changes.Add(workspace.Manager.ActiveContent);

        Activate(workspace, workspace.Floating, activation);
        AssertActive(workspace, workspace.Floating);
        Assert.Same(workspace.Docked.DockModel, workspace.NativeRoot.FocusedDockable);
        Assert.Same(workspace.Floating.DockModel, workspace.FloatingRoot.FocusedDockable);
        Assert.Equal(new[] { workspace.Floating.Content }, changes);

        Activate(workspace, workspace.Docked, activation);
        AssertActive(workspace, workspace.Docked);
        Assert.Same(workspace.Floating.DockModel, workspace.FloatingRoot.FocusedDockable);
        Activate(workspace, workspace.Floating, activation);
        AssertActive(workspace, workspace.Floating);
        Assert.Equal(new[] { workspace.Floating.Content, workspace.Docked.Content, workspace.Floating.Content }, changes);
    }

    [AvaloniaFact]
    public void Clearing_Floating_Active_Content_Clears_Only_Its_Native_Focus_And_Document_History()
    {
        Workspace workspace = new();
        workspace.Floating.IsActive = true;
        workspace.Root.ActiveContent = null;

        Assert.Null(workspace.Root.ActiveContent);
        Assert.Null(workspace.Manager.ActiveContent);
        Assert.Null(workspace.Root.LastFocusedDocument);
        Assert.False(workspace.Floating.IsLastFocusedDocument);
        Assert.False(workspace.Floating.IsActive);
        Assert.Null(workspace.FloatingRoot.FocusedDockable);
        Assert.Same(workspace.Docked.DockModel, workspace.NativeRoot.FocusedDockable);
    }

    [AvaloniaFact]
    public void Native_Focus_Clearing_Uses_The_Source_Root()
    {
        Workspace workspace = new();
        workspace.Floating.IsActive = true;
        workspace.Root.DockFactory.SetFocusedDockable(workspace.NativeRoot, null);
        AssertActive(workspace, workspace.Floating);

        workspace.Root.DockFactory.SetFocusedDockable(workspace.FloatingRoot, null);
        Assert.Null(workspace.Root.ActiveContent);
        Assert.Null(workspace.Root.LastFocusedDocument);
        Assert.Null(workspace.Manager.ActiveContent);
    }

    [AvaloniaFact]
    public void Native_Window_Reactivation_Updates_Focus_And_Deactivation_Retains_It()
    {
        Workspace workspace = new();
        workspace.Floating.IsActive = true;
        workspace.Root.DockFactory.OnWindowDeactivated(workspace.Window.DockWindow);
        Assert.Same(workspace.Floating, workspace.Root.ActiveContent);
        Assert.Same(workspace.Floating, workspace.Root.LastFocusedDocument);
        workspace.Root.DockFactory.OnWindowActivated(null, workspace.NativeRoot);
        AssertActive(workspace, workspace.Docked);
        workspace.Root.DockFactory.OnWindowActivated(workspace.Window.DockWindow, workspace.FloatingRoot);
        AssertActive(workspace, workspace.Floating);
    }

    [AvaloniaFact]
    public void Tools_In_Document_Panes_Participate_In_Last_Focused_Document_Notifications()
    {
        Workspace workspace = new();
        LayoutAnchorable tool = new();
        LayoutAnchorablePane tools = new(tool);
        workspace.Root.RootPanel.Children.Add(tools);
        List<string?> rootChanges = new();
        List<string?> documentChanges = new();
        workspace.Root.PropertyChanged += (_, args) => rootChanges.Add(args.PropertyName);
        workspace.Docked.PropertyChanged += (_, args) => documentChanges.Add(args.PropertyName);
        tool.IsActive = true;
        Assert.Same(workspace.Docked, workspace.Root.LastFocusedDocument);
        Assert.DoesNotContain(nameof(LayoutRoot.LastFocusedDocument), rootChanges);

        tool.DockAsDocument();
        Assert.Same(tool, workspace.Root.LastFocusedDocument);
        Assert.True(tool.IsLastFocusedDocument);
        Assert.False(workspace.Docked.IsLastFocusedDocument);
        Assert.Contains(nameof(LayoutRoot.LastFocusedDocument), rootChanges);
        Assert.Contains(nameof(LayoutContent.IsLastFocusedDocument), documentChanges);

        workspace.Root.ActiveContent = null;
        Assert.Null(workspace.Root.LastFocusedDocument);
        Assert.False(tool.IsLastFocusedDocument);
    }

    [AvaloniaTheory]
    [InlineData("layout")]
    [InlineData("content")]
    [InlineData("factory")]
    public void Active_Content_Notifications_Observe_Completed_Native_Activation_And_Allow_Redirection(string activation)
    {
        Workspace workspace = new();
        List<LayoutContent?> changes = new();
        workspace.Root.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(LayoutRoot.ActiveContent)) return;
            LayoutContent? active = workspace.Root.ActiveContent;
            changes.Add(active);
            Assert.True(active!.IsActive);
            if (ReferenceEquals(active, workspace.Floating)) workspace.Root.ActiveContent = workspace.Docked;
        };

        Activate(workspace, workspace.Floating, activation);

        AssertActive(workspace, workspace.Docked);
        Assert.Equal(new[] { workspace.Floating, workspace.Docked }, changes);
    }

    [AvaloniaFact]
    public void Removing_A_Floating_Window_Clears_Focus_Before_It_Joins_Another_Layout()
    {
        Workspace workspace = new();
        workspace.Floating.IsActive = true;
        workspace.Root.FloatingWindows.Remove(workspace.Window);
        Assert.Null(workspace.Root.ActiveContent);
        Assert.Null(workspace.Root.LastFocusedDocument);
        Assert.Null(workspace.Manager.ActiveContent);

        LayoutRoot other = new();
        other.FloatingWindows.Add(workspace.Window);
        other.ActiveContent = workspace.Floating;
        Assert.Same(workspace.Floating, other.ActiveContent);
        Assert.Same(workspace.Floating, other.LastFocusedDocument);
        Assert.Null(workspace.Root.ActiveContent);
        Assert.Null(workspace.Root.LastFocusedDocument);
    }

    [AvaloniaFact]
    public void Removing_The_Last_Document_Clears_Its_History_And_Preserves_An_Active_Tool()
    {
        Workspace workspace = new();
        LayoutAnchorable tool = new();
        workspace.Root.RootPanel.Children.Add(new LayoutAnchorablePane(tool));
        tool.IsActive = true;
        ((LayoutDocumentPane)workspace.Docked.Parent!).Children.Remove(workspace.Docked);

        Assert.Same(tool, workspace.Root.ActiveContent);
        Assert.Null(workspace.Root.LastFocusedDocument);
        Assert.False(workspace.Docked.IsLastFocusedDocument);
    }

    [AvaloniaFact]
    public void Replacing_The_Root_Panel_Clears_Active_Content_And_Document_History()
    {
        Workspace workspace = new();
        workspace.Root.RootPanel = new LayoutPanel(new LayoutDocumentPane());
        Assert.Null(workspace.Root.ActiveContent);
        Assert.Null(workspace.Manager.ActiveContent);
        Assert.Null(workspace.Root.LastFocusedDocument);
    }

    [AvaloniaFact]
    public void Direct_Native_Focus_Assignments_Update_The_Owning_Public_Layout()
    {
        Workspace workspace = new();
        workspace.FloatingRoot.FocusedDockable = workspace.Floating.DockModel;
        Assert.Same(workspace.Floating, workspace.Root.ActiveContent);
        Assert.Same(workspace.Floating.Content, workspace.Manager.ActiveContent);
        Assert.Same(workspace.Floating, workspace.Root.LastFocusedDocument);
        workspace.NativeRoot.FocusedDockable = null;
        Assert.Same(workspace.Floating, workspace.Root.ActiveContent);
        workspace.FloatingRoot.FocusedDockable = null;
        Assert.Null(workspace.Root.ActiveContent);
        Assert.Null(workspace.Root.LastFocusedDocument);
    }

    [AvaloniaFact]
    public void Unrelated_Native_Roots_Sharing_The_Factory_Do_Not_Replace_The_Public_Active_Content()
    {
        Workspace workspace = new();
        Document foreignDocument = new();
        DocumentDock foreignPane = new() { VisibleDockables = new AvaloniaList<IDockable> { foreignDocument } };
        RootDock foreignRoot = new() { VisibleDockables = new AvaloniaList<IDockable> { foreignPane } };
        workspace.Root.DockFactory.InitLayout(foreignRoot);
        workspace.Root.DockFactory.SetFocusedDockable(foreignPane, foreignDocument);

        Assert.Same(workspace.Docked, workspace.Root.ActiveContent);
        Assert.Same(workspace.Docked.Content, workspace.Manager.ActiveContent);
        Assert.Same(workspace.Docked, workspace.Root.LastFocusedDocument);
        Assert.Same(foreignDocument, foreignRoot.FocusedDockable);
    }

    [AvaloniaTheory]
    [InlineData("root")]
    [InlineData("previous")]
    [InlineData("next")]
    public void Document_History_Changing_Handlers_Can_Redirect_Activation(string source)
    {
        Workspace workspace = new();
        LayoutElement observed = source switch
        {
            "previous" => workspace.Docked,
            "next" => workspace.Floating,
            _ => workspace.Root
        };
        bool redirected = false;
        observed.PropertyChanging += (_, args) =>
        {
            if (redirected || args.PropertyName is not (nameof(LayoutRoot.LastFocusedDocument) or nameof(LayoutContent.IsLastFocusedDocument))) return;
            redirected = true;
            workspace.Root.ActiveContent = workspace.Docked;
        };

        workspace.Root.ActiveContent = workspace.Floating;

        Assert.True(redirected);
        AssertActive(workspace, workspace.Docked);
    }

    private static void Activate(Workspace workspace, LayoutContent content, string activation)
    {
        switch (activation)
        {
            case "layout": workspace.Root.ActiveContent = content; break;
            case "manager": workspace.Manager.ActiveContent = content.Content; break;
            case "content": content.IsActive = true; break;
            case "factory": workspace.Root.DockFactory.SetFocusedDockable((IDock)content.DockModel.Owner!, content.DockModel); break;
        }
    }

    private static void AssertActive(Workspace workspace, LayoutContent content)
    {
        Assert.Same(content, workspace.Root.ActiveContent);
        Assert.Same(content.Content, workspace.Manager.ActiveContent);
        Assert.Same(content, workspace.Root.LastFocusedDocument);
        Assert.True(content.IsActive);
        Assert.True(content.IsSelected);
        Assert.True(content.IsLastFocusedDocument);
        LayoutDocument other = ReferenceEquals(content, workspace.Docked) ? workspace.Floating : workspace.Docked;
        Assert.False(other.IsActive);
        Assert.False(other.IsLastFocusedDocument);
    }

    private sealed class Workspace
    {
        internal LayoutDocument Docked { get; } = new() { ContentId = "docked", Content = new object() };
        internal LayoutDocument Floating { get; } = new() { ContentId = "floating", Content = new object() };
        internal LayoutDocumentFloatingWindow Window { get; }
        internal LayoutRoot Root { get; }
        internal DockingManager Manager { get; }
        internal RootDock NativeRoot => (RootDock)Root.DockModel;
        internal RootDock FloatingRoot => (RootDock)Window.DockModel;

        internal Workspace()
        {
            Root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(Docked)) };
            Window = new() { RootDocument = Floating };
            Root.FloatingWindows.Add(Window);
            Manager = new() { Layout = Root };
            Root.ActiveContent = Docked;
        }
    }
}
