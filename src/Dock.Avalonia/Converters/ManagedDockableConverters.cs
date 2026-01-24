// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converter that resolves whether a managed dockable hosts a tool dock.
/// </summary>
public sealed class ManagedDockableIsToolWindowConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly ManagedDockableIsToolWindowConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isToolWindow = value is ManagedDockWindowDocument document
            && document.Window?.Layout?.ActiveDockable is IToolDock;

        if (parameter is bool invertBool && invertBool)
        {
            return !isToolWindow;
        }

        if (parameter is string text
            && string.Equals(text, "Not", StringComparison.OrdinalIgnoreCase))
        {
            return !isToolWindow;
        }

        return isToolWindow;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that extracts the tool dock from a managed window dockable.
/// </summary>
public sealed class ManagedDockableToolDockConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly ManagedDockableToolDockConverter Instance = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ManagedDockWindowDocument document)
        {
            return document.Window?.Layout?.ActiveDockable as IToolDock;
        }

        return null;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
