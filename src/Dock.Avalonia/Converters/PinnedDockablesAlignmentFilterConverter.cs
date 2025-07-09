using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using Dock.Model.Core;
using Dock.Model.Controls;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Filters dockables by <see cref="ITool.PinnedAlignment"/>.
/// </summary>
public class PinnedDockablesAlignmentFilterConverter : IValueConverter
{
    /// <summary>
    /// Gets converter instance.
    /// </summary>
    public static readonly PinnedDockablesAlignmentFilterConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<IDockable> dockables && parameter is string p && Enum.TryParse<Alignment>(p, out var alignment))
        {
            return dockables.OfType<ITool>().Where(t => t.PinnedAlignment == alignment).ToList();
        }
        return AvaloniaProperty.UnsetValue;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
