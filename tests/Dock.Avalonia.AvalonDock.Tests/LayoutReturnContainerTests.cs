using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutReturnContainerTests
{
    [AvaloniaTheory]
    [InlineData(false, "document")]
    [InlineData(false, "tool")]
    [InlineData(false, "anchor")]
    [InlineData(false, "panel")]
    [InlineData(true, "document")]
    [InlineData(true, "tool")]
    [InlineData(true, "anchor")]
    [InlineData(true, "panel")]
    public void Assignment_Notifies_Before_Initializing_Only_Serializable_Panes(bool tool, string kind)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutElement pane = kind switch
        {
            "document" => new LayoutDocumentPane(),
            "tool" => new LayoutAnchorablePane(),
            "anchor" => new LayoutAnchorGroup(),
            _ => new LayoutPanel()
        };
        content.PreviousContainerId = "stored-metadata";
        List<string> events = new();
        content.PropertyChanging += (_, args) => events.Add("changing:" + args.PropertyName);
        content.PropertyChanged += (_, args) =>
        {
            events.Add("changed:" + args.PropertyName);
            Assert.Same(pane, content.PreviousContainer);
            Assert.Equal(string.Empty, pane.DockModel.Id);
        };

        content.PreviousContainer = (ILayoutContainer)pane;
        content.PreviousContainer = (ILayoutContainer)pane;

        Assert.Equal(new[] { "changed:PreviousContainer" }, events);
        Assert.Equal("stored-metadata", content.PreviousContainerId);
        if (kind == "panel") Assert.Equal(string.Empty, pane.DockModel.Id);
        else Assert.True(Guid.TryParseExact(pane.DockModel.Id, "D", out _));
    }

    [AvaloniaTheory]
    [InlineData(false, "redirect")]
    [InlineData(false, "clear")]
    [InlineData(false, "throw")]
    [InlineData(true, "redirect")]
    [InlineData(true, "clear")]
    [InlineData(true, "throw")]
    public void Reentrant_Callback_Initializes_The_Final_Container_And_Exceptions_Interrupt_Initialization(bool tool, string action)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutDocumentPane first = new(), replacement = new();
        int calls = 0;
        content.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(content.PreviousContainer)) return;
            calls++;
            if (!ReferenceEquals(content.PreviousContainer, first)) return;
            if (action == "throw") throw new InvalidOperationException("application callback");
            content.PreviousContainer = action == "redirect" ? replacement : null;
        };

        if (action == "throw")
        {
            Assert.Throws<InvalidOperationException>(() => content.PreviousContainer = first);
            // A failed callback has already stored the reference; an equal
            // subsequent assignment does not retry that callback or assign an ID.
            content.PreviousContainer = first;
            Assert.Equal(1, calls);
            Assert.Same(first, content.PreviousContainer);
        }
        else
        {
            content.PreviousContainer = first;
            Assert.Equal(2, calls);
            Assert.Same(action == "redirect" ? replacement : null, content.PreviousContainer);
        }
        Assert.Equal(string.Empty, first.DockModel.Id);
        if (action == "redirect") Assert.True(Guid.TryParse(replacement.DockModel.Id, out _));
        else Assert.Equal(string.Empty, replacement.DockModel.Id);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Empty_Ids_Are_Not_Initialized_When_Referenced(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutDocumentPane pane = new();
        using (XmlReader reader = XmlReader.Create(new StringReader("<Pane Id=''/>"))) pane.ReadXml(reader);
        content.PreviousContainer = pane;
        content.PreviousContainerIndex = 2;
        using StringWriter text = new();
        using (XmlWriter writer = XmlWriter.Create(text))
        {
            writer.WriteStartElement("Content");
            content.WriteXml(writer);
            writer.WriteEndElement();
        }
        XElement xml = XElement.Parse(text.ToString());
        Assert.Equal(string.Empty, pane.DockModel.Id);
        Assert.Equal(string.Empty, (string?)xml.Attribute("PreviousContainerId"));
        Assert.Equal("2", (string?)xml.Attribute("PreviousContainerIndex"));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Xml_Identifiers_Are_Quiet_And_Return_Indices_Raise_Only_Changed_Notifications(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        List<string> events = new();
        content.PropertyChanging += (_, args) => events.Add("changing:" + args.PropertyName);
        content.PropertyChanged += (_, args) => events.Add("changed:" + args.PropertyName);
        const string xml = "<Content PreviousContainerId='' PreviousContainerIndex='2' InitialContainerId='initial' InitialContainerIndex='3'/>";
        for (int i = 0; i < 2; i++)
        {
            using XmlReader reader = XmlReader.Create(new StringReader(xml));
            content.ReadXml(reader);
        }
        content.PreviousContainerId = "new metadata";
        Assert.Equal(new[] { "changed:PreviousContainerIndex", "changed:InitialContainerIndex" }, events);
        Assert.Equal("new metadata", content.PreviousContainerId);
        Assert.Equal(2, content.PreviousContainerIndex);
        Assert.Null(content.PreviousContainer);
    }
}
