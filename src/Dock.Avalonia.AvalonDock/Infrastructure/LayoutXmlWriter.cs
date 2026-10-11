using System.Xml;

namespace AvalonDock.Infrastructure;

// Marks a traversal whose return-container IDs have already been prepared.
// Virtual WriteXml hooks receive the same writer, avoiding another whole-tree
// identity scan for each base call. All XML operations remain caller-owned.
internal sealed class LayoutXmlWriter(XmlWriter writer) : XmlWriter
{
    public override XmlWriterSettings? Settings => writer.Settings;
    public override WriteState WriteState => writer.WriteState;
    public override string? XmlLang => writer.XmlLang;
    public override XmlSpace XmlSpace => writer.XmlSpace;
    public override void Flush() => writer.Flush();
    public override string? LookupPrefix(string ns) => writer.LookupPrefix(ns);
    public override void Close() => writer.Close();
    public override void WriteStartDocument() => writer.WriteStartDocument();
    public override void WriteStartDocument(bool standalone) => writer.WriteStartDocument(standalone);
    public override void WriteEndDocument() => writer.WriteEndDocument();
    public override void WriteDocType(string name, string? pubid, string? sysid, string? subset) =>
        writer.WriteDocType(name, pubid, sysid, subset);
    public override void WriteStartElement(string? prefix, string localName, string? ns) =>
        writer.WriteStartElement(prefix, localName, ns);
    public override void WriteEndElement() => writer.WriteEndElement();
    public override void WriteFullEndElement() => writer.WriteFullEndElement();
    public override void WriteStartAttribute(string? prefix, string localName, string? ns) =>
        writer.WriteStartAttribute(prefix, localName, ns);
    public override void WriteEndAttribute() => writer.WriteEndAttribute();
    public override void WriteCData(string? text) => writer.WriteCData(text);
    public override void WriteComment(string? text) => writer.WriteComment(text);
    public override void WriteProcessingInstruction(string name, string? text) => writer.WriteProcessingInstruction(name, text);
    public override void WriteEntityRef(string name) => writer.WriteEntityRef(name);
    public override void WriteCharEntity(char ch) => writer.WriteCharEntity(ch);
    public override void WriteWhitespace(string? ws) => writer.WriteWhitespace(ws);
    public override void WriteString(string? text) => writer.WriteString(text);
    public override void WriteSurrogateCharEntity(char lowChar, char highChar) => writer.WriteSurrogateCharEntity(lowChar, highChar);
    public override void WriteChars(char[] buffer, int index, int count) => writer.WriteChars(buffer, index, count);
    public override void WriteRaw(char[] buffer, int index, int count) => writer.WriteRaw(buffer, index, count);
    public override void WriteRaw(string data) => writer.WriteRaw(data);
    public override void WriteBase64(byte[] buffer, int index, int count) => writer.WriteBase64(buffer, index, count);
}
