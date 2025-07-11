// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Model.Controls;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converter that checks if the owner of a dockable is a <see cref="IToolDock"/>.
/// </summary>
public class OwnerIsToolDockConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly OwnerIsToolDockConverter Instance = new OwnerIsToolDockConverter();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is IToolDock;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
