using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.UnitTests;

public class DocumentDockFluentExtensionsTests
{
    [Fact]
    public void WithEmptyContent_Sets_Value_And_Returns_Dock()
    {
        var factory = new Factory();
        var dock = factory.CreateDocumentDock();

        var returned = dock.WithEmptyContent("Nothing to show");

        Assert.Same(dock, returned);
        Assert.Equal("Nothing to show", dock.EmptyContent);
    }
}
