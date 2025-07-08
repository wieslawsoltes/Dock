using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentDockCommandTests
{
    [AvaloniaFact]
    public void CreateDocumentCommand_Creates_Document()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = true,
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };
        factory.AddDockable(root, dock);

        dock.CreateDocument!.Execute(null);

        Assert.Single(dock.VisibleDockables!);
        Assert.IsType<Document>(dock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void CreateDocumentCommand_DoesNothing_When_Disabled()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = false,
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };
        factory.AddDockable(root, dock);

        dock.CreateDocument!.Execute(null);

        Assert.Empty(dock.VisibleDockables!);
    }
}
