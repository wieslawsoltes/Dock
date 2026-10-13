using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace PackageConsumer;

internal static class XmlWriterContextProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("nested error boundaries", () =>
        {
            ApplicationException failure = new("external hook");
            XmlLayoutSerializer serializer = Serializer(new ExternalDocument(_ => throw failure));
            using StringWriter text = new();
            Exception? error = null;
            try { serializer.Serialize(text); }
            catch (Exception caught) { error = caught; }
            for (int index = 0; index < 3; index++)
            {
                if (error is not InvalidOperationException) throw new Exception("A nested serializer wrapper is missing.");
                error = error.InnerException;
            }
            if (!ReferenceEquals(error, failure)) throw new Exception("The original exception was replaced.");
        });
        Check("caller namespace context", () =>
        {
            using StringWriter text = new();
            using XmlWriter writer = XmlWriter.Create(text);
            writer.WriteStartElement("Envelope", "urn:envelope");
            writer.WriteAttributeString("xmlns", "external", null, "urn:external");
            writer.WriteAttributeString("xml", "lang", null, "pl-PL");
            XmlLayoutSerializer serializer = Serializer(new ExternalDocument(w =>
            {
                if (w.LookupPrefix("urn:external") != "external" || w.XmlLang != "pl-PL")
                    throw new Exception("An external override lost the caller's writer context.");
            }));
            serializer.Serialize(writer);
            writer.WriteEndElement();
        });
        Check("writer failure order", () =>
        {
            int calls = 0;
            using StringWriter text = new();
            using ObservingWriter writer = new(text) { FailOnDocument = true };
            try { Serializer(new ExternalDocument(_ => calls++)).Serialize(writer); }
            catch (InvalidOperationException) { }
            if (calls != 0) throw new Exception("A model hook ran before the preceding writer operation.");
        });
        Check("child namespace", () =>
        {
            using StringWriter text = new();
            using XmlWriter writer = XmlWriter.Create(text);
            writer.WriteStartElement("Pane", "urn:external");
            new LayoutDocumentPane(new LayoutDocument()).WriteXml(writer);
            writer.WriteEndElement();
            writer.Flush();
            if (XElement.Parse(text.ToString()).Element("LayoutDocument") is null)
                throw new Exception("A serialized child inherited the outer element's namespace.");
        });
        Check("nested flushes", () =>
        {
            using StringWriter text = new();
            using ObservingWriter writer = new(text);
            Serializer(new LayoutDocument()).Serialize(writer);
            if (writer.Flushes != 3) throw new Exception("Child, pane, and root serializer flushes were not delivered.");
        });
        Check("retained writer lifetime", () =>
        {
            XmlWriter? retained = null;
            using StringWriter text = new();
            Serializer(new ExternalDocument(w => retained = w)).Serialize(text);
            if (retained is null) throw new Exception("The external hook was not invoked.");
            retained.WriteComment("application continuation");
            retained.Flush();
            if (!text.ToString().Contains("application continuation", StringComparison.Ordinal))
                throw new Exception("The retained writer did not reach the caller's output.");
            retained.Close();
        });
        Check("stream encoding", () =>
        {
            using MemoryStream stream = new();
            Serializer(new LayoutDocument()).Serialize(stream);
            if (stream.ToArray()[0] != (byte)'<') throw new Exception("The stream contains an unexpected byte order mark.");
        });
        if (failures.Count != 0) throw new Exception("Packaged XML writer-context failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static XmlLayoutSerializer Serializer(LayoutDocument document) =>
        new(new DockingManager { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } });

    private sealed class ExternalDocument(Action<XmlWriter> callback) : LayoutDocument
    {
        public override void WriteXml(XmlWriter writer) { base.WriteXml(writer); callback(writer); }
    }

    private sealed class ObservingWriter(TextWriter text) : XmlTextWriter(text)
    {
        internal bool FailOnDocument { get; init; }
        internal int Flushes { get; private set; }
        public override void WriteStartElement(string? prefix, string localName, string? ns)
        {
            if (FailOnDocument && localName == "LayoutDocument") throw new InvalidOperationException("caller writer failure");
            base.WriteStartElement(prefix, localName, ns);
        }
        public override void Flush() { base.Flush(); Flushes++; }
    }
}
