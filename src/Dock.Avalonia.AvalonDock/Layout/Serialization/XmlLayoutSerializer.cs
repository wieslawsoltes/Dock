using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout.Serialization;

/// <summary>Reads and writes AvalonDock XML without runtime type discovery.</summary>
public class XmlLayoutSerializer : LayoutSerializer
{
    /// <summary>Creates a serializer for the manager.</summary>
    public XmlLayoutSerializer(DockingManager manager) : base(manager) { }
    /// <summary>Gets the application type factories used to restore custom XML nodes without reflection.</summary>
    /// <remarks>Unregistered subclasses retain the built-in XML node name. Register a subclass on both
    /// saving and restoring serializers to preserve its concrete type and virtual XML hooks.</remarks>
    public LayoutSerializationTypes KnownTypes { get; } = new();
    /// <summary>Writes to a caller-owned XML writer.</summary>
    public void Serialize(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        AvalonDockXml.Write(Manager.Layout, KnownTypes.Snapshot()).WriteTo(writer);
    }
    /// <summary>Writes to a caller-owned text writer.</summary>
    public void Serialize(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        using XmlWriter xml = XmlWriter.Create(writer, WriterSettings());
        Serialize(xml);
    }
    /// <summary>Writes to a caller-owned stream.</summary>
    public void Serialize(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using XmlWriter xml = XmlWriter.Create(stream, WriterSettings());
        Serialize(xml);
    }
    /// <summary>Writes to a file.</summary>
    public void Serialize(string filepath)
    {
        XElement layout = AvalonDockXml.Write(Manager.Layout, KnownTypes.Snapshot());
        using FileStream stream = File.Create(filepath);
        using XmlWriter writer = XmlWriter.Create(stream, WriterSettings());
        layout.WriteTo(writer);
    }
    /// <summary>Reads from a caller-owned stream.</summary>
    public void Deserialize(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using XmlReader xml = XmlReader.Create(stream, ReaderSettings());
        Deserialize(xml);
    }
    /// <summary>Reads from a caller-owned text reader.</summary>
    public void Deserialize(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        using XmlReader xml = XmlReader.Create(reader, ReaderSettings());
        Deserialize(xml);
    }
    /// <summary>Reads from a caller-owned XML reader and atomically replaces the layout.</summary>
    public void Deserialize(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        StartDeserialization();
        try
        {
            XElement xml = XElement.Load(reader);
            LayoutRoot layout = AvalonDockXml.Read(xml, KnownTypes.Snapshot());
            FixupLayout(layout);
            Manager.Layout = layout;
        }
        finally { EndDeserialization(); }
    }
    /// <summary>Reads a layout file.</summary>
    public void Deserialize(string filepath)
    {
        using FileStream stream = File.OpenRead(filepath);
        Deserialize(stream);
    }
    private static XmlWriterSettings WriterSettings() => new() { Indent = true, CloseOutput = false };
    private static XmlReaderSettings ReaderSettings() => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        CloseInput = false,
        MaxCharactersInDocument = 16 * 1024 * 1024
    };
}
