using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace AvalonDock.Infrastructure;

// Typed construction replaces XmlSerializer's runtime discovery. Every model
// hook sees the same live reader, including its depth, attributes and siblings.
internal static class LayoutXmlDeserializer
{
    internal static LayoutRoot Deserialize(XmlReader reader, LayoutSerializationTypes types)
    {
        LayoutXmlReader contextual = new(reader, new LayoutXmlReadContext(types));
        try
        {
            if (contextual.MoveToContent() != XmlNodeType.Element || contextual.LocalName != "LayoutRoot" || contextual.NamespaceURI.Length != 0)
                throw new InvalidOperationException("Expected an AvalonDock LayoutRoot element without an XML namespace.");
            LayoutRoot root = new();
            root.ReadXml(contextual);
            return root;
        }
        catch (Exception error) { throw ReadError(contextual, error); }
    }

    internal static bool ReadContainer(LayoutElement element, XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        element.VerifyAccess();
        reader.MoveToContent();
        bool empty = reader.IsEmptyElement;
        foreach (LayoutElement child in ReadChildren(reader))
        {
            switch (element)
            {
                case LayoutDocumentFloatingWindow document:
                    LayoutDocument rootDocument = (LayoutDocument)child;
                    document.RestoreXmlBounds(rootDocument);
                    document.RootDocument = rootDocument;
                    break;
                case LayoutAnchorableFloatingWindow tools:
                    LayoutAnchorablePaneGroup rootPanel = (LayoutAnchorablePaneGroup)child;
                    tools.RestoreXmlBounds(rootPanel);
                    tools.RootPanel = rootPanel;
                    break;
                case ILayoutGroup group:
                    group.InsertChildAt(group.ChildrenCount, child);
                    break;
                default:
                    throw new InvalidOperationException("This layout element cannot contain children.");
            }
        }
        if (empty && element is LayoutFloatingWindow && element is ILayoutElementWithVisibility visibility) visibility.ComputeVisibility();
        return empty;
    }

    internal static void ReadRoot(LayoutRoot root, XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        root.VerifyAccess();
        RequireElement(reader);
        bool empty = reader.IsEmptyElement;
        int depth = reader.Depth;
        reader.Read();
        if (empty) return;

        RootSections seen = RootSections.None;
        while (reader.MoveToContent() != XmlNodeType.EndElement || reader.Depth != depth)
        {
            RequireElement(reader);
            switch (reader.LocalName)
            {
                case "RootPanel":
                    if ((seen & RootSections.Panel) != 0) throw new XmlException("A layout can contain only one RootPanel.");
                    LayoutPanel panel = new();
                    LayoutGroupXml.ReadRootPanel(panel, reader);
                    LayoutGroupXml.ReadPositionable(panel, panel, reader);
                    List<ILayoutPanelElement> panels = new();
                    foreach (LayoutElement child in ReadChildren(reader))
                        if (child is ILayoutPanelElement panelElement) panels.Add(panelElement);
                    root.RootPanel = panel;
                    foreach (ILayoutPanelElement child in panels) panel.Children.Add(child);
                    seen |= RootSections.Panel;
                    break;
                case "TopSide":
                    ReadSide(root.TopSide = new LayoutAnchorSide(), reader);
                    seen |= RootSections.Top;
                    break;
                case "RightSide":
                    ReadSide(root.RightSide = new LayoutAnchorSide(), reader);
                    seen |= RootSections.Right;
                    break;
                case "LeftSide":
                    ReadSide(root.LeftSide = new LayoutAnchorSide(), reader);
                    seen |= RootSections.Left;
                    break;
                case "BottomSide":
                    ReadSide(root.BottomSide = new LayoutAnchorSide(), reader);
                    seen |= RootSections.Bottom;
                    break;
                case "FloatingWindows":
                    root.FloatingWindows.Clear();
                    List<LayoutFloatingWindow> windows = new();
                    foreach (LayoutElement child in ReadChildren(reader)) windows.Add((LayoutFloatingWindow)child);
                    foreach (LayoutFloatingWindow window in windows) root.FloatingWindows.Add(window);
                    seen |= RootSections.Floating;
                    break;
                case "Hidden":
                    root.Hidden.Clear();
                    List<LayoutAnchorable> hidden = new();
                    foreach (LayoutElement child in ReadChildren(reader)) hidden.Add((LayoutAnchorable)child);
                    foreach (LayoutAnchorable tool in hidden) root.Hidden.Add(tool);
                    seen |= RootSections.Hidden;
                    break;
                default:
                    throw UnknownElement(reader);
            }
        }
        if ((seen & RootSections.Panel) == 0) throw new XmlException("A layout must contain a RootPanel.");
        reader.ReadEndElement();

        // Preserve the adapter's support for compact layouts with omitted empty
        // sections. Canonical AvalonDock XML supplies all of these sections.
        if ((seen & RootSections.Top) == 0) root.TopSide = new LayoutAnchorSide();
        if ((seen & RootSections.Right) == 0) root.RightSide = new LayoutAnchorSide();
        if ((seen & RootSections.Left) == 0) root.LeftSide = new LayoutAnchorSide();
        if ((seen & RootSections.Bottom) == 0) root.BottomSide = new LayoutAnchorSide();
        if ((seen & RootSections.Floating) == 0) root.FloatingWindows.Clear();
        if ((seen & RootSections.Hidden) == 0) root.Hidden.Clear();
    }

    private static void ReadSide(LayoutAnchorSide side, XmlReader reader)
    {
        // A root replaces its side before reading it, then attaches the restored
        // groups only after the whole side succeeds. Standalone group reads append.
        List<LayoutAnchorGroup> groups = new();
        foreach (LayoutElement child in ReadChildren(reader))
            if (child is LayoutAnchorGroup group) groups.Add(group);
        foreach (LayoutAnchorGroup group in groups) side.Children.Add(group);
    }

    private static IEnumerable<LayoutElement> ReadChildren(XmlReader reader)
    {
        RequireElement(reader);
        bool empty = reader.IsEmptyElement;
        int depth = reader.Depth;
        reader.Read();
        if (empty) yield break;
        while (reader.NodeType != XmlNodeType.EndElement || reader.Depth != depth)
        {
            if (reader.NodeType == XmlNodeType.Whitespace)
            {
                reader.Read();
                continue;
            }
            if (reader.EOF) throw new XmlException("The layout container is incomplete.");
            yield return ReadElement(reader);
        }
        reader.ReadEndElement();
    }

    private static LayoutElement ReadElement(XmlReader reader)
    {
        LayoutElement? element;
        try
        {
            LayoutSerializationTypes? types = (reader as LayoutXmlReader)?.Context.Types;
            element = types?.TryCreate(reader.LocalName, out LayoutElement? custom) == true ? custom : reader.LocalName switch
            {
                "LayoutPanel" => new LayoutPanel(),
                "LayoutDocumentPaneGroup" => new LayoutDocumentPaneGroup(),
                "LayoutAnchorablePaneGroup" => new LayoutAnchorablePaneGroup(),
                "LayoutDocumentPane" => new LayoutDocumentPane(),
                "LayoutAnchorablePane" => new LayoutAnchorablePane(),
                "LayoutDocument" => new LayoutDocument(),
                "LayoutAnchorable" => new LayoutAnchorable(),
                "LayoutAnchorGroup" => new LayoutAnchorGroup(),
                "LayoutAnchorSide" => new LayoutAnchorSide(),
                "LayoutDocumentFloatingWindow" => new LayoutDocumentFloatingWindow(),
                "LayoutAnchorableFloatingWindow" => new LayoutAnchorableFloatingWindow(),
                "LayoutGroupFloatingWindow" => new LayoutGroupFloatingWindow(),
                _ => null
            };
        }
        catch (Exception error) { throw ReadError(reader, error); }
        // Type discovery happens outside the nested XmlSerializer call upstream.
        if (element is null) throw UnknownElement(reader);
        try
        {
            if (reader.NodeType != XmlNodeType.Element || reader.NamespaceURI.Length != 0)
                throw new InvalidOperationException("Expected a layout element without an XML namespace.");
            ((IXmlSerializable)element).ReadXml(reader);
            return element;
        }
        catch (Exception error) { throw ReadError(reader, error); }
    }

    private static void RequireElement(XmlReader reader)
    {
        if (reader.MoveToContent() != XmlNodeType.Element) throw new XmlException("Expected a layout element.");
    }

    private static ArgumentException UnknownElement(XmlReader reader) => new($"Unknown AvalonDock element '{reader.LocalName}'.");

    private static InvalidOperationException ReadError(XmlReader reader, Exception error)
    {
        IXmlLineInfo? line = reader is LayoutXmlReader contextual ? contextual.LineInfo : reader as IXmlLineInfo;
        string message = line is null ? "There is an error in XML document." :
            string.Format(CultureInfo.InvariantCulture, "There is an error in XML document ({0}, {1}).", line.LineNumber, line.LinePosition);
        return new InvalidOperationException(message, error);
    }

    [Flags]
    private enum RootSections { None = 0, Panel = 1, Top = 2, Right = 4, Left = 8, Bottom = 16, Floating = 32, Hidden = 64 }
}
