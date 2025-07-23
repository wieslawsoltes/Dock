using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.MarkupExtension;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Dock.MarkupExtension.UnitTests;

public class LoadExtensionTests
{
    [AvaloniaFact]
    public void Constructor_Sets_Source()
    {
        var uri = new Uri("avares://Dock.MarkupExtension.UnitTests/Views/TestView.axaml");
        var ext = new LoadExtension(uri);
        Assert.Equal(uri, ext.Source);
    }

    [AvaloniaFact]
    public void ProvideValue_Loads_Object_From_Uri()
    {
        var uri = new Uri("avares://Dock.MarkupExtension.UnitTests/Views/TestView.axaml");
        var ext = new LoadExtension(uri);
        var context = new UriContext { BaseUri = new Uri("avares://Dock.MarkupExtension.UnitTests/") };
        var sp = new TestServiceProvider(context, null);
        var value = ext.ProvideValue(sp);
        Assert.IsType<TextBlock>(value);
    }

    [AvaloniaFact]
    public void ProvideValue_No_Context_Returns_Null()
    {
        var uri = new Uri("avares://Dock.MarkupExtension.UnitTests/Views/TestView.axaml");
        var ext = new LoadExtension(uri);
        var sp = new TestServiceProvider(null, null);
        var value = ext.ProvideValue(sp);
        Assert.Null(value);
    }

    [AvaloniaFact]
    public void ProvideValue_No_Source_Returns_Null()
    {
        var ext = new LoadExtension();
        var context = new UriContext { BaseUri = new Uri("avares://Dock.MarkupExtension.UnitTests/") };
        var sp = new TestServiceProvider(context, null);
        var value = ext.ProvideValue(sp);
        Assert.Null(value);
    }
}

internal class UriContext : IUriContext
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Uri BaseUri { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
