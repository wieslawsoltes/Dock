using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvalonDock.Properties;

namespace AvalonDock.Converters;

/// <summary>Selects the restore or auto-hide label from the public command resources.</summary>
public class AnchorableContextMenuAutoHideHeaderConverter : IValueConverter
{
    /// <summary>Returns the restore label for an auto-hidden tool and the auto-hide label otherwise.</summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Resources.Window_Restore : Resources.Anchorable_AutoHide;

    /// <summary>Reverse conversion is not supported.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
