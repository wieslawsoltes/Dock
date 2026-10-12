using System.Collections.Generic;
using System.Xml;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace AvalonDock.Infrastructure;

// The operation context travels with the reader through application ReadXml
// overrides. It neither relies on ambient state nor retains a manager or tree.
internal sealed class LayoutXmlReadContext(LayoutSerializationTypes? types = null)
{
    internal LayoutSerializationTypes? Types { get; } = types;
    internal List<LayoutContent> Selected { get; } = new();
    internal LayoutContent? Active;
}

internal sealed class LayoutXmlReader(XmlReader reader, LayoutXmlReadContext context) : XmlReader
{
    internal LayoutXmlReadContext Context { get; } = context;
    public override XmlReaderSettings? Settings => reader.Settings;
    public override int AttributeCount => reader.AttributeCount;
    public override string BaseURI => reader.BaseURI;
    public override int Depth => reader.Depth;
    public override bool EOF => reader.EOF;
    public override bool HasValue => reader.HasValue;
    public override bool IsEmptyElement => reader.IsEmptyElement;
    public override string LocalName => reader.LocalName;
    public override string NamespaceURI => reader.NamespaceURI;
    public override XmlNameTable NameTable => reader.NameTable;
    public override XmlNodeType NodeType => reader.NodeType;
    public override string Prefix => reader.Prefix;
    public override ReadState ReadState => reader.ReadState;
    public override string Value => reader.Value;
    public override string XmlLang => reader.XmlLang;
    public override XmlSpace XmlSpace => reader.XmlSpace;
    public override string? GetAttribute(string name) => reader.GetAttribute(name);
    public override string? GetAttribute(string name, string? namespaceURI) => reader.GetAttribute(name, namespaceURI);
    public override string GetAttribute(int i) => reader.GetAttribute(i);
    public override string? LookupNamespace(string prefix) => reader.LookupNamespace(prefix);
    public override bool MoveToAttribute(string name) => reader.MoveToAttribute(name);
    public override bool MoveToAttribute(string name, string? ns) => reader.MoveToAttribute(name, ns);
    public override void MoveToAttribute(int i) => reader.MoveToAttribute(i);
    public override bool MoveToElement() => reader.MoveToElement();
    public override bool MoveToFirstAttribute() => reader.MoveToFirstAttribute();
    public override bool MoveToNextAttribute() => reader.MoveToNextAttribute();
    public override bool Read() => reader.Read();
    public override bool ReadAttributeValue() => reader.ReadAttributeValue();
    public override void ResolveEntity() => reader.ResolveEntity();
    public override void Close() => reader.Close();
}
