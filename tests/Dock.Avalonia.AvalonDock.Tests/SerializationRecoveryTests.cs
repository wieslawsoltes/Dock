using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SerializationRecoveryTests
{
    [AvaloniaTheory]
    [InlineData("shared", false)]
    [InlineData("shared", true)]
    [InlineData("", false)]
    [InlineData("", true)]
    public void Content_Recovery_Separates_Tools_And_Documents_And_Leaves_Callback_Icons_To_The_Application(string id, bool callback)
    {
        LayoutDocument document = new() { ContentId = id, Content = new object(), IconSource = new DrawingImage() };
        LayoutAnchorable tool = new() { ContentId = id, Content = new object(), IconSource = new DrawingImage() };
        DockingManager manager = new() { Layout = Layout(document, tool) };
        XmlLayoutSerializer serializer = new(manager);
        DrawingImage replacement = new();
        List<string> order = new();
        if (callback) serializer.LayoutSerializationCallback += (_, args) =>
        {
            bool isTool = args.Model is LayoutAnchorable;
            order.Add(isTool ? "tool" : "document");
            Assert.Same(isTool ? tool.Content : document.Content, args.Content);
            if (isTool) args.Model.IconSource = replacement;
        };

        serializer.Deserialize(new StringReader(Save(serializer)));

        LayoutDocument restoredDocument = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
        LayoutAnchorable restoredTool = Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());
        Assert.Same(document.Content, restoredDocument.Content);
        Assert.Same(tool.Content, restoredTool.Content);
        Assert.Same(callback ? null : document.IconSource, restoredDocument.IconSource);
        Assert.Same(callback ? replacement : tool.IconSource, restoredTool.IconSource);
        if (callback) Assert.Equal(new[] { "tool", "document" }, order);
    }

    [AvaloniaFact]
    public void Constructor_Snapshot_Retains_Models_With_Live_Identifiers_And_Excludes_Later_Additions()
    {
        LayoutDocument document = new() { ContentId = "old", Content = new object() };
        LayoutAnchorable tool = new() { ContentId = "old", Content = new object() };
        DockingManager manager = new() { Layout = Layout(document, tool) };
        XmlLayoutSerializer serializer = new(manager);
        document.ContentId = tool.ContentId = "renamed";
        ((LayoutDocumentPane)document.Parent!).Children.Add(new LayoutDocument { ContentId = "later", Content = new object() });
        ((LayoutAnchorablePane)tool.Parent!).Children.Add(new LayoutAnchorable { ContentId = "later", Content = new object() });

        serializer.Deserialize(new StringReader(Save(serializer)));

        Assert.Same(document.Content, Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>()).Content);
        Assert.Same(tool.Content, manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(x => x.ContentId == "renamed").Content);
        Assert.Equal("later", Assert.Single(manager.Layout.Hidden).ContentId);
        Assert.Null(manager.Layout.Hidden[0].Content);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Already_Populated_Content_Is_Not_Resolved_Or_Overwritten(bool callback)
    {
        DockingManager manager = new() { Layout = Layout(new() { ContentId = "document", Content = "previous" }, new() { ContentId = "tool", Content = "previous" }) };
        ExposedSerializer serializer = new(manager);
        LayoutDocument document = new() { ContentId = "document", Content = new object(), IconSource = new DrawingImage() };
        LayoutAnchorable tool = new() { ContentId = "tool", Content = new object(), IconSource = new DrawingImage() };
        object documentContent = document.Content;
        object toolContent = tool.Content;
        int calls = 0;
        if (callback) serializer.LayoutSerializationCallback += (_, _) => calls++;

        serializer.Fix(Layout(document, tool));

        Assert.Equal(0, calls);
        Assert.Same(documentContent, document.Content);
        Assert.Same(toolContent, tool.Content);
        Assert.NotNull(document.IconSource);
        Assert.NotNull(tool.IconSource);
    }

    [AvaloniaFact]
    public void Tool_Callbacks_Run_Before_Document_Snapshot_And_Can_Populate_Or_Add_Documents()
    {
        LayoutDocument document = new() { ContentId = "populated-by-tool" };
        LayoutAnchorable tool = new() { ContentId = "tool" };
        LayoutRoot layout = Layout(document, tool);
        LayoutDocument added = new() { ContentId = "added-by-tool" };
        object populated = new();
        List<string> calls = new();
        ExposedSerializer serializer = new(new DockingManager());
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            calls.Add(Assert.IsType<string>(args.Model.ContentId));
            if (args.Model is LayoutAnchorable)
            {
                document.Content = populated;
                ((LayoutDocumentPane)document.Parent!).Children.Add(added);
            }
            args.Content = new object();
        };

        serializer.Fix(layout);

        Assert.Equal(new[] { "tool", "added-by-tool" }, calls);
        Assert.Same(populated, document.Content);
        Assert.NotNull(added.Content);
        Assert.Same(layout, added.Root);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Callback_Cancellation_Requests_Closure_With_Content_Events_Even_When_CanClose_Is_False(bool tool, bool veto)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        LayoutContent? restored = null;
        int closing = 0, closed = 0;
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            restored = args.Model;
            Assert.Null(args.Model.Root!.Manager);
            args.Model.Closing += (_, e) => { closing++; e.Cancel = veto; };
            args.Model.Closed += (_, _) => { closed++; Assert.Null(args.Model.Parent); };
            args.Cancel = true;
        };

        serializer.Deserialize(new StringReader(SingleContentXml(tool, "CanClose='false'")));

        Assert.NotNull(restored);
        Assert.Equal(1, closing);
        Assert.Equal(veto ? 0 : 1, closed);
        Assert.Equal(veto, manager.Layout.Descendents().Contains(restored));
        if (veto) Assert.Same(manager.Layout, restored.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Installed_Callback_Can_Keep_A_Placeholder_With_Null_Content(bool tool)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        serializer.LayoutSerializationCallback += (_, _) => { };
        serializer.Deserialize(new StringReader(SingleContentXml(tool)));
        LayoutContent content = Assert.Single(manager.Layout.Descendents().OfType<LayoutContent>());
        Assert.Null(content.Content);
        Assert.False(content is LayoutAnchorable { IsHidden: true });
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Assigning_Model_Content_While_Leaving_Callback_Content_Null_Closes_Documents_And_Hides_Tools(bool tool)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        LayoutContent? restored = null;
        object content = new();
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            restored = args.Model;
            args.Model.Content = content;
            Assert.Null(args.Content);
        };
        serializer.Deserialize(new StringReader(SingleContentXml(tool)));
        Assert.Same(content, restored!.Content);
        if (tool) Assert.Same(restored, Assert.Single(manager.Layout.Hidden));
        else Assert.Empty(manager.Layout.Descendents().OfType<LayoutDocument>());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Existing_Matching_Model_With_Null_Content_Is_Different_From_No_Previous_Model(bool tool)
    {
        DockingManager manager = new();
        XmlLayoutSerializer initial = new(manager);
        initial.LayoutSerializationCallback += (_, _) => { };
        initial.Deserialize(new StringReader(SingleContentXml(tool)));
        XmlLayoutSerializer serializer = new(manager);
        serializer.Deserialize(new StringReader(Save(serializer)));
        LayoutContent restored = Assert.Single(manager.Layout.Descendents().OfType<LayoutContent>());
        Assert.Null(restored.Content);
        Assert.False(restored is LayoutAnchorable { IsHidden: true });
    }

    private static LayoutRoot Layout(LayoutDocument document, LayoutAnchorable tool) =>
        new() { RootPanel = new() { Children = { new LayoutDocumentPane(document), new LayoutAnchorablePane(tool) } } };
    private static string Save(XmlLayoutSerializer serializer) { using StringWriter writer = new(); serializer.Serialize(writer); return writer.ToString(); }
    private static string SingleContentXml(bool tool, string attributes = "") => tool
        ? $"<LayoutRoot><RootPanel><LayoutAnchorablePane><LayoutAnchorable ContentId='item' {attributes}/></LayoutAnchorablePane></RootPanel></LayoutRoot>"
        : $"<LayoutRoot><RootPanel><LayoutDocumentPane><LayoutDocument ContentId='item' {attributes}/></LayoutDocumentPane></RootPanel></LayoutRoot>";
    private sealed class ExposedSerializer(DockingManager manager) : LayoutSerializer(manager)
    {
        public void Fix(LayoutRoot layout) => FixupLayout(layout);
    }
}
