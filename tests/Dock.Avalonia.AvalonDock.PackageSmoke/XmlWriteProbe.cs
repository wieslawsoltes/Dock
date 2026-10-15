using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Avalonia.Layout;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class XmlWriteProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("document defaults", () =>
        {
            XElement xml = Write(new LayoutDocument());
            if (xml.Attributes().Count() != 1 || xml.Attribute("CanClose")?.Value != "True")
                throw new Exception("Default document attributes differ.");
        });
        Check("unused pane identity", () =>
        {
            LayoutDocumentPane pane = new();
            if (Write(pane).HasAttributes || !string.IsNullOrEmpty(pane.DockModel.Id))
                throw new Exception("Writing an unreferenced pane added attributes or an identifier.");
        });
        Check("tool inheritance order", () =>
        {
            LayoutAnchorable tool = new() { Title = "Tool", CanHide = false, AutoHideMinWidth = 25, AutoHideMinHeight = 25, CanFloat = false };
            XElement xml = Write(tool);
            if (string.Join(",", xml.Attributes().Select(attribute => attribute.Name.LocalName)) != "CanHide,Title,CanClose,CanFloat" || xml.Attribute("CanHide")?.Value != "False")
                throw new Exception("Tool metadata did not precede its base content attributes.");
        });
        Check("blank strings", () =>
        {
            XElement xml = Write(new LayoutDocument { Title = " ", ContentId = "", ToolTip = "\t", Description = " " });
            if (xml.Attributes().Count() != 1) throw new Exception("Whitespace-only metadata was persisted.");
        });
        Check("external inherited writer", () =>
        {
            if (Write(new OrientableGroup()).HasAttributes)
                throw new Exception("A base writer inferred concrete XML from an extra interface.");
        });
        if (failures.Count != 0) throw new Exception("Packaged XML write failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static XElement Write(IXmlSerializable element)
    {
        XDocument document = new();
        using (XmlWriter writer = document.CreateWriter())
        {
            writer.WriteStartElement("Item");
            element.WriteXml(writer);
            writer.WriteEndElement();
        }
        return document.Root!;
    }

    private sealed class OrientableGroup : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutOrientableGroup
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;
    }
}
