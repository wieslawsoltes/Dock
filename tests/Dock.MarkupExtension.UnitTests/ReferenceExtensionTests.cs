using System;
using Avalonia.Controls;
using Avalonia.MarkupExtension;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Dock.MarkupExtension.UnitTests;

public class ReferenceExtensionTests
{
    [AvaloniaFact]
    public void Constructor_Sets_Name()
    {
        var ext = new ReferenceExtension("myElement");
        Assert.Equal("myElement", ext.Name);
    }

    [AvaloniaFact]
    public void ProvideValue_Returns_Referenced_Object()
    {
        var button = new Button();
        var scope = new NameScope();
        scope.Register("btn", button);
        var ext = new ReferenceExtension("btn");
        var sp = new TestServiceProvider(null, scope);
        var value = ext.ProvideValue(sp);
        Assert.Same(button, value);
    }

    [AvaloniaFact]
    public void ProvideValue_No_Scope_Returns_Null()
    {
        var ext = new ReferenceExtension("missing");
        var sp = new TestServiceProvider(null, null);
        var value = ext.ProvideValue(sp);
        Assert.Null(value);
    }

    [AvaloniaFact]
    public void ProvideValue_Missing_Name_Returns_Null()
    {
        var scope = new NameScope();
        var ext = new ReferenceExtension("unknown");
        var sp = new TestServiceProvider(null, scope);
        var value = ext.ProvideValue(sp);
        Assert.Null(value);
    }
}
