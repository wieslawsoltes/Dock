using System;
using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AutoHideSizePropertyTests
{
    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Size_Assignments_Clamp_And_Notify_Around_The_Native_Bounds_Update(bool width)
    {
        LayoutAnchorable tool = new();
        string property = width ? nameof(tool.AutoHideWidth) : nameof(tool.AutoHideHeight);
        double Read() => width ? tool.AutoHideWidth : tool.AutoHideHeight;
        void Write(double value) { if (width) tool.AutoHideWidth = value; else tool.AutoHideHeight = value; }
        List<(string, double)> events = new();
        tool.PropertyChanging += (_, args) => { if (args.PropertyName == property) events.Add(("before", Read())); };
        tool.PropertyChanged += (_, args) => { if (args.PropertyName == property) events.Add(("after", Read())); };
        Assert.Equal(0, Read());
        Write(0);
        Assert.Empty(events);
        Write(40);
        Assert.Equal(new[] { ("before", 0d), ("after", 100d) }, events);
        events.Clear(); Write(100);
        Assert.Empty(events);
        Write(-1);
        Assert.Equal(new[] { ("before", 100d), ("after", 100d) }, events);
        if (width) tool.AutoHideMinWidth = 180; else tool.AutoHideMinHeight = 180;
        Assert.Equal(100, Read());
        events.Clear(); Write(120);
        Assert.Equal(new[] { ("before", 100d), ("after", 180d) }, events);
        tool.DockModel.GetPinnedBounds(out _, out _, out double nativeWidth, out double nativeHeight);
        Assert.Equal(180, width ? nativeWidth : nativeHeight);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Negative_Minima_Raise_Changing_Before_Rejecting_The_Value(bool width)
    {
        LayoutAnchorable tool = new();
        string property = width ? nameof(tool.AutoHideMinWidth) : nameof(tool.AutoHideMinHeight);
        List<string> events = new();
        tool.PropertyChanging += (_, args) => { if (args.PropertyName == property) events.Add("before"); };
        tool.PropertyChanged += (_, args) => { if (args.PropertyName == property) events.Add("after"); };
        Assert.Throws<ArgumentException>(() => { if (width) tool.AutoHideMinWidth = -1; else tool.AutoHideMinHeight = -1; });
        Assert.Equal(new[] { "before" }, events);
        Assert.Equal(100, width ? tool.AutoHideMinWidth : tool.AutoHideMinHeight);
        events.Clear();
        if (width) tool.AutoHideMinWidth = 100; else tool.AutoHideMinHeight = 100;
        Assert.Empty(events);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Repeated_NaN_Assignments_Keep_Upstream_Comparison_And_Notification_Semantics(bool width)
    {
        LayoutAnchorable tool = new();
        int changing = 0, changed = 0;
        tool.PropertyChanging += (_, _) => changing++;
        tool.PropertyChanged += (_, _) => changed++;
        for (int i = 0; i < 2; i++)
        {
            if (width) { tool.AutoHideMinWidth = double.NaN; tool.AutoHideWidth = double.NaN; }
            else { tool.AutoHideMinHeight = double.NaN; tool.AutoHideHeight = double.NaN; }
        }
        Assert.Equal(4, changing);
        Assert.Equal(4, changed);
        Assert.True(double.IsNaN(width ? tool.AutoHideWidth : tool.AutoHideHeight));
        Assert.True(double.IsNaN(width ? tool.AutoHideMinWidth : tool.AutoHideMinHeight));
    }
}
