/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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

        // Console.WriteLine($"{jsonTypeInfo.Type}");

        if (jsonTypeInfo.Type == typeof(IDockable)
            || jsonTypeInfo.Type == typeof(DockableBase))
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

        if (jsonTypeInfo.Type == typeof(IDock)
            || jsonTypeInfo.Type == typeof(DockBase))
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

        if (jsonTypeInfo.Type == typeof(IRootDock)
            || jsonTypeInfo.Type == typeof(RootDock))
        {
            jsonTypeInfo.PolymorphismOptions =
                new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(RootDock), "RootDock"),
                    }
                };
        }

        if (jsonTypeInfo.Type == typeof(IDockWindow)
            || jsonTypeInfo.Type == typeof(DockWindow))
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

        if (jsonTypeInfo.Type == typeof(IDocumentTemplate)
            || jsonTypeInfo.Type == typeof(DocumentTemplate))
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
