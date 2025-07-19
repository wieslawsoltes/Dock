using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using AC = Avalonia.Controls;
using LayoutOrientation = Avalonia.Layout.Orientation;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Converters;
using Dock.Model.Core;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ConvertersTests
{
    [AvaloniaFact]
    public void IntLessThanConverter_Returns_Correct_Result()
    {
        var converter = new IntLessThanConverter { TrueIfLessThan = 5 };

        var less = converter.Convert(3, typeof(bool), null, CultureInfo.InvariantCulture);
        var greater = converter.Convert(6, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.True((bool)less!);
        Assert.False((bool)greater!);
    }

    [AvaloniaFact]
    public void IntLessThanConverter_ConvertBack_Not_Implemented()
    {
        var converter = new IntLessThanConverter();
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(true, typeof(int), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaFact]
    public void OrientationConverter_Converts_Both_Ways()
    {
        var resultH = OrientationConverter.Instance.Convert(Model.Core.Orientation.Horizontal, typeof(LayoutOrientation), null, CultureInfo.InvariantCulture);
        var resultV = OrientationConverter.Instance.Convert(Model.Core.Orientation.Vertical, typeof(LayoutOrientation), null, CultureInfo.InvariantCulture);
        var backH = OrientationConverter.Instance.ConvertBack(LayoutOrientation.Horizontal, typeof(Model.Core.Orientation), null, CultureInfo.InvariantCulture);
        var backV = OrientationConverter.Instance.ConvertBack(LayoutOrientation.Vertical, typeof(Model.Core.Orientation), null, CultureInfo.InvariantCulture);

        Assert.Equal(LayoutOrientation.Horizontal, resultH);
        Assert.Equal(LayoutOrientation.Vertical, resultV);
        Assert.Equal(Model.Core.Orientation.Horizontal, backH);
        Assert.Equal(Model.Core.Orientation.Vertical, backV);
    }

    [AvaloniaFact]
    public void DocumentTabOrientationConverter_Converts()
    {
        var resultLeft = DocumentTabOrientationConverter.Instance.Convert(DocumentTabLayout.Left, typeof(LayoutOrientation), null, CultureInfo.InvariantCulture);
        var resultRight = DocumentTabOrientationConverter.Instance.Convert(DocumentTabLayout.Right, typeof(LayoutOrientation), null, CultureInfo.InvariantCulture);
        var resultTop = DocumentTabOrientationConverter.Instance.Convert(DocumentTabLayout.Top, typeof(LayoutOrientation), null, CultureInfo.InvariantCulture);
        var resultTitle = DocumentTabOrientationConverter.Instance.Convert(DocumentTabLayout.TitleBar, typeof(LayoutOrientation), null, CultureInfo.InvariantCulture);
        var resultInvalid = DocumentTabOrientationConverter.Instance.Convert((DocumentTabLayout)123, typeof(LayoutOrientation), null, CultureInfo.InvariantCulture);

        Assert.Equal(LayoutOrientation.Vertical, resultLeft);
        Assert.Equal(LayoutOrientation.Vertical, resultRight);
        Assert.Equal(LayoutOrientation.Horizontal, resultTop);
        Assert.Equal(LayoutOrientation.Horizontal, resultTitle);
        Assert.Equal(AvaloniaProperty.UnsetValue, resultInvalid);
    }

    [AvaloniaFact]
    public void DocumentTabDockConverter_Converts()
    {
        var resultLeft = DocumentTabDockConverter.Instance.Convert(DocumentTabLayout.Left, typeof(AC.Dock), null, CultureInfo.InvariantCulture);
        var resultRight = DocumentTabDockConverter.Instance.Convert(DocumentTabLayout.Right, typeof(AC.Dock), null, CultureInfo.InvariantCulture);
        var resultTop = DocumentTabDockConverter.Instance.Convert(DocumentTabLayout.Top, typeof(AC.Dock), null, CultureInfo.InvariantCulture);
        var resultTitle = DocumentTabDockConverter.Instance.Convert(DocumentTabLayout.TitleBar, typeof(AC.Dock), null, CultureInfo.InvariantCulture);
        var resultInvalid = DocumentTabDockConverter.Instance.Convert((DocumentTabLayout)123, typeof(AC.Dock), null, CultureInfo.InvariantCulture);

        Assert.Equal(AC.Dock.Left, resultLeft);
        Assert.Equal(AC.Dock.Right, resultRight);
        Assert.Equal(AC.Dock.Top, resultTop);
        Assert.Equal(AC.Dock.Top, resultTitle);
        Assert.Equal(AvaloniaProperty.UnsetValue, resultInvalid);
    }

    [AvaloniaFact]
    public void GripModeConverters_Work_As_Expected()
    {
        Assert.Equal(1, GripModeConverters.GridRowAutoHideConverter.Convert(GripMode.AutoHide, typeof(int), null, CultureInfo.InvariantCulture));
        Assert.Equal(0, GripModeConverters.GridRowAutoHideConverter.Convert(GripMode.Visible, typeof(int), null, CultureInfo.InvariantCulture));

        Assert.True((bool)GripModeConverters.IsVisibleVisibleConverter.Convert(GripMode.Visible, typeof(bool), null, CultureInfo.InvariantCulture)!);
        Assert.False((bool)GripModeConverters.IsVisibleVisibleConverter.Convert(GripMode.Hidden, typeof(bool), null, CultureInfo.InvariantCulture)!);
    }

    [AvaloniaFact]
    public void CanRemoveDockableConverter_Evaluates_Dock_State()
    {
        var converter = CanRemoveDockableConverter.Instance;
        var dock = new RootDock { CanCloseLastDockable = false, OpenedDockablesCount = 1 };

        var resultDock = converter.Convert(new object?[] { dock }, typeof(bool), null, CultureInfo.InvariantCulture);
        var resultBool = converter.Convert(new object?[] { false, 1 }, typeof(bool), null, CultureInfo.InvariantCulture);
        var resultBoolTrue = converter.Convert(new object?[] { true, 0 }, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.False((bool)resultDock!);
        Assert.False((bool)resultBool!);
        Assert.True((bool)resultBoolTrue!);
    }
}
