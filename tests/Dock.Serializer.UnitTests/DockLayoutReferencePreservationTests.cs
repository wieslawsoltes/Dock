using System;
using System.Collections.Generic;
using System.IO;
using Dock.Model.Core;
using Dock.Model.Inpc;
using Dock.Model.Inpc.Controls;
using Dock.Serializer.Xml;
using Dock.Serializer.Yaml;
using Xunit;
using NewtonsoftDockSerializer = Dock.Serializer.DockSerializer;
using SystemTextJsonDockSerializer = Dock.Serializer.SystemTextJson.DockSerializer;

namespace Dock.Serializer.UnitTests;

public class DockLayoutReferencePreservationTests
{
    public static IEnumerable<object[]> ReferencePreservingSerializerFactories =>
    [
        new object[] { "Newtonsoft", (Func<IDockSerializer>)(() => new NewtonsoftDockSerializer()) },
        new object[] { "SystemTextJson", (Func<IDockSerializer>)(() => new SystemTextJsonDockSerializer()) },
        new object[] { "Yaml", (Func<IDockSerializer>)(() => new DockYamlSerializer()) },
        new object[] { "Xml", (Func<IDockSerializer>)(() => new DockXmlSerializer()) }
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
    [MemberData(nameof(ReferencePreservingSerializerFactories))]
    public void SaveLoad_Roundtrip_PreservesActiveDockableIdentity(string serializerName, Func<IDockSerializer> factory)
    {
        var serializer = factory();
        var layout = CreateLayout();

        using var stream = new NonClosingMemoryStream();
        serializer.Save(stream, layout);
        Assert.True(stream.Length > 0, $"{serializerName} did not write any data.");

        stream.Position = 0;
        var loaded = serializer.Load<RootDock>(stream);

        Assert.NotNull(loaded);
        var documentDock = Assert.IsType<DocumentDock>(loaded!.VisibleDockables![0]);
        Assert.NotNull(documentDock.VisibleDockables);
        Assert.Same(documentDock.ActiveDockable, documentDock.VisibleDockables![0]);
    }

    private static RootDock CreateLayout()
    {
        var factory = new Factory();

        var documentDock = new DocumentDock
        {
            Id = "DocumentDock",
            Title = "Documents",
            VisibleDockables = factory.CreateList<IDockable>(),
        };

        var root = (RootDock)factory.CreateRootDock();
        root.Id = "Root";
        root.Title = "Root";
        root.VisibleDockables = factory.CreateList<IDockable>(documentDock);
        root.DefaultDockable = documentDock;

        factory.InitLayout(root);

        var document = new Document { Id = "Doc1", Title = "Document 1" };
        factory.AddDockable(documentDock, document);
        factory.SetActiveDockable(document);

        return root;
    }
}
