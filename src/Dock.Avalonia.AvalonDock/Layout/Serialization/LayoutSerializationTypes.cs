using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace AvalonDock.Layout.Serialization;

/// <summary>Maps application layout types to XML names and typed factories without runtime type discovery.</summary>
/// <remarks>Register types before serializing or deserializing. Each operation takes a snapshot, so a
/// serialization callback can configure subsequent operations without changing the current traversal.</remarks>
public sealed class LayoutSerializationTypes
{
    private readonly Dictionary<string, Func<LayoutElement>> _factories = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, string> _names = new();

    /// <summary>Registers a custom document, tool, pane, group, side, or floating-window type.</summary>
    /// <typeparam name="T">The exact concrete type produced by the factory.</typeparam>
    /// <param name="xmlName">An unqualified XML element name, usually the application's CLR class name.</param>
    /// <param name="factory">Creates a new detached instance on each invocation.</param>
    /// <exception cref="ArgumentException">The name or type is already registered, reserved, or invalid.</exception>
    public void Register<T>(string xmlName, Func<T> factory) where T : LayoutElement, IXmlSerializable
    {
        ArgumentNullException.ThrowIfNull(xmlName);
        ArgumentNullException.ThrowIfNull(factory);
        XmlConvert.VerifyNCName(xmlName);
        if (IsReserved(xmlName) || typeof(T) == typeof(LayoutRoot))
            throw new ArgumentException("Built-in layout names and the layout root cannot be registered.", nameof(xmlName));
        if (_factories.ContainsKey(xmlName) || _names.ContainsKey(typeof(T)))
            throw new ArgumentException("Each XML name and layout type can be registered only once.", nameof(xmlName));
        _factories.Add(xmlName, () =>
        {
            T element = factory() ?? throw new InvalidOperationException("A layout type factory returned null.");
            if (element.GetType() != typeof(T) || element.Parent is not null || element is LayoutRoot)
                throw new InvalidOperationException("A layout type factory must return a detached instance of its registered type.");
            return element;
        });
        _names.Add(typeof(T), xmlName);
    }

    internal LayoutSerializationTypes Snapshot()
    {
        LayoutSerializationTypes result = new();
        foreach (KeyValuePair<string, Func<LayoutElement>> entry in _factories) result._factories.Add(entry.Key, entry.Value);
        foreach (KeyValuePair<Type, string> entry in _names) result._names.Add(entry.Key, entry.Value);
        return result;
    }

    internal bool TryCreate(string name, out LayoutElement? element)
    {
        element = _factories.TryGetValue(name, out Func<LayoutElement>? factory) ? factory() : null;
        return element is not null;
    }

    internal string? GetName(LayoutElement element) => _names.TryGetValue(element.GetType(), out string? name) ? name : null;

    private static bool IsReserved(string name) => name is
        "LayoutRoot" or "RootPanel" or "TopSide" or "RightSide" or "LeftSide" or "BottomSide" or "Hidden" or "FloatingWindows" or
        "LayoutPanel" or "LayoutDocumentPaneGroup" or "LayoutAnchorablePaneGroup" or "LayoutDocumentPane" or
        "LayoutAnchorablePane" or "LayoutDocument" or "LayoutAnchorable" or "LayoutAnchorGroup" or "LayoutAnchorSide" or
        "LayoutDocumentFloatingWindow" or "LayoutAnchorableFloatingWindow" or "LayoutGroupFloatingWindow";
}
