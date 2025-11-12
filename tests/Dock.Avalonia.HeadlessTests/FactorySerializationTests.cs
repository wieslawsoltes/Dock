using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Json;
using Dock.Model.Controls;
using Dock.Model.Core;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>
/// Tests for serialization and deserialization of layouts with hidden dockables
/// </summary>
public class FactorySerializationTests
{
    [AvaloniaFact]
    public void HiddenDockable_SerializeDeserialize_PreservesOriginalOwner()
    {
        // Arrange
        var factory = new Factory();
        var root = new RootDock 
        { 
            Id = "root",
            HiddenDockables = factory.CreateList<IDockable>(),
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        
        var dock = new ProportionalDock 
        { 
            Id = "dock",
            VisibleDockables = factory.CreateList<IDockable>() 
        };
        dock.Factory = factory;
        
        var doc1 = new Document { Title = "Doc1", Id = "doc1" };
        var doc2 = new Document { Title = "Doc2", Id = "doc2" };
        
        // Add dockables to the dock
        factory.AddDockable(root, dock);
        factory.AddDockable(dock, doc1);
        factory.AddDockable(dock, doc2);
        
        // Hide doc1
        factory.HideDockable(doc1);
        
        // Verify doc1 is hidden and OriginalOwner is set before serialization
        Assert.Single(root.HiddenDockables!);
        Assert.Equal(doc1, root.HiddenDockables[0]);
        Assert.Equal(dock, doc1.OriginalOwner);
        Assert.Equal(root, doc1.Owner);
        
        // Act - Serialize and deserialize
        var serializer = new AvaloniaDockSerializer();
        var json = serializer.Serialize(root);
        
        // Debug: Print JSON to see what's being serialized
        // System.Console.WriteLine(json);
        
        var deserializedRoot = serializer.Deserialize<RootDock>(json);
        
        // Assert - Verify the structure is preserved
        Assert.NotNull(deserializedRoot);
        Assert.NotNull(deserializedRoot!.HiddenDockables);
        Assert.Single(deserializedRoot.HiddenDockables!);
        
        var deserializedDoc1 = deserializedRoot.HiddenDockables[0];
        Assert.Equal("doc1", deserializedDoc1.Id);
        
        // The critical assertion: OriginalOwner should be preserved
        Assert.NotNull(deserializedDoc1.OriginalOwner);
        Assert.Equal("dock", deserializedDoc1.OriginalOwner!.Id);
        
        // Verify we can restore the dockable
        deserializedRoot.Factory = factory;
        var deserializedDock = deserializedRoot.VisibleDockables!.First() as IDock;
        Assert.NotNull(deserializedDock);
        deserializedDock!.Factory = factory;
        
        factory.RestoreDockable(deserializedDoc1);
        
        // After restoration, the dockable should be back in its original owner
        Assert.Equal(deserializedDock, deserializedDoc1.Owner);
        Assert.Contains(deserializedDoc1, deserializedDock.VisibleDockables!);
        Assert.Empty(deserializedRoot.HiddenDockables);
    }
    
    [AvaloniaFact]
    public void HiddenDockable_WithNullOriginalOwner_SerializeDeserialize()
    {
        // Arrange
        var factory = new Factory();
        var root = new RootDock 
        { 
            Id = "root",
            HiddenDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        
        var doc = new Document { Title = "Doc", Id = "doc" };
        doc.Owner = root;
        doc.OriginalOwner = null;
        root.HiddenDockables.Add(doc);
        
        // Act
        var serializer = new AvaloniaDockSerializer();
        var json = serializer.Serialize(root);
        var deserializedRoot = serializer.Deserialize<RootDock>(json);
        
        // Assert
        Assert.NotNull(deserializedRoot);
        Assert.Single(deserializedRoot!.HiddenDockables!);
        var deserializedDoc = deserializedRoot.HiddenDockables[0];
        Assert.Null(deserializedDoc.OriginalOwner);
    }
    
    [AvaloniaFact]
    public void MultipleHiddenDockables_SerializeDeserialize_PreservesOriginalOwners()
    {
        // Arrange
        var factory = new Factory();
        var root = new RootDock 
        { 
            Id = "root",
            HiddenDockables = factory.CreateList<IDockable>(),
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        
        var dock1 = new ToolDock 
        { 
            Id = "dock1",
            VisibleDockables = factory.CreateList<IDockable>() 
        };
        var dock2 = new ToolDock 
        { 
            Id = "dock2",
            VisibleDockables = factory.CreateList<IDockable>() 
        };
        dock1.Factory = factory;
        dock2.Factory = factory;
        
        var tool1 = new Tool { Title = "Tool1", Id = "tool1" };
        var tool2 = new Tool { Title = "Tool2", Id = "tool2" };
        
        // Add dockables
        factory.AddDockable(root, dock1);
        factory.AddDockable(root, dock2);
        factory.AddDockable(dock1, tool1);
        factory.AddDockable(dock2, tool2);
        
        // Hide both tools
        factory.HideDockable(tool1);
        factory.HideDockable(tool2);
        
        // Verify before serialization
        Assert.Equal(2, root.HiddenDockables!.Count);
        Assert.Equal(dock1, tool1.OriginalOwner);
        Assert.Equal(dock2, tool2.OriginalOwner);
        
        // Act
        var serializer = new AvaloniaDockSerializer();
        var json = serializer.Serialize(root);
        var deserializedRoot = serializer.Deserialize<RootDock>(json);
        
        // Assert
        Assert.NotNull(deserializedRoot);
        Assert.Equal(2, deserializedRoot!.HiddenDockables!.Count);
        
        var deserializedTool1 = deserializedRoot.HiddenDockables.First(d => d.Id == "tool1");
        var deserializedTool2 = deserializedRoot.HiddenDockables.First(d => d.Id == "tool2");
        
        Assert.NotNull(deserializedTool1.OriginalOwner);
        Assert.Equal("dock1", deserializedTool1.OriginalOwner!.Id);
        
        Assert.NotNull(deserializedTool2.OriginalOwner);
        Assert.Equal("dock2", deserializedTool2.OriginalOwner!.Id);
    }
}
