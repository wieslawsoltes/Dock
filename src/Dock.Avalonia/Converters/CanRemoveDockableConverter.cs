using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Determines whether a dockable can be removed from its parent.
/// Returns true if <c>CanCloseLastDockable</c> is true or there is more than one
/// visible dockable.
/// </summary>
internal class CanRemoveDockableConverter : IMultiValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly CanRemoveDockableConverter Instance = new();

    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 1 && values[0] is IDock dock)
        {
            return dock.CanCloseLastDockable || dock.OpenedDockablesCount > 1;
        }
        if (values.Count >= 2 && values[0] is bool canCloseLast && values[1] is int count)
        {
            return canCloseLast || count > 1;
        }

        return true;
    }
}
