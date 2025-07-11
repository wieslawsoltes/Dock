// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Dock.Serializer.Yaml;

internal static class ListTypeConverter
{
    public static void Convert(object? obj, Type listType)
    {
        if (obj is null)
            return;
        if (obj is IEnumerable enumerable && obj is not string)
        {
            foreach (var item in enumerable)
            {
                Convert(item, listType);
            }
        }
        var type = obj.GetType();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || !property.CanWrite)
                continue;
            var value = property.GetValue(obj);
            if (value is null)
                continue;
            var propType = property.PropertyType;
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var elementType = propType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(listType.MakeGenericType(elementType))!;
                foreach (var item in (IEnumerable)value)
                {
                    list.Add(item);
                }
                foreach (var item in list)
                {
                    Convert(item, listType);
                }
                property.SetValue(obj, list);
            }
            else
            {
                Convert(value, listType);
            }
        }
    }
}
