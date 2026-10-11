using AvalonDockApiAudit;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ApiKeyTests
{
    [Theory]
    [InlineData("object[]")]
    [InlineData("object []")]
    public void Array_Rank_Spacing_Does_Not_Change_The_Key(string type)
    {
        Assert.Equal("AvalonDock.Probe|method:Convert(object[])", ApiKey.Normalize($"AvalonDock.Probe|method:Convert({type})"));
    }

    [Theory]
    [InlineData("CultureInfo")]
    [InlineData("System.Globalization.CultureInfo")]
    [InlineData("global::System.Globalization.CultureInfo")]
    public void Imported_And_Qualified_Bcl_CultureInfo_Have_The_Same_Key(string type)
    {
        string declaration = $"Xceed.Wpf.AvalonDock.Converters.Probe|method:Convert(object,{type})\tignored signature";
        Assert.Equal("AvalonDock.Converters.Probe|method:Convert(object,CultureInfo)", ApiKey.Normalize(declaration));
    }

    [Theory]
    [InlineData("Other.CultureInfo")]
    [InlineData("Other.System.Globalization.CultureInfo")]
    [InlineData("System.Globalization.CultureInfoExtra")]
    [InlineData("System.Globalization.CultureInfo.Nested")]
    [InlineData("DependencyPropertyChangedEventArgs")]
    public void Unrelated_Or_Framework_Types_Are_Not_Rewritten(string type)
    {
        string key = $"AvalonDock.Probe|method:Convert({type})";
        Assert.Equal(key, ApiKey.Normalize(key));
    }
}
