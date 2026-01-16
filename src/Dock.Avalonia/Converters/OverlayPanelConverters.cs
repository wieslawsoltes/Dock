// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts <see cref="OverlayHorizontalAlignment"/> to <see cref="HorizontalAlignment"/>.
/// </summary>
public sealed class OverlayHorizontalAlignmentConverter : IValueConverter
{
    /// <summary>
    /// Gets the converter instance.
    /// </summary>
    public static readonly OverlayHorizontalAlignmentConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            OverlayHorizontalAlignment.Left => HorizontalAlignment.Left,
            OverlayHorizontalAlignment.Center => HorizontalAlignment.Center,
            OverlayHorizontalAlignment.Right => HorizontalAlignment.Right,
            OverlayHorizontalAlignment.Stretch => HorizontalAlignment.Stretch,
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            HorizontalAlignment.Left => OverlayHorizontalAlignment.Left,
            HorizontalAlignment.Center => OverlayHorizontalAlignment.Center,
            HorizontalAlignment.Right => OverlayHorizontalAlignment.Right,
            HorizontalAlignment.Stretch => OverlayHorizontalAlignment.Stretch,
            _ => AvaloniaProperty.UnsetValue
        };
    }
}

/// <summary>
/// Converts <see cref="OverlayVerticalAlignment"/> to <see cref="VerticalAlignment"/>.
/// </summary>
public sealed class OverlayVerticalAlignmentConverter : IValueConverter
{
    /// <summary>
    /// Gets the converter instance.
    /// </summary>
    public static readonly OverlayVerticalAlignmentConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            OverlayVerticalAlignment.Top => VerticalAlignment.Top,
            OverlayVerticalAlignment.Center => VerticalAlignment.Center,
            OverlayVerticalAlignment.Bottom => VerticalAlignment.Bottom,
            OverlayVerticalAlignment.Stretch => VerticalAlignment.Stretch,
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            VerticalAlignment.Top => OverlayVerticalAlignment.Top,
            VerticalAlignment.Center => OverlayVerticalAlignment.Center,
            VerticalAlignment.Bottom => OverlayVerticalAlignment.Bottom,
            VerticalAlignment.Stretch => OverlayVerticalAlignment.Stretch,
            _ => AvaloniaProperty.UnsetValue
        };
    }
}

/// <summary>
/// Converts <see cref="OverlayVisibility"/> to a boolean for visibility bindings.
/// </summary>
public sealed class OverlayVisibilityToBoolConverter : IValueConverter
{
    /// <summary>
    /// Gets the converter instance.
    /// </summary>
    public static readonly OverlayVisibilityToBoolConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            OverlayVisibility.Visible => true,
            OverlayVisibility.Hidden => false,
            OverlayVisibility.Collapsed => false,
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}

/// <summary>
/// Converts a double to a uniform <see cref="CornerRadius"/>.
/// </summary>
public sealed class DoubleToCornerRadiusConverter : IValueConverter
{
    /// <summary>
    /// Gets the converter instance.
    /// </summary>
    public static readonly DoubleToCornerRadiusConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            CornerRadius radius => radius,
            double uniform => new CornerRadius(uniform),
            float uniform => new CornerRadius(uniform),
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            CornerRadius radius => radius.TopLeft,
            _ => AvaloniaProperty.UnsetValue
        };
    }
}

/// <summary>
/// Converts a boolean into overlay panel shadows.
/// </summary>
public sealed class OverlayShadowConverter : IValueConverter
{
    /// <summary>
    /// Gets the converter instance.
    /// </summary>
    public static readonly OverlayShadowConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool enabled && enabled)
        {
            return new BoxShadows(new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 6,
                Blur = 16,
                Spread = 0,
                Color = Color.FromArgb(0x40, 0, 0, 0)
            });
        }

        return default(BoxShadows);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}

/// <summary>
/// Converts backdrop blur inputs into a blur effect.
/// </summary>
public sealed class OverlayBackdropBlurEffectConverter : IMultiValueConverter
{
    /// <summary>
    /// Gets the converter instance.
    /// </summary>
    public static readonly OverlayBackdropBlurEffectConverter Instance = new();

    /// <inheritdoc />
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var enabled = values[0] as bool? ?? false;
        var radius = values[1] as double? ?? 0;

        if (!enabled)
        {
            return null!;
        }

        return new BlurEffect
        {
            Radius = radius > 0 ? radius : 12
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
