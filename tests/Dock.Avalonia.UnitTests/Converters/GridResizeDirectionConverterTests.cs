using System;
using System.Globalization;
using Avalonia;
using Dock.Avalonia.Converters;
using Xunit;
using ControlDirection = Avalonia.Controls.GridResizeDirection;
using ModelDirection = Dock.Model.Core.GridResizeDirection;

namespace Dock.Avalonia.UnitTests.Converters;

public class GridResizeDirectionConverterTests
{
    [Theory]
    [InlineData(ModelDirection.Rows, ControlDirection.Rows)]
    [InlineData(ModelDirection.Columns, ControlDirection.Columns)]
    [InlineData((ModelDirection)123, ControlDirection.Auto)]
    public void Maps_Model_Direction_Explicitly(ModelDirection value, ControlDirection expected)
    {
        Assert.Equal(expected, GridResizeDirectionConverter.Instance.Convert(value, typeof(ControlDirection), null, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Rows")]
    public void Unsupported_Values_Leave_The_Control_Default(object? value)
    {
        Assert.Same(AvaloniaProperty.UnsetValue, GridResizeDirectionConverter.Instance.Convert(value, typeof(ControlDirection), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Conversion_Is_OneWay()
    {
        Assert.Throws<NotSupportedException>(() => GridResizeDirectionConverter.Instance.ConvertBack(ControlDirection.Rows, typeof(ModelDirection), null, CultureInfo.InvariantCulture));
    }
}
