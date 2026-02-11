using System.Collections.Generic;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockCapabilityResolverTests
{
    private static (RootDock Root, ToolDock Dock, Tool Tool) CreateHierarchy()
    {
        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = new List<IDockable>()
        };

        var dock = new ToolDock
        {
            Id = "Tools",
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };

        var tool = new Tool
        {
            Id = "Tool",
            Owner = dock
        };

        root.VisibleDockables!.Add(dock);
        dock.VisibleDockables!.Add(tool);
        root.ActiveDockable = dock;
        dock.ActiveDockable = tool;

        return (root, dock, tool);
    }

    private static (Factory Factory, RootDock Root, ToolDock Dock, Tool Tool) CreateFactoryHierarchy()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Id = "Root";
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var dock = factory.CreateToolDock();
        dock.Id = "Tools";
        dock.VisibleDockables = factory.CreateList<IDockable>();

        var tool = factory.CreateTool();
        tool.Id = "Tool";

        factory.AddDockable(root, dock);
        factory.AddDockable(dock, tool);
        root.ActiveDockable = dock;
        dock.ActiveDockable = tool;

        return (factory, (RootDock)root, (ToolDock)dock, (Tool)tool);
    }

    [Fact]
    public void Evaluate_Uses_Dockable_Value_When_No_Policies()
    {
        var (_, dock, tool) = CreateHierarchy();
        tool.CanClose = false;

        var evaluation = DockCapabilityResolver.Evaluate(tool, DockCapability.Close, dock);

        Assert.False(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.Dockable, evaluation.EffectiveSource);
        Assert.Null(evaluation.RootPolicyValue);
        Assert.Null(evaluation.DockPolicyValue);
        Assert.Null(evaluation.DockableOverrideValue);
    }

    [Fact]
    public void Evaluate_Uses_Root_Policy_When_Set()
    {
        var (root, dock, tool) = CreateHierarchy();
        tool.CanClose = true;
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };

        var evaluation = DockCapabilityResolver.Evaluate(tool, DockCapability.Close, dock);

        Assert.False(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.RootPolicy, evaluation.EffectiveSource);
        Assert.False(evaluation.RootPolicyValue);
    }

    [Fact]
    public void Evaluate_Uses_Dock_Policy_Over_Root()
    {
        var (root, dock, tool) = CreateHierarchy();
        tool.CanClose = true;
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };
        dock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = true
        };

        var evaluation = DockCapabilityResolver.Evaluate(tool, DockCapability.Close, dock);

        Assert.True(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.DockPolicy, evaluation.EffectiveSource);
        Assert.False(evaluation.RootPolicyValue);
        Assert.True(evaluation.DockPolicyValue);
    }

    [Fact]
    public void Evaluate_Uses_Dockable_Override_Over_Dock_And_Root()
    {
        var (root, dock, tool) = CreateHierarchy();
        tool.CanClose = true;
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };
        dock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = true
        };
        tool.DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanClose = false
        };

        var evaluation = DockCapabilityResolver.Evaluate(tool, DockCapability.Close, dock);

        Assert.False(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.DockableOverride, evaluation.EffectiveSource);
        Assert.False(evaluation.DockableOverrideValue);
    }

    [Fact]
    public void Evaluate_Resolves_Root_From_Owner_Hierarchy()
    {
        var (root, _, tool) = CreateHierarchy();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanFloat = false
        };

        var evaluation = DockCapabilityResolver.Evaluate(tool, DockCapability.Float);

        Assert.False(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.RootPolicy, evaluation.EffectiveSource);
        Assert.Contains("blocked by root capability policy", evaluation.DiagnosticMessage);
    }

    [Fact]
    public void Evaluate_Resolves_Root_From_Factory_When_Available()
    {
        var (_, root, _, tool) = CreateFactoryHierarchy();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };

        var evaluation = DockCapabilityResolver.Evaluate(tool, DockCapability.Close);

        Assert.False(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.RootPolicy, evaluation.EffectiveSource);
    }

    [Fact]
    public void Evaluate_Resolves_Root_From_DockContext_Factory_When_Dockable_Has_No_Factory()
    {
        var (_, root, dock, _) = CreateFactoryHierarchy();
        var detachedTool = new Tool
        {
            Id = "DetachedTool"
        };

        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrag = false
        };

        var evaluation = DockCapabilityResolver.Evaluate(detachedTool, DockCapability.Drag, dock);

        Assert.False(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.RootPolicy, evaluation.EffectiveSource);
    }

    [Fact]
    public void Evaluate_Resolves_Root_From_DockContext_Owner_Hierarchy_When_Factory_Is_Missing()
    {
        var root = new RootDock();
        var dock = new ToolDock
        {
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };
        var detachedTool = new Tool
        {
            CanDrop = true
        };

        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrop = false
        };

        var evaluation = DockCapabilityResolver.Evaluate(detachedTool, DockCapability.Drop, dock);

        Assert.False(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.RootPolicy, evaluation.EffectiveSource);
    }

    [Fact]
    public void Evaluate_Uses_Dockable_Default_For_Unknown_Capability()
    {
        var (_, dock, tool) = CreateHierarchy();
        var evaluation = DockCapabilityResolver.Evaluate(tool, (DockCapability)777, dock);

        Assert.False(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.Dockable, evaluation.EffectiveSource);
        Assert.Contains("blocked by dockable flag", evaluation.DiagnosticMessage);
    }

    [Fact]
    public void ResolveOperationDock_Returns_Dock_When_Dockable_Is_Dock()
    {
        var dock = new ToolDock();
        var result = DockCapabilityResolver.ResolveOperationDock(dock);
        Assert.Same(dock, result);
    }

    [Fact]
    public void ResolveOperationDock_Returns_Owner_Dock_When_Dockable_Is_Not_Dock()
    {
        var dock = new ToolDock();
        var tool = new Tool
        {
            Owner = dock
        };

        var result = DockCapabilityResolver.ResolveOperationDock(tool);

        Assert.Same(dock, result);
    }

    [Fact]
    public void ResolveOperationDock_Returns_Null_When_No_Dock_Context()
    {
        var tool = new Tool();

        var result = DockCapabilityResolver.ResolveOperationDock(tool);

        Assert.Null(result);
    }

    [Fact]
    public void ResolveDropTargetDock_Returns_Dock_When_Target_Is_Dock()
    {
        var dock = new DocumentDock();
        var result = DockCapabilityResolver.ResolveDropTargetDock(dock);
        Assert.Same(dock, result);
    }

    [Fact]
    public void ResolveDropTargetDock_Returns_Owner_Dock_When_Target_Is_Not_Dock()
    {
        var dock = new DocumentDock();
        var doc = new Document
        {
            Owner = dock
        };

        var result = DockCapabilityResolver.ResolveDropTargetDock(doc);

        Assert.Same(dock, result);
    }

    [Fact]
    public void ResolveDropTargetDock_Returns_Null_When_No_Dock_Context()
    {
        var doc = new Document();

        var result = DockCapabilityResolver.ResolveDropTargetDock(doc);

        Assert.Null(result);
    }

    [Fact]
    public void IsEnabled_Returns_Effective_Value()
    {
        var (root, dock, tool) = CreateHierarchy();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanFloat = false
        };

        var result = DockCapabilityResolver.IsEnabled(tool, DockCapability.Float, dock);

        Assert.False(result);
    }

    [Fact]
    public void Evaluate_Returns_Dockable_Base_Value_When_Root_Cannot_Be_Resolved()
    {
        var tool = new Tool
        {
            CanClose = true
        };

        var evaluation = DockCapabilityResolver.Evaluate(tool, DockCapability.Close);

        Assert.True(evaluation.EffectiveValue);
        Assert.Equal(DockCapabilityValueSource.Dockable, evaluation.EffectiveSource);
        Assert.Null(evaluation.RootPolicyValue);
        Assert.Null(evaluation.DockPolicyValue);
        Assert.Null(evaluation.DockableOverrideValue);
    }
}
