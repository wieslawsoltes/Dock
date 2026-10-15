using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace PackageConsumer;

internal static class ContentXmlProbe
{
    internal static void Verify()
    {
        LayoutAnchorable tool = new() { ContentId = "previous" };
        bool selected = false;
        tool.IsSelectedChanged += (_, _) =>
        {
            if (tool.CanHide || tool.Title != "Restored" || tool.ContentId != "previous")
                throw new Exception("Packaged content XML did not restore derived metadata, title, and selection before the identifier.");
            selected = true;
        };
        Read(tool, "<Tool CanHide='false' Title='Restored' IsSelected='true' ContentId='restored' IsLastFocusedDocument='true' IsFloating='true' />");
        if (!selected || !tool.IsLastFocusedDocument || !tool.IsFloating || tool.IsActive || tool.ContentId != "restored")
            throw new Exception("Packaged content XML lost persisted flags or activated the content.");
        XDocument xml = new();
        using (XmlWriter writer = xml.CreateWriter())
        {
            writer.WriteStartElement("Tool");
            tool.WriteXml(writer);
            writer.WriteEndElement();
        }
        if (xml.Root?.Attribute("IsFloating")?.Value != "True")
            throw new Exception("Packaged content XML did not persist the floating flag.");

        ExtendedDocument document = new();
        using (XmlReader reader = Reader("<Document Title='read' Description='details'><Extra>extension</Extra></Document><Next />"))
        {
            document.ReadXml(reader);
            if (document.Title != "read" || document.Description != "details" || document.Extra != "extension" || reader.LocalName != "Next")
                throw new Exception("A packaged content ReadXml override could not read its own child element.");
        }

        LayoutDocument partial = new();
        using (XmlReader reader = Reader("<Document Title='retained' IsSelected='true' FloatingWidth='307' FloatingHeight='invalid' />"))
        {
            bool failed = false;
            try { partial.ReadXml(reader); }
            catch (FormatException) { failed = true; }
            if (!failed || partial.Title != "retained" || !partial.IsSelected || partial.FloatingWidth != 307 ||
                reader.NodeType != XmlNodeType.Attribute || reader.LocalName != "FloatingHeight")
                throw new Exception("Packaged content XML lost preceding state or the failing reader position.");
        }

        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        serializer.KnownTypes.Register("ApplicationDocument", static () => new ExtendedDocument());
        int callbacks = 0;
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            callbacks++;
            if (args.Model is not ExtendedDocument { Extra: "extension", IsSelected: true, IsLastFocusedDocument: true, IsActive: false } ||
                args.Model.Root?.ActiveContent is not null || args.Model.LastActivationTimeStamp != new DateTime(2001, 2, 3, 4, 5, 6))
                throw new Exception("Packaged serialization changed persisted focus metadata before the content callback.");
            args.Content = new object();
        };
        serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><LayoutDocumentPane><ApplicationDocument ContentId='id' IsSelected='true' IsLastFocusedDocument='true' LastActivationTimeStamp='2001-02-03T04:05:06'><Extra>extension</Extra></ApplicationDocument></LayoutDocumentPane></RootPanel><TopSide /><RightSide /><LeftSide /><BottomSide /><FloatingWindows /><Hidden /></LayoutRoot>"));
        if (callbacks != 1 || manager.Layout.ActiveContent is not null || manager.Layout.LastFocusedDocument is not null)
            throw new Exception("Packaged XML flags unexpectedly changed root focus.");
    }

    private static void Read(LayoutContent content, string xml)
    {
        using XmlReader reader = Reader(xml);
        content.ReadXml(reader);
    }

    private static XmlReader Reader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment });
        reader.MoveToContent();
        return reader;
    }

    private sealed class ExtendedDocument : LayoutDocument
    {
        internal string? Extra { get; private set; }
        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            Extra = reader.ReadElementContentAsString("Extra", "");
            reader.ReadEndElement();
        }
    }
}
