using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Determines if collection of dockables contains any items.
/// </summary>
public class DockablesAnyConverter : IValueConverter
{
    /// <summary>
    /// Gets converter instance.
    /// </summary>
    public static readonly DockablesAnyConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is IEnumerable<IDockable> dockables && dockables.Any();
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
