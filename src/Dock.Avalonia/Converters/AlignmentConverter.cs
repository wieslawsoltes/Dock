using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters
{
    /// <summary>
    /// Converts model <see cref="Model.Alignment"/> enum to avalonia <see cref="Dock"/> enum.
    /// </summary>
    public class AlignmentConverter : IValueConverter
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
                Model.Alignment alignment => alignment switch
                {
                    Model.Alignment.Unset => AvaloniaProperty.UnsetValue,
                    Model.Alignment.Left => global::Avalonia.Controls.Dock.Left,
                    Model.Alignment.Bottom => global::Avalonia.Controls.Dock.Bottom,
                    Model.Alignment.Right => global::Avalonia.Controls.Dock.Right,
                    Model.Alignment.Top => global::Avalonia.Controls.Dock.Top,
                    _ => throw new NotSupportedException($"Provided dock is not supported in Avalonia.")
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
                global::Avalonia.Controls.Dock dock => dock switch
                {
                    global::Avalonia.Controls.Dock.Left => Model.Alignment.Left,
                    global::Avalonia.Controls.Dock.Bottom => Model.Alignment.Bottom,
                    global::Avalonia.Controls.Dock.Right => Model.Alignment.Right,
                    global::Avalonia.Controls.Dock.Top => Model.Alignment.Top,
                    _ => Model.Alignment.Unset
                },
                _ => value
            };
        }
    }
}
