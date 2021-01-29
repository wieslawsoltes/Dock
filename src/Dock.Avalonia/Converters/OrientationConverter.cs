using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace Dock.Avalonia.Converters
{
    /// <summary>
    /// Converts model <see cref="Model.Orientation"/> enum to avalonia <see cref="Orientation"/> enum.
    /// </summary>
    public class OrientationConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="parameter">A user-defined parameter.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The converted value.</returns>
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                null => AvaloniaProperty.UnsetValue,
                Model.Orientation orientation => orientation switch
                {
                    Model.Orientation.Horizontal => Orientation.Horizontal,
                    Model.Orientation.Vertical => Orientation.Vertical,
                    _ => throw new NotSupportedException($"Provided orientation is not supported in Avalonia.")
                },
                _ => value
            };
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="parameter">A user-defined parameter.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The converted value.</returns>
        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                null => AvaloniaProperty.UnsetValue,
                Orientation orientation => orientation switch
                {
                    Orientation.Horizontal => Model.Orientation.Horizontal,
                    Orientation.Vertical => Model.Orientation.Vertical,
                    _ => throw new NotSupportedException($"Provided orientation is not supported in Model.")
                },
                _ => value
            };
        }
    }
}
