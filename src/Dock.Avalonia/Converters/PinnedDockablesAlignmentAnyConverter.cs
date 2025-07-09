using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Dock.Model.Core;
using Dock.Model.Controls;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Determines if any dockables match <see cref="ITool.PinnedAlignment"/>.
/// </summary>
public class PinnedDockablesAlignmentAnyConverter : IValueConverter
{
    /// <summary>
    /// Gets converter instance.
    /// </summary>
    public static readonly PinnedDockablesAlignmentAnyConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<IDockable> dockables && parameter is string p && Enum.TryParse<Alignment>(p, out var alignment))
        {
            return dockables.OfType<ITool>().Any(t => t.PinnedAlignment == alignment);
        }
        return false;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
