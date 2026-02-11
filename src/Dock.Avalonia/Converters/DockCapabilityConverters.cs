// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Model;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

internal sealed class DockCapabilityConverter : IValueConverter, IMultiValueConverter
{
    private readonly DockCapability _capability;

    public DockCapabilityConverter(DockCapability capability)
    {
        _capability = capability;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ConvertDockable(value);
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0)
        {
            return false;
        }

        // The first binding value must be the dockable context.
        return ConvertDockable(values[0]);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private object ConvertDockable(object? value)
    {
        if (value is not IDockable dockable)
        {
            return false;
        }

        var dockContext = DockCapabilityResolver.ResolveOperationDock(dockable);
        return DockCapabilityResolver.IsEnabled(dockable, _capability, dockContext);
    }
}

/// <summary>
/// Provides singleton converters for effective dock capability evaluation.
/// </summary>
public static class DockCapabilityConverters
{
    private static readonly DockCapabilityConverter s_canClose = new(DockCapability.Close);
    private static readonly DockCapabilityConverter s_canPin = new(DockCapability.Pin);
    private static readonly DockCapabilityConverter s_canFloat = new(DockCapability.Float);
    private static readonly DockCapabilityConverter s_canDrag = new(DockCapability.Drag);
    private static readonly DockCapabilityConverter s_canDrop = new(DockCapability.Drop);
    private static readonly DockCapabilityConverter s_canDockAsDocument = new(DockCapability.DockAsDocument);

    /// <summary>
    /// Evaluates effective close capability.
    /// </summary>
    public static readonly IValueConverter CanCloseConverter = s_canClose;

    /// <summary>
    /// Evaluates effective close capability for multi-binding scenarios.
    /// </summary>
    public static readonly IMultiValueConverter CanCloseMultiConverter = s_canClose;

    /// <summary>
    /// Evaluates effective pin capability.
    /// </summary>
    public static readonly IValueConverter CanPinConverter = s_canPin;

    /// <summary>
    /// Evaluates effective pin capability for multi-binding scenarios.
    /// </summary>
    public static readonly IMultiValueConverter CanPinMultiConverter = s_canPin;

    /// <summary>
    /// Evaluates effective float capability.
    /// </summary>
    public static readonly IValueConverter CanFloatConverter = s_canFloat;

    /// <summary>
    /// Evaluates effective float capability for multi-binding scenarios.
    /// </summary>
    public static readonly IMultiValueConverter CanFloatMultiConverter = s_canFloat;

    /// <summary>
    /// Evaluates effective drag capability.
    /// </summary>
    public static readonly IValueConverter CanDragConverter = s_canDrag;

    /// <summary>
    /// Evaluates effective drag capability for multi-binding scenarios.
    /// </summary>
    public static readonly IMultiValueConverter CanDragMultiConverter = s_canDrag;

    /// <summary>
    /// Evaluates effective drop capability.
    /// </summary>
    public static readonly IValueConverter CanDropConverter = s_canDrop;

    /// <summary>
    /// Evaluates effective drop capability for multi-binding scenarios.
    /// </summary>
    public static readonly IMultiValueConverter CanDropMultiConverter = s_canDrop;

    /// <summary>
    /// Evaluates effective dock-as-document capability.
    /// </summary>
    public static readonly IValueConverter CanDockAsDocumentConverter = s_canDockAsDocument;

    /// <summary>
    /// Evaluates effective dock-as-document capability for multi-binding scenarios.
    /// </summary>
    public static readonly IMultiValueConverter CanDockAsDocumentMultiConverter = s_canDockAsDocument;
}
