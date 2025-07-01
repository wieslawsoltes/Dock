using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converter that clips <see cref="ToolControl"/> border to exclude selected tab item area.
/// </summary>
public sealed class ToolControlClipConverter : IMultiValueConverter
{
    /// <summary>
    /// Gets <see cref="ToolControlClipConverter"/> instance.
    /// </summary>
    public static readonly ToolControlClipConverter Instance = new();

    /// <inheritdoc />
    public object Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count != 3 ||
            values[0] is not IControl border ||
            values[1] is not ToolTabStrip tabStrip)
        {
            return new BindingNotification(
                new ArgumentException("Expecting border, tab strip and selected item."),
                BindingErrorType.Error);
        }

        var bounds = border.Bounds;
        var selectedItem = values[2];
        IControl? container = null;

        if (selectedItem is IControl ctrl)
        {
            container = ctrl;
        }
        else if (selectedItem is not null)
        {
            container = tabStrip.ItemContainerGenerator.ContainerFromItem(selectedItem) as IControl;
        }

        if (container is null)
        {
            return new RectangleGeometry { Rect = new Rect(bounds.Size) };
        }

        var topLeft = container.TranslatePoint(new Point(), border) ?? new Point();
        var gap = new Rect(topLeft, container.Bounds.Size);
        gap = bounds.Intersect(gap);

        return new CombinedGeometry(
            GeometryCombineMode.Exclude,
            new RectangleGeometry { Rect = new Rect(bounds.Size) },
            new RectangleGeometry { Rect = new Rect(gap.Position - bounds.Position, gap.Size) });
    }

    /// <inheritdoc />
    public IMultiValueConverter ProvideValue(IServiceProvider serviceProvider) => Instance;
}
