using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class CapabilityPolicyFactoryTests
{
    private static IRootDock CreateRoot(Factory factory)
    {
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();
        return root;
    }

    [AvaloniaFact]
    public void CloseDockable_DoesNothing_When_RootPolicy_Disables_Close()
    {
        var factory = new Factory();
        var root = CreateRoot(factory);
        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();
        var document = factory.CreateDocument();

        factory.AddDockable(root, documentDock);
        factory.AddDockable(documentDock, document);

        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };

        factory.CloseDockable(document);

        Assert.Same(documentDock, document.Owner);
        Assert.Contains(document, documentDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CloseDockable_DockableOverride_Can_Allow_Close_When_Root_Disables()
    {
        var factory = new Factory();
        var root = CreateRoot(factory);
        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();
        var document = factory.CreateDocument();

        factory.AddDockable(root, documentDock);
        factory.AddDockable(documentDock, document);

        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };
        document.DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanClose = true
        };

        factory.CloseDockable(document);

        Assert.DoesNotContain(document, documentDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void DockAsDocument_DoesNothing_When_DockPolicy_Disables_DockAsDocument()
    {
        var factory = new Factory();
        var root = CreateRoot(factory);
        var toolDock = factory.CreateToolDock();
        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();
        var tool = factory.CreateTool();

        factory.AddDockable(root, toolDock);
        factory.AddDockable(root, documentDock);
        factory.AddDockable(toolDock, tool);

        toolDock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDockAsDocument = false
        };

        factory.DockAsDocument(tool);

        Assert.Same(toolDock, tool.Owner);
        Assert.DoesNotContain(tool, documentDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void FloatDockable_DoesNothing_When_DockPolicy_Disables_Float()
    {
        var factory = new Factory();
        var root = CreateRoot(factory);
        var toolDock = factory.CreateToolDock();
        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        var tool = factory.CreateTool();

        factory.AddDockable(root, toolDock);
        factory.AddDockable(toolDock, tool);

        toolDock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanFloat = false
        };

        factory.FloatDockable(tool);

        Assert.Same(toolDock, tool.Owner);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.Empty(root.Windows!);
    }

    [AvaloniaFact]
    public void PinDockable_DoesNothing_When_DockPolicy_Disables_Pin()
    {
        var factory = new Factory();
        var root = CreateRoot(factory);
        var toolDock = factory.CreateToolDock();
        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        toolDock.Alignment = Alignment.Left;
        var tool = factory.CreateTool();

        factory.AddDockable(root, toolDock);
        factory.AddDockable(toolDock, tool);

        toolDock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanPin = false
        };

        factory.PinDockable(tool);

        Assert.Same(toolDock, tool.Owner);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.Empty(root.LeftPinnedDockables!);
        Assert.Empty(root.RightPinnedDockables!);
        Assert.Empty(root.TopPinnedDockables!);
        Assert.Empty(root.BottomPinnedDockables!);
    }

    [AvaloniaFact]
    public void FloatAllDockables_DoesNothing_When_DockPolicy_Disables_Float()
    {
        var factory = new Factory();
        var root = CreateRoot(factory);
        var toolDock = factory.CreateToolDock();
        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        var firstTool = factory.CreateTool();
        var secondTool = factory.CreateTool();

        factory.AddDockable(root, toolDock);
        factory.AddDockable(toolDock, firstTool);
        factory.AddDockable(toolDock, secondTool);

        toolDock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanFloat = false
        };

        factory.FloatAllDockables(firstTool);

        Assert.Contains(firstTool, toolDock.VisibleDockables!);
        Assert.Contains(secondTool, toolDock.VisibleDockables!);
        Assert.Empty(root.Windows!);
    }
}
