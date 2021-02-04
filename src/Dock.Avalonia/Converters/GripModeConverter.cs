using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters
{
    /// <summary>
    /// Converts model <see cref="GripMode"/> enum to avalonia IsVisible boolean.
    /// </summary>
    public class GripModeConverter : IValueConverter
    {
        /// <summary>
        /// Gets <see cref="AlignmentConverter"/> instance.
        /// </summary>
        public static readonly GripModeConverter Instance = new GripModeConverter();

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
                GripMode mode => mode switch
                {
                    GripMode.Visible => true,
                    GripMode.AutoHide => false,
                    GripMode.Hidden => false,
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
                bool isVisible => isVisible switch
                {
                    true => GripMode.Visible,
                    false => GripMode.Hidden
                },
                _ => value
            };
        }
    }
}
