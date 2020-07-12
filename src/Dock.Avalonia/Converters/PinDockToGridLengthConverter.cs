using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters
{
    /// <summary>
    /// Converter used by PinHostControl to position PinDocks around rock docks
    /// </summary>
    public class PinDockToGridLengthConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts 3 bindings into <see cref="GridLength"/>
        /// This is used to puppeter RootDock content when there are PinDocks without AutoHide
        /// </summary>
        /// <param name="values">3 objects from binding are expected in exact order-</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 3 && values[0] is double panelWidth && values[1] is bool isExpanded && values[2] is bool autoHide)
            {
                if (autoHide || isExpanded == false)
                {
                    return new GridLength(0);
                }
                else
                {
                    return new GridLength(panelWidth);
                }

            }
            return new GridLength(0);
        }
    }
}
