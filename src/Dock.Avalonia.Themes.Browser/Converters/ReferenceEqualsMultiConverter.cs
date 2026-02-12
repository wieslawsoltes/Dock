using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Themes.Browser.Converters;

/// <summary>
/// Compares two bindings and returns whether they reference the same instance.
/// </summary>
public sealed class ReferenceEqualsMultiConverter : IMultiValueConverter
{
    /// <summary>
    /// Shared singleton instance.
    /// </summary>
    public static readonly ReferenceEqualsMultiConverter Instance = new();

    /// <summary>
    /// Returns <see langword="true"/> when the first two values are the same reference.
    /// </summary>
    /// <param name="values">Input values from the multi-binding.</param>
    /// <param name="targetType">Target property type.</param>
    /// <param name="parameter">Optional converter parameter.</param>
    /// <param name="culture">Converter culture.</param>
    /// <returns><see langword="true"/> when both values reference the same object; otherwise <see langword="false"/>.</returns>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return false;
        }

        return ReferenceEquals(values[0], values[1]);
    }
}
