using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SerializationReturnLocationTests
{
    [AvaloniaTheory]
    [InlineData("document-previous")]
    [InlineData("tool-previous")]
    [InlineData("document-initial")]
    [InlineData("tool-initial")]
    [InlineData("anchor-previous")]
    public void Empty_Pane_Identifiers_Resolve_And_Survive_Repeat_Saves(string origin)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
        string xml = ReferenceXml(origin, true);
        for (int i = 0; i < 2; i++)
        {
            serializer.Deserialize(new StringReader(xml));
            LayoutContent source = manager.Layout.Descendents().OfType<LayoutContent>().Single(x => x.ContentId == "source");
            LayoutContent target = manager.Layout.Descendents().OfType<LayoutContent>().Single(x => x.ContentId == "target");
            if (!origin.EndsWith("initial", StringComparison.Ordinal)) Assert.Same(target.Parent, source.PreviousContainer);
            using StringWriter writer = new();
            serializer.Serialize(writer);
            xml = writer.ToString();
            XElement saved = XElement.Parse(xml);
            XElement savedTarget = saved.Descendants().Single(x => (string?)x.Attribute("ContentId") == "target");
            XElement savedSource = origin.StartsWith("anchor", StringComparison.Ordinal)
                ? saved.Descendants("LayoutAnchorGroup").Single()
                : saved.Descendants().Single(x => (string?)x.Attribute("ContentId") == "source");
            string attribute = origin.EndsWith("initial", StringComparison.Ordinal) ? "InitialContainerId" : "PreviousContainerId";
            Assert.Equal(string.Empty, (string?)savedTarget.Parent!.Attribute("Id"));
            Assert.Equal(string.Empty, (string?)savedSource.Attribute(attribute));
        }
    }

    [AvaloniaTheory]
    [InlineData("document-previous")]
    [InlineData("tool-previous")]
    [InlineData("document-initial")]
    [InlineData("tool-initial")]
    [InlineData("anchor-previous")]
    public void Unset_Pane_Identifiers_Do_Not_Resolve_Empty_References_Or_Invoke_Content_Callbacks(string origin)
    {
        DockingManager manager = new();
        LayoutRoot previous = manager.Layout;
        XmlLayoutSerializer serializer = new(manager);
        int calls = 0;
        serializer.LayoutSerializationCallback += (_, _) => calls++;

        Assert.Throws<ArgumentException>(() => serializer.Deserialize(new StringReader(ReferenceXml(origin, false))));

        Assert.Same(previous, manager.Layout);
        Assert.Equal(0, calls);
    }

    [AvaloniaTheory]
    [InlineData("")]
    [InlineData("shared")]
    public void Pane_Identifiers_Take_Precedence_Over_Native_Group_Extensions(string id)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
        string xml = $"<LayoutRoot><RootPanel Id='{id}'><LayoutDocumentPane Id='{id}'><LayoutDocument ContentId='target'/></LayoutDocumentPane><LayoutDocumentPane><LayoutDocument ContentId='source' PreviousContainerId='{id}'/></LayoutDocumentPane></RootPanel></LayoutRoot>";
        serializer.Deserialize(new StringReader(xml));
        LayoutDocument source = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => x.ContentId == "source");
        LayoutDocument target = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => x.ContentId == "target");
        Assert.Same(target.Parent, source.PreviousContainer);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        Assert.Equal(id, ((LayoutElement)target.Parent!).DockModel.Id);
        Assert.NotEqual(id, manager.Layout.RootPanel.DockModel.Id);
    }

    [AvaloniaFact]
    public void Previous_References_Are_Resolved_Before_Initial_References_And_Content_Callbacks()
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        List<string> events = new();
        serializer.KnownTypes.Register("ObservedDocument", () =>
        {
            ObservedDocument document = new();
            document.PropertyChanging += (_, args) =>
            {
                if (args.PropertyName is "PreviousContainer" or "InitialContainer") events.Add("changing:" + args.PropertyName);
            };
            document.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is "PreviousContainer" or "InitialContainer") events.Add(document.ContentId + ":" + args.PropertyName);
            };
            return document;
        });
        serializer.LayoutSerializationCallback += (_, args) => { events.Add(args.Model.ContentId + ":content"); args.Content = new object(); };
        const string xml = "<LayoutRoot><RootPanel><LayoutDocumentPane Id='pane'><ObservedDocument ContentId='first' PreviousContainerId='pane' InitialContainerId='pane'/><ObservedDocument ContentId='second' PreviousContainerId='pane' InitialContainerId='pane'/></LayoutDocumentPane></RootPanel></LayoutRoot>";

        serializer.Deserialize(new StringReader(xml));

        Assert.Equal(new[] { "first:PreviousContainer", "second:PreviousContainer", "first:InitialContainer", "second:InitialContainer", "first:content", "second:content" }, events);
    }

    private static string ReferenceXml(string origin, bool explicitId)
    {
        string attribute = origin.EndsWith("initial", StringComparison.Ordinal) ? "InitialContainerId" : "PreviousContainerId";
        string source = origin.StartsWith("anchor", StringComparison.Ordinal)
            ? "<LeftSide><LayoutAnchorGroup PreviousContainerId=''><LayoutAnchorable ContentId='source'/></LayoutAnchorGroup></LeftSide>"
            : origin.StartsWith("tool", StringComparison.Ordinal)
                ? $"<LayoutAnchorablePane><LayoutAnchorable ContentId='source' {attribute}=''/></LayoutAnchorablePane>"
                : $"<LayoutDocumentPane><LayoutDocument ContentId='source' {attribute}=''/></LayoutDocumentPane>";
        string target = $"<LayoutDocumentPane {(explicitId ? "Id=''" : "")}><LayoutDocument ContentId='target'/></LayoutDocumentPane>";
        return origin.StartsWith("anchor", StringComparison.Ordinal)
            ? $"<LayoutRoot><RootPanel>{target}</RootPanel>{source}</LayoutRoot>"
            : $"<LayoutRoot><RootPanel>{target}{source}</RootPanel></LayoutRoot>";
    }

    private sealed class ObservedDocument : LayoutDocument { }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void WriteXml_Uses_The_Public_Return_Container_Instead_Of_Native_Float_History(bool tool, bool hasPrevious)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutDocumentPane nativePrevious = new();
        LayoutDocumentPane publicPrevious = new();
        content.DockModel.OriginalOwner = nativePrevious.DockModel;
        content.PreviousContainer = hasPrevious ? publicPrevious : null;
        content.PreviousContainerIndex = 3;
        using StringWriter text = new();
        using (XmlWriter writer = XmlWriter.Create(text))
        {
            writer.WriteStartElement("Content");
            content.WriteXml(writer);
            writer.WriteEndElement();
        }
        XElement xml = XElement.Parse(text.ToString());
        Assert.Equal(hasPrevious ? publicPrevious.DockModel.Id : null, (string?)xml.Attribute("PreviousContainerId"));
        Assert.Equal(hasPrevious ? "3" : null, (string?)xml.Attribute("PreviousContainerIndex"));
        Assert.Same(nativePrevious.DockModel, content.DockModel.OriginalOwner);
    }
}
