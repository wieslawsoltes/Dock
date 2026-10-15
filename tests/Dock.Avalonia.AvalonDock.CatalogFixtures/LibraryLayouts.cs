using System.Xml;
using AvalonDock.Layout;

namespace Dock.Avalonia.AvalonDock.CatalogFixtures;

public class LibraryDocument : LayoutDocument
{
    public string? Category { get; set; }
    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("Category", Category);
        base.WriteXml(writer);
    }
    public override void ReadXml(XmlReader reader)
    {
        Category = reader.GetAttribute("Category");
        base.ReadXml(reader);
    }
}

public class LibraryTool : LayoutAnchorable { }
public class LibraryDocumentGroup : LayoutDocumentPaneGroup { }
public class LibraryDocumentWindow : LayoutDocumentFloatingWindow { }
