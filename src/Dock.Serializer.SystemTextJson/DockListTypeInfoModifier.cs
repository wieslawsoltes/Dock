// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Dock.Model.Core;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// Resolver modifier that makes <see cref="IList{T}"/> properties deserialize into a
/// configured concrete list type without replacing the built-in enumerable path.
/// </summary>
internal static class DockListTypeInfoModifier
{
    public static void Apply(JsonTypeInfo typeInfo, Type listType, IDockJsonCollectionFactory? collectionFactory, bool preserveSuppliedFactory)
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

        if (collectionFactory?.GetListFactory(type, listType, typeInfo.CreateObject) is { } createObject)
        {
            typeInfo.CreateObject = createObject;
            return;
        }

        if (TryAssignAotSafeCreator(typeInfo, type, listType))
        {
            return;
        }

        // External resolvers own their collection factories. In particular, generated contexts
        // can supply scalar/custom lists without implementing Dock's optional factory contract.
        if (preserveSuppliedFactory && typeInfo.CreateObject is not null)
        {
            return;
        }

        // Keep the existing reflection fallback on JIT runtimes. Native AOT needs a registered
        // constructor, and does not execute or retain this dynamic construction path.
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            typeInfo.CreateObject = () => throw new NotSupportedException(
                $"The list type '{listType}' for '{type}' has no factory. Register its closed collection type with DockJsonSerializable or supply collection metadata with CreateObject.");
            return;
        }

        var elementType = type.GetGenericArguments()[0];
        var concreteListType = listType.MakeGenericType(elementType);
        typeInfo.CreateObject = () => Activator.CreateInstance(concreteListType)!;
    }

    private static void AssignCreator<T>(JsonTypeInfo typeInfo) where T : class, new()
    {
        Func<object>? suppliedFactory = typeInfo.CreateObject;
        typeInfo.CreateObject = suppliedFactory is null
            ? static () => new T()
            : () => suppliedFactory() is T compatible ? compatible : new T();
    }

    private static bool TryAssignAotSafeCreator(JsonTypeInfo typeInfo, Type type, Type listType)
    {
        if (listType == typeof(ObservableCollection<>))
        {
            if (type == typeof(IList<IDockable>))
            {
                AssignCreator<ObservableCollection<IDockable>>(typeInfo);
                return true;
            }

            if (type == typeof(IList<IDockWindow>))
            {
                AssignCreator<ObservableCollection<IDockWindow>>(typeInfo);
                return true;
            }
        }

        if (listType == typeof(List<>))
        {
            if (type == typeof(IList<IDockable>))
            {
                AssignCreator<List<IDockable>>(typeInfo);
                return true;
            }

            if (type == typeof(IList<IDockWindow>))
            {
                AssignCreator<List<IDockWindow>>(typeInfo);
                return true;
            }
        }

        return false;
    }
}
