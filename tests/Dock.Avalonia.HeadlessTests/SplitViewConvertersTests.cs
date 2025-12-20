// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Globalization;
using Avalonia;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Converters;
using Dock.Model.Core;
using Xunit;
using AvaloniaSplitViewDisplayMode = Avalonia.Controls.SplitViewDisplayMode;
using AvaloniaSplitViewPanePlacement = Avalonia.Controls.SplitViewPanePlacement;

namespace Dock.Avalonia.HeadlessTests;

public class SplitViewConvertersTests
{
    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_Converts_Inline()
    {
        var result = SplitViewDisplayModeConverter.Instance.Convert(
            SplitViewDisplayMode.Inline,
            typeof(AvaloniaSplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaSplitViewDisplayMode.Inline, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_Converts_CompactInline()
    {
        var result = SplitViewDisplayModeConverter.Instance.Convert(
            SplitViewDisplayMode.CompactInline,
            typeof(AvaloniaSplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaSplitViewDisplayMode.CompactInline, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_Converts_Overlay()
    {
        var result = SplitViewDisplayModeConverter.Instance.Convert(
            SplitViewDisplayMode.Overlay,
            typeof(AvaloniaSplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaSplitViewDisplayMode.Overlay, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_Converts_CompactOverlay()
    {
        var result = SplitViewDisplayModeConverter.Instance.Convert(
            SplitViewDisplayMode.CompactOverlay,
            typeof(AvaloniaSplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaSplitViewDisplayMode.CompactOverlay, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_Invalid_Value_Returns_Default()
    {
        var result = SplitViewDisplayModeConverter.Instance.Convert(
            (SplitViewDisplayMode)999,
            typeof(AvaloniaSplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        // Invalid enum values return the default Overlay mode
        Assert.Equal(AvaloniaSplitViewDisplayMode.Overlay, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_Null_Value_Returns_UnsetValue()
    {
        var result = SplitViewDisplayModeConverter.Instance.Convert(
            null,
            typeof(AvaloniaSplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaProperty.UnsetValue, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_ConvertBack_Inline()
    {
        var result = SplitViewDisplayModeConverter.Instance.ConvertBack(
            AvaloniaSplitViewDisplayMode.Inline,
            typeof(SplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(SplitViewDisplayMode.Inline, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_ConvertBack_CompactInline()
    {
        var result = SplitViewDisplayModeConverter.Instance.ConvertBack(
            AvaloniaSplitViewDisplayMode.CompactInline,
            typeof(SplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(SplitViewDisplayMode.CompactInline, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_ConvertBack_Overlay()
    {
        var result = SplitViewDisplayModeConverter.Instance.ConvertBack(
            AvaloniaSplitViewDisplayMode.Overlay,
            typeof(SplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(SplitViewDisplayMode.Overlay, result);
    }

    [AvaloniaFact]
    public void SplitViewDisplayModeConverter_ConvertBack_CompactOverlay()
    {
        var result = SplitViewDisplayModeConverter.Instance.ConvertBack(
            AvaloniaSplitViewDisplayMode.CompactOverlay,
            typeof(SplitViewDisplayMode),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(SplitViewDisplayMode.CompactOverlay, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_Converts_Left()
    {
        var result = SplitViewPanePlacementConverter.Instance.Convert(
            SplitViewPanePlacement.Left,
            typeof(AvaloniaSplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaSplitViewPanePlacement.Left, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_Converts_Right()
    {
        var result = SplitViewPanePlacementConverter.Instance.Convert(
            SplitViewPanePlacement.Right,
            typeof(AvaloniaSplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaSplitViewPanePlacement.Right, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_Converts_Top()
    {
        var result = SplitViewPanePlacementConverter.Instance.Convert(
            SplitViewPanePlacement.Top,
            typeof(AvaloniaSplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaSplitViewPanePlacement.Top, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_Converts_Bottom()
    {
        var result = SplitViewPanePlacementConverter.Instance.Convert(
            SplitViewPanePlacement.Bottom,
            typeof(AvaloniaSplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaSplitViewPanePlacement.Bottom, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_Invalid_Value_Returns_Default()
    {
        var result = SplitViewPanePlacementConverter.Instance.Convert(
            (SplitViewPanePlacement)999,
            typeof(AvaloniaSplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        // Invalid enum values return the default Left placement
        Assert.Equal(AvaloniaSplitViewPanePlacement.Left, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_Null_Value_Returns_UnsetValue()
    {
        var result = SplitViewPanePlacementConverter.Instance.Convert(
            null,
            typeof(AvaloniaSplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(AvaloniaProperty.UnsetValue, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_ConvertBack_Left()
    {
        var result = SplitViewPanePlacementConverter.Instance.ConvertBack(
            AvaloniaSplitViewPanePlacement.Left,
            typeof(SplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(SplitViewPanePlacement.Left, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_ConvertBack_Right()
    {
        var result = SplitViewPanePlacementConverter.Instance.ConvertBack(
            AvaloniaSplitViewPanePlacement.Right,
            typeof(SplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(SplitViewPanePlacement.Right, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_ConvertBack_Top()
    {
        var result = SplitViewPanePlacementConverter.Instance.ConvertBack(
            AvaloniaSplitViewPanePlacement.Top,
            typeof(SplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(SplitViewPanePlacement.Top, result);
    }

    [AvaloniaFact]
    public void SplitViewPanePlacementConverter_ConvertBack_Bottom()
    {
        var result = SplitViewPanePlacementConverter.Instance.ConvertBack(
            AvaloniaSplitViewPanePlacement.Bottom,
            typeof(SplitViewPanePlacement),
            null,
            CultureInfo.InvariantCulture);
        
        Assert.Equal(SplitViewPanePlacement.Bottom, result);
    }
}
