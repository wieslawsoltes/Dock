using System.IO;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SerializationReturnLocationTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void WriteXml_Uses_The_Public_Return_Container_Instead_Of_Native_Float_History(bool tool, bool hasPrevious)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutDocumentPane nativePrevious = new();
        LayoutDocumentPane publicPrevious = new();
        content.DockModel.OriginalOwner = nativePrevious.DockModel;
        content.PreviousContainer = hasPrevious ? publicPrevious : null;
        content.PreviousContainerIndex = 3;
        using StringWriter text = new();
        using (XmlWriter writer = XmlWriter.Create(text))
        {
            writer.WriteStartElement("Content");
            content.WriteXml(writer);
            writer.WriteEndElement();
        }
        XElement xml = XElement.Parse(text.ToString());
        Assert.Equal(hasPrevious ? publicPrevious.DockModel.Id : null, (string?)xml.Attribute("PreviousContainerId"));
        Assert.Equal(hasPrevious ? "3" : null, (string?)xml.Attribute("PreviousContainerIndex"));
        Assert.Same(nativePrevious.DockModel, content.DockModel.OriginalOwner);
    }
}
