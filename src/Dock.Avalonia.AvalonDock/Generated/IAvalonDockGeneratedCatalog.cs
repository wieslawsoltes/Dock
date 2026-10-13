using System;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;

namespace AvalonDock.Generated;

/// <summary>Supplies compile-time layout and resource factories without runtime assembly discovery.</summary>
public interface IAvalonDockGeneratedCatalog
{
    /// <summary>Adds the catalog's layout factories to an operation-owned registry.</summary>
    void RegisterLayoutTypes(LayoutSerializationTypes types);

    /// <summary>Adds the catalog's compiled theme factories to an operation-owned registry.</summary>
    void RegisterThemeResources(ThemeResourceRegistry resources);
}

/// <summary>Exposes an application's immutable generated compatibility catalog.</summary>
public interface IAvalonDockGeneratedCatalogProvider
{
    /// <summary>Gets the application's generated catalog.</summary>
    IAvalonDockGeneratedCatalog AvalonDockCatalog { get; }
}

/// <summary>Identifies a library catalog for composition by the application source generator.</summary>
/// <remarks>This attribute is consumed during compilation; runtime code does not inspect it.</remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class AvalonDockGeneratedCatalogAttribute : Attribute
{
    /// <summary>Identifies the generated catalog type.</summary>
    public AvalonDockGeneratedCatalogAttribute(Type catalogType) => CatalogType = catalogType;

    /// <summary>Gets the catalog type referenced by generated application code.</summary>
    public Type CatalogType { get; }
}

/// <summary>Describes a generated XML name for cross-assembly collision diagnostics.</summary>
/// <remarks>This attribute is consumed only by the application source generator.</remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AvalonDockGeneratedLayoutAttribute : Attribute
{
    /// <summary>Describes a generated custom layout name and its exact type.</summary>
    public AvalonDockGeneratedLayoutAttribute(string xmlName, Type layoutType)
    { XmlName = xmlName; LayoutType = layoutType; }
    /// <summary>Gets the unqualified XML element name.</summary>
    public string XmlName { get; }
    /// <summary>Gets the exact layout type.</summary>
    public Type LayoutType { get; }
}
