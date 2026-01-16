using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Dock.Serializer.SystemTextJson;
using Xunit;

namespace Dock.Serializer.UnitTests;

public class SystemTextJsonDockSerializerTests
{
    private class Sample
    {
        public string? Name { get; set; }
        public IList<int>? Numbers { get; set; }
        public string ReadOnly => "skip";
    }

    private class MultiModelSample
    {
        public IList<IDockable>? Dockables { get; set; }
    }

    private sealed class NonClosingMemoryStream : MemoryStream
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Flush();
            }
        }
    }

    [Fact]
    public void SerializeDeserialize_DefaultListType_ObservableCollection()
    {
        var serializer = new DockSerializer();
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 1, 2 } };

        var json = serializer.Serialize(sample);

        Assert.Contains("\"Name\"", json);
        Assert.DoesNotContain("ReadOnly", json);

        var result = serializer.Deserialize<Sample>(json);
        Assert.NotNull(result);
        Assert.Equal(sample.Name, result!.Name);
        Assert.IsType<ObservableCollection<int>>(result.Numbers);
        Assert.Equal(sample.Numbers, result.Numbers.ToList());
    }

    [Fact]
    public void CustomListType_List_DeserializesToList()
    {
        var serializer = new DockSerializer(typeof(List<>));
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 7, 8 } };

        var json = serializer.Serialize(sample);
        var result = serializer.Deserialize<Sample>(json);

        Assert.NotNull(result);
        Assert.IsType<List<int>>(result!.Numbers);
    }

    [Fact]
    public void SaveLoad_Roundtrip_Works()
    {
        var serializer = new DockSerializer();
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 3, 4, 5 } };
        using var stream = new NonClosingMemoryStream();

        serializer.Save(stream, sample);
        Assert.True(stream.Length > 0);

        stream.Position = 0;
        var loaded = serializer.Load<Sample>(stream);

        Assert.NotNull(loaded);
        Assert.Equal(sample.Name, loaded!.Name);
        Assert.IsType<ObservableCollection<int>>(loaded.Numbers);
        Assert.Equal(sample.Numbers, loaded.Numbers.ToList());
    }

    [Fact]
    public void Load_EmptyStream_ReturnsNull()
    {
        var serializer = new DockSerializer();
        using var stream = new NonClosingMemoryStream();

        var result = serializer.Load<Sample>(stream);

        Assert.Null(result);
    }

    [Fact]
    public void Save_Null_WritesNullString()
    {
        var serializer = new DockSerializer();
        using var stream = new NonClosingMemoryStream();

        serializer.Save<object?>(stream, null);
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = reader.ReadToEnd();

        Assert.Equal("null", text.Trim());
    }

    [Fact]
    public void OverlayLayout_Roundtrips_WithTypeDiscriminators()
    {
        var layout = OverlayLayoutBuilder.CreateLayout();
        var serializer = new DockSerializer();

        var json = serializer.Serialize(layout);
        var result = serializer.Deserialize<OverlayDock>(json);

        Assert.NotNull(result);
        Assert.Equal(layout.VisibleDockables?.Count, result!.VisibleDockables?.Count);
        Assert.IsType<OverlaySplitterGroup>(Assert.Single(result.SplitterGroups!));
        var restoredPanels = Assert.Single(result.SplitterGroups!).Panels;
        Assert.NotNull(restoredPanels);
        Assert.All(restoredPanels!, p => Assert.IsType<OverlayPanel>(p));

        var restoredBackground = result.VisibleDockables!.First();
        Assert.IsType<DocumentDock>(restoredBackground);
    }

    [Fact]
    public void TypeDiscriminators_AreUnique_Across_ModelAssemblies()
    {
        var serializer = new DockSerializer();
        var mvvmDock = new Dock.Model.Mvvm.Controls.OverlayDock { Id = "MvvmOverlay" };
        var avaloniaDock = new Dock.Model.Avalonia.Controls.OverlayDock { Id = "AvaloniaOverlay" };
        var sample = new MultiModelSample
        {
            Dockables = new List<IDockable> { mvvmDock, avaloniaDock }
        };

        var json = serializer.Serialize(sample);

        var mvvmTypeName = typeof(Dock.Model.Mvvm.Controls.OverlayDock).FullName ?? typeof(Dock.Model.Mvvm.Controls.OverlayDock).Name;
        var avaloniaTypeName = typeof(Dock.Model.Avalonia.Controls.OverlayDock).FullName ?? typeof(Dock.Model.Avalonia.Controls.OverlayDock).Name;

        Assert.Contains(mvvmTypeName, json);
        Assert.Contains(avaloniaTypeName, json);

        var result = serializer.Deserialize<MultiModelSample>(json);

        Assert.NotNull(result);
        Assert.NotNull(result!.Dockables);
        Assert.Collection(result.Dockables!,
            dockable => Assert.IsType<Dock.Model.Mvvm.Controls.OverlayDock>(dockable),
            dockable => Assert.IsType<Dock.Model.Avalonia.Controls.OverlayDock>(dockable));
    }
}
