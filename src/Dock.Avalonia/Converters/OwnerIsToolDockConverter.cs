using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Model.Controls;

namespace Dock.Avalonia.Converters;

internal class OwnerIsToolDockConverter : IValueConverter
{
    public static readonly OwnerIsToolDockConverter Instance = new OwnerIsToolDockConverter();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is IToolDock;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
