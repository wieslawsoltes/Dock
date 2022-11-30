using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Json;

/// <inheritdoc/>
public class AvaloniaModelPolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    /// <inheritdoc/>
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Type == typeof(IDockable))
        {
            jsonTypeInfo.PolymorphismOptions =
                new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType,
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(Document), "Document"),
                        new JsonDerivedType(typeof(Tool), "Tool"),
                        new JsonDerivedType(typeof(DockDock), "DockDock"),
                        new JsonDerivedType(typeof(DocumentDock), "DocumentDock"),
                        new JsonDerivedType(typeof(ProportionalDock), "ProportionalDock"),
                        new JsonDerivedType(typeof(ProportionalDockSplitter), "ProportionalDockSplitter"),
                        new JsonDerivedType(typeof(RootDock), "RootDock"),
                        new JsonDerivedType(typeof(ToolDock), "ToolDock"),
                    }
                };
        }

        if (jsonTypeInfo.Type == typeof(IDock))
        {
            jsonTypeInfo.PolymorphismOptions =
                new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType,
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(DockDock), "DockDock"),
                        new JsonDerivedType(typeof(DocumentDock), "DocumentDock"),
                        new JsonDerivedType(typeof(ProportionalDock), "ProportionalDock"),
                        new JsonDerivedType(typeof(ProportionalDockSplitter), "ProportionalDockSplitter"),
                        new JsonDerivedType(typeof(RootDock), "RootDock"),
                        new JsonDerivedType(typeof(ToolDock), "ToolDock"),
                    }
                };
        }

        if (jsonTypeInfo.Type == typeof(IDockWindow))
        {
            jsonTypeInfo.PolymorphismOptions =
                new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType,
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(DockWindow), "DockWindow"),
                    }
                };
        }

        if (jsonTypeInfo.Type == typeof(IDocumentTemplate))
        {
            jsonTypeInfo.PolymorphismOptions =
                new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(DocumentTemplate), "DocumentTemplate"),
                    }
                };
        }

        return jsonTypeInfo;
    }
}
