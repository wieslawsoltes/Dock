using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class NativeFloatingGroupTests
{
    [AvaloniaTheory]
    [InlineData("documents", false)]
    [InlineData("documents", true)]
    [InlineData("document-group", false)]
    [InlineData("document-group", true)]
    [InlineData("mixed", false)]
    [InlineData("mixed", true)]
    [InlineData("tools", false)]
    [InlineData("tools", true)]
    [InlineData("root", false)]
    [InlineData("root", true)]
    public void Native_Groups_Preserve_Every_Content_And_Return_Location_Through_Floating_Xml_And_Redocking(string shape, bool restore)
    {
        using Workspace workspace = new(shape);
        LayoutContent[] original = workspace.Source.Descendents().OfType<LayoutContent>().ToArray();
        LayoutElement[] parents = original.Select(content => (LayoutElement)content.Parent!).ToArray();
        workspace.Manager.Layout.ActiveContent = original[^1];
        workspace.Manager.DockFactory.FloatDockable(workspace.Source.DockModel);
        workspace.Update();

        LayoutGroupFloatingWindow floating = Assert.IsType<LayoutGroupFloatingWindow>(Assert.Single(workspace.Manager.Layout.FloatingWindows));
        Assert.Equal(original, floating.Descendents().OfType<LayoutContent>());
        Assert.Same(workspace.Source, Assert.Single(floating.RootPanels));
        for (int index = 0; index < original.Length; index++)
        {
            Assert.Same(parents[index], original[index].Parent);
            Assert.True(original[index].IsFloating);
            Assert.NotNull(original[index].PreviousContainer);
            Assert.Same(workspace.Manager.Layout, original[index].PreviousContainer!.Root);
        }
        Assert.Same(original[^1], workspace.Manager.Layout.ActiveContent);
        Assert.NotNull(workspace.Manager.Layout.RootPanel);
        LayoutGroupFloatingWindowControl host = Assert.IsType<LayoutGroupFloatingWindowControl>(Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Single(host.GetVisualDescendants().OfType<DockControl>());

        if (restore)
        {
            XmlLayoutSerializer serializer = new(workspace.Manager);
            using StringWriter writer = new();
            serializer.Serialize(writer);
            Assert.Contains("<LayoutGroupFloatingWindow>", writer.ToString());
            serializer.Deserialize(new StringReader(writer.ToString()));
            workspace.Update();
            floating = Assert.IsType<LayoutGroupFloatingWindow>(Assert.Single(workspace.Manager.Layout.FloatingWindows));
            Assert.Equal(original.Select(content => content.Content), floating.Descendents().OfType<LayoutContent>().Select(content => content.Content));
        }

        LayoutContent[] contents = floating.Descendents().OfType<LayoutContent>().ToArray();
        ILayoutContainer[] returnPanes = contents.Select(content => content.PreviousContainer!).ToArray();
        for (int index = 0; index < contents.Length; index++)
        {
            contents[index].Dock();
            Assert.Same(returnPanes[index], contents[index].Parent);
            Assert.False(contents[index].IsFloating);
        }
        workspace.Update();
        Assert.All(workspace.Manager.FloatingWindows, window => Assert.False(window.IsVisible));
        Assert.False(floating.IsVisible);
        Assert.Equal(contents.Length + (shape == "root" ? 0 : 1), workspace.Manager.Layout.Descendents().OfType<LayoutContent>().Count());
    }

    [AvaloniaFact]
    public void Floating_All_Documents_Keeps_Tab_Order_Selection_And_The_Original_Return_Pane()
    {
        using Workspace workspace = new("documents");
        LayoutDocumentPane pane = Assert.IsType<LayoutDocumentPane>(workspace.Source);
        LayoutContent[] documents = pane.Children.ToArray();
        pane.SelectedContentIndex = 1;
        workspace.Manager.DockFactory.FloatAllDockables(documents[1].DockModel);
        workspace.Update();
        LayoutGroupFloatingWindow floating = Assert.IsType<LayoutGroupFloatingWindow>(Assert.Single(workspace.Manager.Layout.FloatingWindows));
        LayoutDocumentPane floatingPane = Assert.IsType<LayoutDocumentPane>(Assert.Single(floating.RootPanels));
        Assert.Equal(documents, floatingPane.Children);
        Assert.Same(documents[1], floatingPane.SelectedContent);
        Assert.Equal(pane.DockWidth, floatingPane.DockWidth);
        Assert.All(documents, document => Assert.Same(pane, document.PreviousContainer));
        foreach (LayoutContent document in documents) document.Dock();
        workspace.Update();
        Assert.Equal(documents, pane.Children);
    }

    [AvaloniaTheory]
    [InlineData("documents")]
    [InlineData("document-group")]
    [InlineData("mixed")]
    [InlineData("root")]
    public void A_Nonfloatable_Descendant_Prevents_Group_Floating_Without_Changing_The_Root(string shape)
    {
        using Workspace workspace = new(shape);
        LayoutPanel panel = workspace.Manager.Layout.RootPanel;
        LayoutContent content = workspace.Source.Descendents().OfType<LayoutContent>().First();
        content.CanFloat = false;
        workspace.Manager.DockFactory.FloatDockable(workspace.Source.DockModel);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Same(panel, workspace.Manager.Layout.RootPanel);
        Assert.False(content.IsFloating);
    }

    [AvaloniaFact]
    public void Group_Host_Close_Uses_All_Document_And_Tool_Commands_And_View_Reattachment_Preserves_The_Model()
    {
        using Workspace workspace = new("mixed");
        workspace.Manager.DockFactory.FloatDockable(workspace.Source.DockModel);
        workspace.Update();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        LayoutDocument document = Assert.Single(host.Model.Descendents().OfType<LayoutDocument>());
        LayoutAnchorable tool = Assert.Single(host.Model.Descendents().OfType<LayoutAnchorable>());
        document.CanClose = false;
        host.Close();
        Assert.True(host.IsVisible);
        document.CanClose = true;
        tool.CanClose = false;
        host.Close();
        Assert.True(host.IsVisible);
        tool.CanClose = true;
        workspace.Window.Content = null;
        workspace.Update();
        Assert.Empty(workspace.Manager.FloatingWindows);
        workspace.Window.Content = workspace.Manager;
        workspace.Update();
        LayoutFloatingWindowControl replacement = Assert.Single(workspace.Manager.FloatingWindows);
        Assert.NotSame(host, replacement);
        Assert.Same(host.Model, replacement.Model);
        Assert.True(replacement.IsVisible);
    }

    private sealed class Workspace : IDisposable
    {
        public DockingManager Manager { get; }
        public Window Window { get; }
        public LayoutElement Source { get; }

        public Workspace(string shape)
        {
            LayoutDocument first = new() { ContentId = "first", Content = "First" };
            LayoutDocument second = new() { ContentId = "second", Content = "Second" };
            LayoutDocumentPane documents = new(first) { DockWidth = new GridLength(270), DockMinWidth = 180 };
            documents.Children.Add(second);
            LayoutPanel panel = new(new LayoutDocumentPane(new LayoutDocument { ContentId = "main", Content = "Main" }));
            if (shape == "documents") Source = documents;
            else if (shape == "document-group")
            {
                LayoutDocumentPaneGroup group = new(new LayoutDocumentPane(first)) { Orientation = Orientation.Vertical };
                group.Children.Add(new LayoutDocumentPane(second));
                Source = group;
            }
            else if (shape == "tools")
                Source = new LayoutPanel { Children = { new LayoutAnchorablePane(new LayoutAnchorable { ContentId = "tool1", Content = "Tool 1" }),
                    new LayoutAnchorablePane(new LayoutAnchorable { ContentId = "tool2", Content = "Tool 2" }) } };
            else Source = new LayoutPanel { Children = { new LayoutDocumentPane(first), new LayoutAnchorablePane(new LayoutAnchorable { ContentId = "tool", Content = "Tool" }) } };
            if (shape == "root") panel = (LayoutPanel)Source;
            else panel.Children.Add((ILayoutPanelElement)Source);
            Manager = new() { Layout = new LayoutRoot { RootPanel = panel } };
            Window = new() { Width = 850, Height = 550, Content = Manager };
            Window.Show();
            Update();
        }

        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose()
        {
            Window.Content = null;
            Update();
            Window.Close();
        }
    }
}
