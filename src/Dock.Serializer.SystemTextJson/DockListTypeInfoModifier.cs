// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// Resolver modifier that makes <see cref="IList{T}"/> properties deserialize into a
/// configured concrete list type without replacing the built-in enumerable path.
/// </summary>
internal static class DockListTypeInfoModifier
{
    public static void Apply(JsonTypeInfo typeInfo, Type listType)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Enumerable)
        {
            return;
        }

        var type = typeInfo.Type;
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(IList<>))
        {
            return;
        }

        var elementType = type.GetGenericArguments()[0];
        var concreteListType = listType.MakeGenericType(elementType);
        typeInfo.CreateObject = () => Activator.CreateInstance(concreteListType)!;
    }
}
