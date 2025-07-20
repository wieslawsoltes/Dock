using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowDocumentCloseTests
{
    private class ClosingDocument : Document
    {
        public bool Closed { get; private set; }

        public override bool OnClose()
        {
            Closed = true;
            return base.OnClose();
        }
    }

    [AvaloniaFact]
    public void Closing_HostWindow_Closes_Document()
    {
        var factory = new Factory
        {
            DefaultHostWindowLocator = () => new HostWindow()
        };
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Windows = factory.CreateList<IDockWindow>()
        };
        root.Factory = factory;

        var docDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, docDock);
        docDock.Factory = factory;
        root.ActiveDockable = docDock;

        var doc = new ClosingDocument();
        factory.AddDockable(docDock, doc);
        docDock.ActiveDockable = doc;

        factory.SplitToWindow(docDock, doc, 0, 0, 100, 100);

        var window = Assert.IsType<DockWindow>(root.Windows![0]);
        Assert.IsType<HostWindow>(window.Host);
        ((HostWindow)window.Host!).Exit();

        Assert.True(doc.Closed);
    }
}
