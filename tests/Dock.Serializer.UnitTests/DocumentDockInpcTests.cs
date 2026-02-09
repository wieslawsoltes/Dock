using Dock.Model.Inpc.Controls;
using Xunit;

namespace Dock.Serializer.UnitTests;

public class DocumentDockInpcTests
{
    [Fact]
    public void DocumentDock_Default_EmptyContent_Is_Set()
    {
        var dock = new DocumentDock();

        Assert.Equal("No documents open", dock.EmptyContent);
    }
}
