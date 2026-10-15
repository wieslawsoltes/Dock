using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace AvalonDock.Infrastructure;

// Explicit codecs keep persistence independent of reflection and CLR type names.
internal static class AvalonDockXml
{
    internal static XElement Write(LayoutRoot layout, LayoutSerializationTypes? types = null)
    {
        XDocument document = new();
        using (XmlWriter writer = document.CreateWriter())
            Write(layout, writer, types);
        return document.Root!;
    }

    internal static void Write(LayoutRoot layout, XmlWriter writer, LayoutSerializationTypes? types)
    {
        // Reject unsupported native hierarchies before emitting any XML. This
        // validation belongs to the native extension, outside serializer errors.
        foreach (LayoutFloatingWindow window in layout.FloatingWindows) ValidateFloatingWindow(window);
        try
        {
            EnsureContainerIds(layout);
            LayoutXmlWriter contextual = new(writer, types);
            if (contextual.WriteState == WriteState.Start) contextual.WriteStartDocument();
            contextual.WriteStartElement("LayoutRoot", string.Empty);
            layout.WriteXml(contextual);
            contextual.WriteEndElement();
        }
        catch (Exception error) { throw WriteError(error); }
        // XmlSerializer flushes after its error boundary; final flush failures
        // retain their original identity. Nested flushes belong to their caller.
        writer.Flush();
    }

    private static void WriteRootContents(LayoutRoot layout, XmlWriter writer)
    {
        foreach (LayoutFloatingWindow window in layout.FloatingWindows) window.DockWindow.Save();
        WriteElement(layout.RootPanel, writer, "RootPanel");
        WriteElement(layout.TopSide, writer, "TopSide");
        WriteElement(layout.RightSide, writer, "RightSide");
        WriteElement(layout.LeftSide, writer, "LeftSide");
        WriteElement(layout.BottomSide, writer, "BottomSide");
        writer.WriteStartElement("FloatingWindows");
        foreach (LayoutFloatingWindow window in layout.FloatingWindows) WriteElement(window, writer);
        writer.WriteEndElement();
        writer.WriteStartElement("Hidden");
        foreach (LayoutAnchorable content in layout.Hidden) WriteElement(content, writer);
        writer.WriteEndElement();
    }

    private static void EnsureContainerIds(LayoutElement scope)
    {
        EnsureReferences(scope);
        foreach (ILayoutElement child in scope.Descendents()) EnsureReferences(child);
        HashSet<string> containerIds = new(StringComparer.Ordinal);
        // Keep pane IDs when resolving collisions with native group extensions,
        // matching the precedence used while restoring references.
        if (LayoutContainerIdentity.IsPane(scope)) EnsureId(scope);
        foreach (ILayoutElement child in scope.Descendents())
            if (LayoutContainerIdentity.IsPane(child)) EnsureId(child);
        if (!LayoutContainerIdentity.IsPane(scope)) EnsureId(scope);
        foreach (ILayoutElement child in scope.Descendents())
            if (!LayoutContainerIdentity.IsPane(child)) EnsureId(child);

        void EnsureId(ILayoutElement child)
        {
            if (child is not LayoutElement element || child is not (ILayoutPositionableGroup or LayoutAnchorGroup)) return;
            if (element.XmlId is null) return;
            // Native pane copies may share an Id. References in AvalonDock XML
            // must identify one return container, including across repeat saves.
            if (element.XmlId is { } current && containerIds.Add(current)) return;
            string id;
            do { id = Guid.NewGuid().ToString("N"); } while (!containerIds.Add(id));
            element.DockModel.Id = id;
        }

        static void EnsureReferences(ILayoutElement element)
        {
            if (element is LayoutContent content)
            {
                EnsureReference(content.PreviousContainer);
                EnsureReference(content.InitialContainer);
            }
            if (element is LayoutAnchorGroup group) EnsureReference(group.PreviousContainer);
        }

        static void EnsureReference(ILayoutContainer? container)
        {
            if (LayoutContainerIdentity.CanWriteReference(container) && container is LayoutElement { XmlId: null } element)
                element.DockModel.Id = Guid.NewGuid().ToString("N");
        }
    }

    private static void WriteElement(LayoutElement? element, XmlWriter writer, string? name = null, bool serialized = false)
    {
        if (element is LayoutFloatingWindow window) ValidateFloatingWindow(window);
        if (element is not null) name ??= (writer as LayoutXmlWriter)?.Types?.GetName(element);
        name ??= element switch
        {
            LayoutPanel => "LayoutPanel",
            LayoutDocumentPaneGroup => "LayoutDocumentPaneGroup",
            LayoutAnchorablePaneGroup => "LayoutAnchorablePaneGroup",
            LayoutDocumentPane => "LayoutDocumentPane",
            LayoutAnchorablePane => "LayoutAnchorablePane",
            LayoutDocument => "LayoutDocument",
            LayoutAnchorable => "LayoutAnchorable",
            LayoutAnchorGroup => "LayoutAnchorGroup",
            LayoutAnchorSide => "LayoutAnchorSide",
            LayoutDocumentFloatingWindow => "LayoutDocumentFloatingWindow",
            LayoutAnchorableFloatingWindow => "LayoutAnchorableFloatingWindow",
            LayoutGroupFloatingWindow => "LayoutGroupFloatingWindow",
            _ => throw new NotSupportedException("Unsupported AvalonDock layout node.")
        };
        if (serialized) writer.WriteStartElement(name, string.Empty);
        else writer.WriteStartElement(name);
        if (element is IXmlSerializable serializable) serializable.WriteXml(writer);
        writer.WriteEndElement();
    }

    private static void SerializeElement(LayoutElement element, XmlWriter writer)
    {
        try
        {
            if (writer.WriteState == WriteState.Start) writer.WriteStartDocument();
            WriteElement(element, writer, serialized: true);
        }
        catch (Exception error) { throw WriteError(error); }
        writer.Flush();
    }

    private static InvalidOperationException WriteError(Exception error) =>
        new("There was an error generating the XML document.", error);

    internal static void WriteContents(LayoutElement element, XmlWriter writer)
    {
        writer = PrepareWriter(element, writer);
        if (element is LayoutRoot root) WriteRootContents(root, writer);
        else if (element is ILayoutContainer container)
            foreach (ILayoutElement child in container.Children) SerializeElement((LayoutElement)child, writer);
    }

    internal static XmlWriter PrepareWriter(LayoutElement element, XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (writer is LayoutXmlWriter) return writer;
        EnsureContainerIds(element.Root as LayoutElement ?? element);
        return new LayoutXmlWriter(writer);
    }

    internal static void WriteFloatingContents(LayoutFloatingWindow window, XmlWriter writer)
    {
        writer = PrepareWriter(window, writer);
        window.DockWindow.Save();
        ValidateFloatingWindow(window);
        foreach (ILayoutElement child in window.Children) SerializeElement((LayoutElement)child, writer);
    }

    private static void ValidateFloatingWindow(LayoutFloatingWindow window)
    {
        RootDock root = window.NativeRoot;
        if (root.HiddenDockables is { Count: > 0 } || root.LeftPinnedDockables is { Count: > 0 } ||
            root.TopPinnedDockables is { Count: > 0 } || root.RightPinnedDockables is { Count: > 0 } ||
            root.BottomPinnedDockables is { Count: > 0 } || root.Windows is { Count: > 0 })
            throw new NotSupportedException("The native floating window contains content outside the AvalonDock floating hierarchy.");
        if (window is LayoutDocumentFloatingWindow document)
        {
            if (root.VisibleDockables is not { Count: 1 } children || children[0] is not DocumentDock pane ||
                (pane.VisibleDockables?.Count ?? 0) != document.ChildrenCount ||
                (document.RootDocument is { } content && !ReferenceEquals(pane.VisibleDockables![0], content.DockModel)))
                throw new NotSupportedException("An AvalonDock document floating window can contain only its root document.");
        }
        else if (window is LayoutAnchorableFloatingWindow tools &&
                 ((root.VisibleDockables?.Count ?? 0) != tools.ChildrenCount ||
                  (tools.RootPanel is { } panel && !ReferenceEquals(root.VisibleDockables![0], panel.DockModel))))
            throw new NotSupportedException("An AvalonDock tool floating window can contain only its root pane group.");
    }
}
