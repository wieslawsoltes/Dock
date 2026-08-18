// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization.Metadata;
using Dock.Model.Core;

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

        if (TryAssignAotSafeCreator(typeInfo, type, listType))
        {
            return;
        }

        var elementType = type.GetGenericArguments()[0];
        var concreteListType = listType.MakeGenericType(elementType);
        typeInfo.CreateObject = () => Activator.CreateInstance(concreteListType)!;
    }

    private static bool TryAssignAotSafeCreator(JsonTypeInfo typeInfo, Type type, Type listType)
    {
        if (listType == typeof(ObservableCollection<>))
        {
            if (type == typeof(IList<IDockable>))
            {
                typeInfo.CreateObject = static () => new ObservableCollection<IDockable>();
                return true;
            }

            if (type == typeof(IList<IDockWindow>))
            {
                typeInfo.CreateObject = static () => new ObservableCollection<IDockWindow>();
                return true;
            }
        }

        if (listType == typeof(List<>))
        {
            if (type == typeof(IList<IDockable>))
            {
                typeInfo.CreateObject = static () => new List<IDockable>();
                return true;
            }

            if (type == typeof(IList<IDockWindow>))
            {
                typeInfo.CreateObject = static () => new List<IDockWindow>();
                return true;
            }
        }

        return false;
    }
}
