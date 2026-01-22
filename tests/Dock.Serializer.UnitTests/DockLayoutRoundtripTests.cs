using System;
using System.Collections.Generic;
using System.IO;
using Dock.Model.Core;
using Dock.Model.Inpc;
using Dock.Model.Inpc.Controls;
using Dock.Serializer.Protobuf;
using Dock.Serializer.Xml;
using Dock.Serializer.Yaml;
using Xunit;
using NewtonsoftDockSerializer = Dock.Serializer.DockSerializer;
using SystemTextJsonDockSerializer = Dock.Serializer.SystemTextJson.DockSerializer;

namespace Dock.Serializer.UnitTests;

public class DockLayoutRoundtripTests
{
    public static IEnumerable<object[]> SerializerFactories =>
    [
        new object[] { "Newtonsoft", (Func<IDockSerializer>)(() => new NewtonsoftDockSerializer()) },
        new object[] { "SystemTextJson", (Func<IDockSerializer>)(() => new SystemTextJsonDockSerializer()) },
        new object[] { "Yaml", (Func<IDockSerializer>)(() => new DockYamlSerializer()) },
        new object[] { "Xml", (Func<IDockSerializer>)(() => new DockXmlSerializer()) },
        new object[] { "Protobuf", (Func<IDockSerializer>)(() => new ProtobufDockSerializer()) }
    ];

    private sealed class NonClosingMemoryStream : MemoryStream
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Flush();
            }
        }
    }

    [Theory]
    [MemberData(nameof(SerializerFactories))]
    public void SaveLoad_Roundtrip_Works(string serializerName, Func<IDockSerializer> factory)
    {
        var serializer = factory();
        var layout = CreateLayout();

        using var stream = new NonClosingMemoryStream();
        serializer.Save(stream, layout);
        Assert.True(stream.Length > 0, $"{serializerName} did not write any data.");

        stream.Position = 0;
        var loaded = serializer.Load<RootDock>(stream);

        Assert.NotNull(loaded);
        AssertLayout(loaded!);
    }

    private static RootDock CreateLayout()
    {
        var factory = new Factory();

        var tool1 = new Tool { Id = "Tool1", Title = "Tool 1" };
        var tool2 = new Tool { Id = "Tool2", Title = "Tool 2" };
        var toolDock = new ToolDock
        {
            Id = "ToolDock",
            Title = "Tools",
            VisibleDockables = factory.CreateList<IDockable>(tool1, tool2),
            ActiveDockable = tool1,
            Proportion = 0.4
        };

        var document = new Document { Id = "Doc1", Title = "Document 1" };
        var documentDock = new DocumentDock
        {
            Id = "DocumentDock",
            Title = "Documents",
            VisibleDockables = factory.CreateList<IDockable>(document),
            ActiveDockable = document,
            Proportion = 0.6
        };

        var layout = new ProportionalDock
        {
            Id = "Layout",
            Title = "Layout",
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(
                toolDock,
                factory.CreateProportionalDockSplitter(),
                documentDock)
        };

        var root = (RootDock)factory.CreateRootDock();
        root.Id = "Root";
        root.Title = "Root";
        root.VisibleDockables = factory.CreateList<IDockable>(layout);
        root.ActiveDockable = layout;
        root.Windows = factory.CreateList<IDockWindow>();

        return root;
    }

    private static void AssertLayout(RootDock root)
    {
        Assert.Equal("Root", root.Id);
        Assert.NotNull(root.VisibleDockables);
        Assert.Single(root.VisibleDockables!);

        var layout = Assert.IsType<ProportionalDock>(root.VisibleDockables[0]);
        Assert.Equal("Layout", layout.Id);
        Assert.Equal(Orientation.Horizontal, layout.Orientation);
        Assert.NotNull(layout.VisibleDockables);
        Assert.Equal(3, layout.VisibleDockables!.Count);

        var toolDock = Assert.IsType<ToolDock>(layout.VisibleDockables[0]);
        Assert.Equal("ToolDock", toolDock.Id);
        Assert.Equal(0.4, toolDock.Proportion);
        Assert.NotNull(toolDock.VisibleDockables);
        Assert.Equal(2, toolDock.VisibleDockables!.Count);
        Assert.Equal("Tool1", toolDock.VisibleDockables[0].Id);
        Assert.Equal("Tool2", toolDock.VisibleDockables[1].Id);
        Assert.Equal("Tool1", toolDock.ActiveDockable?.Id);

        Assert.IsType<ProportionalDockSplitter>(layout.VisibleDockables[1]);

        var documentDock = Assert.IsType<DocumentDock>(layout.VisibleDockables[2]);
        Assert.Equal("DocumentDock", documentDock.Id);
        Assert.Equal(0.6, documentDock.Proportion);
        Assert.NotNull(documentDock.VisibleDockables);
        Assert.Single(documentDock.VisibleDockables!);
        Assert.Equal("Doc1", documentDock.VisibleDockables[0].Id);
        Assert.Equal("Doc1", documentDock.ActiveDockable?.Id);
    }
}
