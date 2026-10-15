using System;
using System.IO;
using System.Linq;
using System.Xml;
using Avalonia.Controls;
using AvalonDock;
using AvalonDock.Generated;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;

namespace PackageConsumer;

internal static class GeneratedExtensionsProbe
{
    internal static void Verify()
    {
        if (Avalonia.Application.Current is not IAvalonDockGeneratedCatalogProvider)
            throw new Exception("The packaged generator did not attach the application catalog.");
        GeneratedConsumerDocument document = new() { ContentId = "generated", FileName = "generated.cs", Content = new object() };
        DockingManager manager = new()
        {
            Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) },
            Theme = new ResourceTheme(new Uri("avares://Dock.Avalonia.AvalonDock.PackageSmoke/GeneratedResources.axaml"))
        };
        if (!Equals(manager.FindResource("GeneratedMarker"), "packaged generated resource"))
            throw new Exception("The packaged compiler did not generate the theme resource factory.");
        XmlLayoutSerializer serializer = new(manager);
        using StringWriter text = new();
        serializer.Serialize(text);
        serializer.Deserialize(new StringReader(text.ToString()));
        GeneratedConsumerDocument restored = manager.Layout.Descendents().OfType<GeneratedConsumerDocument>().Single();
        if (restored.FileName != "generated.cs" || !ReferenceEquals(document.Content, restored.Content))
            throw new Exception("The packaged generated XML factory lost type, metadata, or content.");
    }

    private sealed class ResourceTheme(Uri uri) : Theme
    {
        public override Uri GetResourceUri() => uri;
    }
}

public sealed class GeneratedConsumerDocument : LayoutDocument
{
    public string? FileName { get; set; }
    public override void WriteXml(XmlWriter writer) { writer.WriteAttributeString("FileName", FileName); base.WriteXml(writer); }
    public override void ReadXml(XmlReader reader) { FileName = reader.GetAttribute("FileName"); base.ReadXml(reader); }
}
