using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class XmlNumericSelectionProbe
{
    internal static void Verify()
    {
        LayoutDocument document = new() { FloatingWidth = double.PositiveInfinity, FloatingLeft = double.NegativeInfinity };
        XElement xml = Write(document);
        if (xml.Attribute("FloatingWidth")?.Value != "Infinity" || xml.Attribute("FloatingLeft")?.Value != "-Infinity")
            throw new Exception("Packaged XML used numeric spellings that the AvalonDock reader cannot parse.");
        LayoutDocument restored = new();
        using (XmlReader reader = xml.CreateReader()) restored.ReadXml(reader);
        if (!double.IsPositiveInfinity(restored.FloatingWidth) || !double.IsNegativeInfinity(restored.FloatingLeft))
            throw new Exception("Packaged XML did not round-trip infinite floating dimensions.");

        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "minus";
        CultureInfo.CurrentCulture = culture;
        try
        {
            document.PreviousContainer = new LayoutDocumentPane();
            document.PreviousContainerIndex = -3;
            xml = Write(document);
            if (xml.Attribute("PreviousContainerIndex")?.Value != "minus3")
                throw new Exception("Packaged return-index formatting did not match the current-culture reader.");
            using XmlReader reader = xml.CreateReader();
            restored.ReadXml(reader);
            if (restored.PreviousContainerIndex != -3)
                throw new Exception("Packaged return-index metadata did not round-trip.");
        }
        finally { CultureInfo.CurrentCulture = previous; }

        ObservedPane pane = new();
        Read(pane, "<Pane><LayoutAnchorable /><LayoutAnchorable IsSelected='true' /></Pane>");
        if (pane.Selections.Count != 2 || pane.Selections[0] != -1 || pane.Selections[1] != 1 || pane.SelectedContentIndex != 1)
            throw new Exception("Packaged tool-pane XML selected a fallback before explicit selection was restored.");
        ObservedPane failed = new();
        try { Read(failed, "<Pane DockMinWidth='invalid' />"); }
        catch (FormatException) { }
        failed.Children.Add(new LayoutAnchorable());
        if (failed.SelectedContentIndex != -1)
            throw new Exception("A failed packaged tool-pane read incorrectly reenabled automatic selection.");
        Read(failed, "<Pane />");
        if (failed.SelectedContentIndex != 0)
            throw new Exception("A successful packaged tool-pane read did not restore automatic selection.");
    }

    private static XElement Write(LayoutContent content)
    {
        XDocument document = new();
        using (XmlWriter writer = document.CreateWriter())
        {
            writer.WriteStartElement("Content");
            content.WriteXml(writer);
            writer.WriteEndElement();
        }
        return document.Root!;
    }

    private static void Read(LayoutAnchorablePane pane, string xml)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        reader.MoveToContent();
        pane.ReadXml(reader);
    }

    private sealed class ObservedPane : LayoutAnchorablePane
    {
        internal List<int> Selections { get; } = new();
        protected override void OnChildrenCollectionChanged()
        {
            base.OnChildrenCollectionChanged();
            Selections.Add(SelectedContentIndex);
        }
    }
}
