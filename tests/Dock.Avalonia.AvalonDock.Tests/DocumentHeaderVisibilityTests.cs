using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DocumentHeaderVisibilityTests
{
    [AvaloniaTheory]
    [InlineData("HeaderlessDocuments")]
    [InlineData("LegacyHeaderlessDocuments")]
    public void Compiled_Headerless_Panes_Hide_All_Header_Chrome_And_Keep_Their_Content(string key)
    {
        using Workspace workspace = new(key);
        LayoutDocumentPane model = workspace.Model;
        LayoutContent document = model.Children[0];
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(document)!;
        Control view = item.View;
        TextBox editor = Assert.IsType<TextBox>(document.Content);
        document.IsActive = true;
        workspace.Update();
        Assert.True(editor.Focus());
        workspace.AssertHeader(false);
        Assert.True(view.IsEffectivelyVisible);
        double fullHeight = view.Bounds.Height;

        model.ShowHeader = true;
        workspace.Update();
        workspace.AssertHeader(true);
        Assert.True(view.Bounds.Height < fullHeight);
        model.ShowHeader = false;
        workspace.Update();
        workspace.AssertHeader(false);
        Assert.Equal(fullHeight, view.Bounds.Height);
        Assert.True(editor.IsFocused);
        Assert.Same(document, model.SelectedContent);
        Assert.Same(document, workspace.Manager.Layout.ActiveContent);
        Assert.Same(item, workspace.Manager.GetLayoutItemFromModel(document));
        Assert.Same(view, item.View);
        Assert.True(view.IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void Header_Visibility_Combines_With_The_Live_Native_Theme_Resource()
    {
        using Workspace workspace = new();
        Assert.True(workspace.Model.ShowHeader);
        workspace.AssertHeader(true);
        workspace.Manager.Resources["DockDocumentControlTabStripVisible"] = false;
        workspace.Update();
        workspace.AssertHeader(false);
        workspace.Model.ShowHeader = false;
        workspace.Manager.Resources["DockDocumentControlTabStripVisible"] = true;
        workspace.Update();
        workspace.AssertHeader(false);
        workspace.Model.ShowHeader = true;
        workspace.Update();
        workspace.AssertHeader(true);
        workspace.Model.ShowHeader = false;
        workspace.Manager.Resources["DockDocumentControlTabStripVisible"] = false;
        workspace.Model.ShowHeader = true;
        workspace.Update();
        workspace.AssertHeader(false);
        workspace.Manager.Resources.Remove("DockDocumentControlTabStripVisible");
        workspace.Update();
        workspace.AssertHeader(true);
    }

    [AvaloniaFact]
    public void Each_Pane_Retains_Its_Header_Policy_After_A_Manager_Theme_Change()
    {
        using Workspace workspace = new();
        LayoutDocumentPane second = new(new LayoutDocument { Title = "Other pane", Content = new TextBox() }) { ShowHeader = false };
        workspace.Manager.Layout.RootPanel.Children.Add(second);
        workspace.Update();
        LayoutDocumentPaneControl firstControl = Find(workspace.Model);
        LayoutDocumentPaneControl secondControl = Find(second);
        AssertHeader(firstControl, true);
        AssertHeader(secondControl, false);
        workspace.Model.ShowHeader = false;
        second.ShowHeader = true;
        workspace.Manager.DocumentPaneControlStyle = (ControlTheme)workspace.Window.Resources["LegacyDocumentPane"]!;
        workspace.Update();
        Assert.Same(firstControl, Find(workspace.Model));
        Assert.Same(secondControl, Find(second));
        AssertHeader(firstControl, false);
        AssertHeader(secondControl, true);

        LayoutDocumentPaneControl Find(LayoutDocumentPane model) => workspace.Manager.GetVisualDescendants()
            .OfType<LayoutDocumentPaneControl>().Single(control => ReferenceEquals(control.Model, model));
    }

    [AvaloniaFact]
    public void Xml_Restoration_And_Manager_Reattachment_Keep_The_Headerless_Pane_Live()
    {
        using Workspace workspace = new("HeaderlessDocuments");
        LayoutContent document = workspace.Model.Children[0];
        object? content = document.Content;
        using StringWriter writer = new();
        XmlLayoutSerializer serializer = new(workspace.Manager);
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        workspace.Update();
        workspace.AssertHeader(false);
        Assert.False(workspace.Model.ShowHeader);
        Assert.Same(content, workspace.Model.Children[0].Content);
        LayoutDocumentPane restored = workspace.Model;
        workspace.Window.Content = null;
        workspace.Update();
        restored.ShowHeader = true;
        workspace.Window.Content = workspace.Manager;
        workspace.Update();
        workspace.AssertHeader(true);
        restored.ShowHeader = false;
        workspace.Update();
        workspace.AssertHeader(false);
        Assert.Same(content, workspace.Model.Children[0].Content);
    }

    [AvaloniaFact]
    public void Floating_And_Docked_Panes_Use_Their_Own_Header_Policy_And_Existing_Content_View()
    {
        using Workspace workspace = new("HeaderlessDocuments");
        LayoutDocumentPane original = workspace.Model;
        LayoutContent document = original.Children[0];
        Control view = workspace.Manager.GetLayoutItemFromModel(document)!.View;
        document.Float();
        workspace.Update();
        LayoutFloatingWindowControl floating = workspace.Manager.FloatingWindows.Single();
        LayoutDocumentPaneControl pane = floating.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Single();
        LayoutDocumentPane floatingModel = Assert.IsType<LayoutDocumentPane>(pane.Model);
        Assert.NotSame(original, floatingModel);
        Assert.True(floatingModel.ShowHeader);
        AssertHeader(pane, true);
        floatingModel.ShowHeader = false;
        workspace.Update();
        AssertHeader(pane, false);
        Assert.Contains(view, floating.GetVisualDescendants());
        Assert.True(view.IsEffectivelyVisible);
        document.Dock();
        workspace.Update();
        workspace.AssertHeader(false);
        Assert.Same(original, document.Parent);
        Assert.Same(view, workspace.Manager.GetLayoutItemFromModel(document)!.View);
        original.ShowHeader = true;
        workspace.Update();
        workspace.AssertHeader(true);
    }

    private static void AssertHeader(LayoutDocumentPaneControl pane, bool visible)
    {
        DocumentTabStrip strip = pane.GetVisualDescendants().OfType<DocumentTabStrip>().Single();
        Assert.Equal(visible, strip.IsVisible);
        Assert.Equal(visible, strip.IsEffectivelyVisible);
        Panel separator = pane.GetVisualDescendants().OfType<Panel>().Single(x => x.Name == "PART_DocumentSeperatorHost");
        Assert.Equal(visible, separator.IsVisible);
        Button? menu = strip.GetVisualDescendants().OfType<Button>().SingleOrDefault(x => x.Classes.Contains("avalondock-document-menu"));
        if (visible) Assert.NotNull(menu);
        if (menu is not null) Assert.Equal(visible, menu.IsEffectivelyVisible);
    }

    private sealed class Workspace : IDisposable
    {
        public PaneControlFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public LayoutDocumentPane Model => (LayoutDocumentPane)Manager.Layout.RootPanel.Children[0];

        public Workspace(string? key = null)
        {
            Manager = Window.FindControl<DockingManager>("Manager")!;
            if (key is not null)
                Manager.Layout = new LayoutRoot { RootPanel = new LayoutPanel((LayoutDocumentPane)Window.Resources[key]!) };
            Window.Show();
            Update();
        }

        public void AssertHeader(bool visible) => DocumentHeaderVisibilityTests.AssertHeader(
            Manager.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Single(), visible);

        public void Update()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            foreach (LayoutFloatingWindowControl window in Manager.FloatingWindows) window.UpdateLayout();
        }

        public void Dispose()
        {
            foreach (LayoutFloatingWindowControl window in Manager.FloatingWindows.ToArray()) window.Close();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
