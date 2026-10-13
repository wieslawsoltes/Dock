using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class GroupMinimumContractTests
{
    [AvaloniaTheory]
    [InlineData("document", false)]
    [InlineData("document", true)]
    [InlineData("tool", false)]
    [InlineData("tool", true)]
    [InlineData("documents", false)]
    [InlineData("documents", true)]
    [InlineData("tools", false)]
    [InlineData("tools", true)]
    [InlineData("panel", false)]
    [InlineData("panel", true)]
    public void Public_And_Native_Changes_Publish_Only_The_Compatibility_Minimum_Name(string kind, bool width)
    {
        Minimum minimum = Create(kind, width);
        List<(string, double)> events = Observe(minimum);
        Assert.Equal(25, minimum.Read());

        minimum.Write(40);
        minimum.Write(40);
        if (width) minimum.Element.DockModel.MinWidth = 60;
        else minimum.Element.DockModel.MinHeight = 60;

        Assert.Equal(new[] { ($"before:{minimum.Name}", 25d), ($"after:{minimum.Name}", 40d), ($"after:{minimum.Name}", 60d) }, events);
        Assert.Equal(60, minimum.Read());
    }

    [AvaloniaTheory]
    [InlineData("document", false)]
    [InlineData("document", true)]
    [InlineData("tool", false)]
    [InlineData("tool", true)]
    [InlineData("documents", false)]
    [InlineData("documents", true)]
    [InlineData("tools", false)]
    [InlineData("tools", true)]
    [InlineData("panel", false)]
    [InlineData("panel", true)]
    public void Repeated_NaN_Assignments_Raise_Both_Compatibility_Notifications(string kind, bool width)
    {
        Minimum minimum = Create(kind, width);
        List<(string, double)> events = Observe(minimum);
        minimum.Write(double.NaN);
        minimum.Write(double.NaN);
        Assert.Equal(new[]
        {
            ($"before:{minimum.Name}", 25d), ($"after:{minimum.Name}", double.NaN),
            ($"before:{minimum.Name}", double.NaN), ($"after:{minimum.Name}", double.NaN)
        }, events);
        Assert.True(double.IsNaN(width ? minimum.Element.DockModel.MinWidth : minimum.Element.DockModel.MinHeight));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Equality_Precedes_Validation_For_A_Negative_Minimum_Restored_From_Xml(bool width)
    {
        Minimum minimum = Create("panel", width);
        Restore(minimum, -12);
        List<(string, double)> events = Observe(minimum);
        minimum.Write(-12);
        Assert.Empty(events);
        ArgumentException error = Assert.Throws<ArgumentException>(() => minimum.Write(-13));
        Assert.Null(error.ParamName);
        Assert.Equal(-12, minimum.Read());
        Assert.Empty(events);
        minimum.Write(0);
        Assert.Equal(new[] { ($"before:{minimum.Name}", -12d), ($"after:{minimum.Name}", 0d) }, events);
    }

    [AvaloniaTheory]
    [InlineData(false, -1d)]
    [InlineData(true, -1d)]
    [InlineData(false, double.NegativeInfinity)]
    [InlineData(true, double.NegativeInfinity)]
    public void Invalid_Minimum_Fails_Before_Changing_And_Preserves_The_Upstream_Argument_Error(bool width, double value)
    {
        Minimum minimum = Create("panel", width);
        List<(string, double)> events = Observe(minimum);
        ArgumentException error = Assert.Throws<ArgumentException>(() => minimum.Write(value));
        Assert.Null(error.ParamName);
        Assert.Equal("Invalid value, must be a positive number or equal to zero", error.Message);
        Assert.Empty(events);
        Assert.Equal(25, minimum.Read());
        minimum.Write(double.PositiveInfinity);
        Assert.Equal(double.PositiveInfinity, minimum.Read());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Changing_Callback_That_Writes_The_Requested_Value_Does_Not_Suppress_The_Outer_Changed_Notification(bool width)
    {
        Minimum minimum = Create("panel", width);
        List<(string, double)> events = Observe(minimum);
        bool nested = false;
        minimum.Element.PropertyChanging += (_, _) =>
        {
            if (nested) return;
            nested = true;
            minimum.Write(80);
        };
        minimum.Write(80);
        Assert.Equal(new[]
        {
            ($"before:{minimum.Name}", 25d), ($"before:{minimum.Name}", 25d),
            ($"after:{minimum.Name}", 80d), ($"after:{minimum.Name}", 80d)
        }, events);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Changed_Callback_Can_Replace_The_Minimum_Without_A_Second_Outer_Notification(bool width)
    {
        Minimum minimum = Create("panel", width);
        List<(string, double)> events = Observe(minimum);
        minimum.Element.PropertyChanged += (_, e) => { if (e.PropertyName == minimum.Name && minimum.Read() == 80) minimum.Write(120); };
        minimum.Write(80);
        Assert.Equal(new[]
        {
            ($"before:{minimum.Name}", 25d), ($"after:{minimum.Name}", 80d),
            ($"before:{minimum.Name}", 80d), ($"after:{minimum.Name}", 120d)
        }, events);
        Assert.Equal(120, minimum.Read());
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Observer_Errors_Stop_At_Their_Phase_And_Do_Not_Disable_Later_Notifications(bool width, bool before)
    {
        Minimum minimum = Create("panel", width);
        InvalidOperationException error = new("minimum observer");
        bool fail = true;
        minimum.Element.PropertyChanging += (_, _) => { if (before && fail) throw error; };
        minimum.Element.PropertyChanged += (_, e) => { if (!before && fail && e.PropertyName == minimum.Name) throw error; };
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => minimum.Write(80)));
        Assert.Equal(before ? 25 : 80, minimum.Read());
        fail = false;
        List<(string, double)> events = Observe(minimum);
        minimum.Write(90);
        Assert.Equal(new[] { ($"before:{minimum.Name}", before ? 25d : 80d), ($"after:{minimum.Name}", 90d) }, events);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Native_Xml_Observer_Can_Publicly_Reassign_NaN_Without_Losing_The_Public_Notifications(bool width)
    {
        Minimum minimum = Create("panel", width);
        ((LayoutPanel)minimum.Element).ComputeVisibility();
        List<(string, double)> events = Observe(minimum);
        minimum.Element.DockModel.PropertyChanged += (_, e) =>
        {
            if (e.Property == (width ? DockableBase.MinWidthProperty : DockableBase.MinHeightProperty)) minimum.Write(double.NaN);
        };
        Restore(minimum, double.NaN);
        Assert.Equal(new[] { ($"before:{minimum.Name}", double.NaN), ($"after:{minimum.Name}", double.NaN) }, events);
    }

    private static List<(string, double)> Observe(Minimum minimum)
    {
        List<(string, double)> events = new();
        minimum.Element.PropertyChanging += (_, e) => events.Add(($"before:{e.PropertyName}", minimum.Read()));
        minimum.Element.PropertyChanged += (_, e) => events.Add(($"after:{e.PropertyName}", minimum.Read()));
        return events;
    }

    private static void Restore(Minimum minimum, double value)
    {
        using XmlReader reader = XmlReader.Create(new StringReader($"<Group {minimum.Name}='{XmlConvert.ToString(value)}' />"));
        reader.MoveToContent();
        ((IXmlSerializable)minimum.Element).ReadXml(reader);
    }

    private static Minimum Create(string kind, bool width) => kind switch
    {
        "document" => From(new LayoutDocumentPane(), width),
        "tool" => From(new LayoutAnchorablePane(), width),
        "documents" => From(new LayoutDocumentPaneGroup(), width),
        "tools" => From(new LayoutAnchorablePaneGroup(), width),
        _ => From(new LayoutPanel(), width)
    };

    private static Minimum From<T>(LayoutPositionableGroup<T> group, bool width) where T : class, ILayoutElement =>
        new(group, width ? nameof(group.DockMinWidth) : nameof(group.DockMinHeight),
            () => width ? group.DockMinWidth : group.DockMinHeight,
            value => { if (width) group.DockMinWidth = value; else group.DockMinHeight = value; });

    private sealed record Minimum(LayoutElement Element, string Name, Func<double> Read, Action<double> Write);
}
