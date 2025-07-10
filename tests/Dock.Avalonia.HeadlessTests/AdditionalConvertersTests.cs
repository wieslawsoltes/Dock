using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using AC = Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Converters;
using Dock.Model.Core;
using Dock.Model.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class AdditionalConvertersTests
{
    [AvaloniaFact]
    public void AlignmentConverter_Converts_Both_Ways()
    {
        Assert.Equal(AC.Dock.Left, AlignmentConverter.Instance.Convert(Alignment.Left, typeof(AC.Dock), null, CultureInfo.InvariantCulture));
        Assert.Equal(AC.Dock.Right, AlignmentConverter.Instance.Convert(Alignment.Right, typeof(AC.Dock), null, CultureInfo.InvariantCulture));
        Assert.Equal(AC.Dock.Top, AlignmentConverter.Instance.Convert(Alignment.Top, typeof(AC.Dock), null, CultureInfo.InvariantCulture));
        Assert.Equal(AC.Dock.Bottom, AlignmentConverter.Instance.Convert(Alignment.Bottom, typeof(AC.Dock), null, CultureInfo.InvariantCulture));
        Assert.Equal(AvaloniaProperty.UnsetValue, AlignmentConverter.Instance.Convert(Alignment.Unset, typeof(AC.Dock), null, CultureInfo.InvariantCulture));

        Assert.Equal(Alignment.Left, AlignmentConverter.Instance.ConvertBack(AC.Dock.Left, typeof(Alignment), null, CultureInfo.InvariantCulture));
        Assert.Equal(Alignment.Right, AlignmentConverter.Instance.ConvertBack(AC.Dock.Right, typeof(Alignment), null, CultureInfo.InvariantCulture));
        Assert.Equal(Alignment.Top, AlignmentConverter.Instance.ConvertBack(AC.Dock.Top, typeof(Alignment), null, CultureInfo.InvariantCulture));
        Assert.Equal(Alignment.Bottom, AlignmentConverter.Instance.ConvertBack(AC.Dock.Bottom, typeof(Alignment), null, CultureInfo.InvariantCulture));
        Assert.Equal(Alignment.Unset, AlignmentConverter.Instance.ConvertBack((AC.Dock)42, typeof(Alignment), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaFact]
    public void DockModeConverter_Converts_Both_Ways()
    {
        Assert.Equal(AC.Dock.Left, DockModeConverter.Instance.Convert(DockMode.Left, typeof(AC.Dock), null, CultureInfo.InvariantCulture));
        Assert.Equal(AC.Dock.Right, DockModeConverter.Instance.Convert(DockMode.Right, typeof(AC.Dock), null, CultureInfo.InvariantCulture));
        Assert.Equal(AC.Dock.Top, DockModeConverter.Instance.Convert(DockMode.Top, typeof(AC.Dock), null, CultureInfo.InvariantCulture));
        Assert.Equal(AC.Dock.Bottom, DockModeConverter.Instance.Convert(DockMode.Bottom, typeof(AC.Dock), null, CultureInfo.InvariantCulture));
        Assert.Equal(AvaloniaProperty.UnsetValue, DockModeConverter.Instance.Convert(null, typeof(AC.Dock), null, CultureInfo.InvariantCulture));

        Assert.Equal(DockMode.Left, DockModeConverter.Instance.ConvertBack(AC.Dock.Left, typeof(DockMode), null, CultureInfo.InvariantCulture));
        Assert.Equal(DockMode.Right, DockModeConverter.Instance.ConvertBack(AC.Dock.Right, typeof(DockMode), null, CultureInfo.InvariantCulture));
        Assert.Equal(DockMode.Top, DockModeConverter.Instance.ConvertBack(AC.Dock.Top, typeof(DockMode), null, CultureInfo.InvariantCulture));
        Assert.Equal(DockMode.Bottom, DockModeConverter.Instance.ConvertBack(AC.Dock.Bottom, typeof(DockMode), null, CultureInfo.InvariantCulture));
        Assert.Equal(DockMode.Center, DockModeConverter.Instance.ConvertBack((AC.Dock)42, typeof(DockMode), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaFact]
    public void IsMaximizedConverter_Works_As_Expected()
    {
        Assert.True((bool)IsMaximizedConverter.Instance.Convert(WindowState.Maximized, typeof(bool), null, CultureInfo.InvariantCulture)!);
        Assert.False((bool)IsMaximizedConverter.Instance.Convert(WindowState.Normal, typeof(bool), null, CultureInfo.InvariantCulture)!);
        Assert.False((bool)IsMaximizedConverter.Instance.Convert("invalid", typeof(bool), null, CultureInfo.InvariantCulture)!);

        Assert.Throws<NotImplementedException>(() => IsMaximizedConverter.Instance.ConvertBack(true, typeof(WindowState), null, CultureInfo.InvariantCulture));
    }


    [AvaloniaFact]
    public void OwnerIsToolDockConverter_Evaluates_Type()
    {
        var dock = new Dock.Model.Avalonia.Controls.ToolDock();
        Assert.True((bool)OwnerIsToolDockConverter.Instance.Convert(dock, typeof(bool), null, CultureInfo.InvariantCulture)!);
        Assert.False((bool)OwnerIsToolDockConverter.Instance.Convert(new object(), typeof(bool), null, CultureInfo.InvariantCulture)!);
        Assert.Throws<NotImplementedException>(() => OwnerIsToolDockConverter.Instance.ConvertBack(true, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaFact]
    public void EitherNotNullConverter_Returns_First_Non_Null()
    {
        var values = new object?[] { null, 5, "test" };
        var result = EitherNotNullConverter.Instance.Convert(values, typeof(object), null, CultureInfo.InvariantCulture);
        Assert.Equal(5, result);

        var valuesAllNull = new object?[] { null, null };
        var resultAllNull = EitherNotNullConverter.Instance.Convert(valuesAllNull, typeof(object), null, CultureInfo.InvariantCulture) as IList<object?>;
        Assert.Equal(valuesAllNull, resultAllNull);
    }
}
