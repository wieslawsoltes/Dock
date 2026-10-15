using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Avalonia.Layout;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace PackageConsumer;

internal static class XmlReadInheritanceProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("additional side interface", () =>
        {
            ApplicationSide side = new();
            using XmlReader reader = XmlReader.Create(new StringReader("<Side Orientation='Horizontal'><LayoutAnchorGroup /></Side>"));
            side.ReadXml(reader);
            if (side.Orientation != Orientation.Vertical || side.ChildrenCount != 1 || !reader.EOF)
                throw new Exception("The base reader inferred metadata from an additional interface.");
        });
        Check("additional positionable interface", () =>
        {
            ApplicationGroup group = new();
            using XmlReader reader = XmlReader.Create(new StringReader("<Group Orientation='invalid' DockMinWidth='82'><LayoutDocumentPane /></Group>"));
            reader.MoveToContent();
            group.ReadXml(reader);
            if (group.Orientation != Orientation.Vertical || group.DockMinWidth != 82 || group.ChildrenCount != 1 || group.Reads != 1 || !reader.EOF)
                throw new Exception("The inherited reader did not preserve its positionable-only contract.");
        });
        Check("registered external reader", () =>
        {
            XmlLayoutSerializer serializer = new(new DockingManager());
            ApplicationGroup? restored = null;
            serializer.KnownTypes.Register("ApplicationGroup", () => restored = new ApplicationGroup());
            serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
            serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><ApplicationGroup Orientation='invalid' DockMinWidth='83'><LayoutDocumentPane><LayoutDocument ContentId='item' /></LayoutDocumentPane></ApplicationGroup></RootPanel></LayoutRoot>"));
            if (restored is not { Orientation: Orientation.Vertical, DockMinWidth: 83, Reads: 1, Depth: 2 })
                throw new Exception("The external base call lost model metadata or reader context.");
        });
        if (failures.Count != 0) throw new Exception("Packaged XML read inheritance failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private sealed class ApplicationSide : LayoutAnchorSide, ILayoutOrientableGroup
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;
    }

    private sealed class ApplicationGroup : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutOrientableGroup, ILayoutPanelElement
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;
        public int Reads { get; private set; }
        public int Depth { get; private set; }
        public override void ReadXml(XmlReader reader)
        {
            Depth = reader.Depth;
            base.ReadXml(reader);
            Reads++;
        }
    }
}
