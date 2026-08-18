using System.Collections.ObjectModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Inpc.Controls;
using Dock.Serializer.Protobuf;
using Xunit;

namespace Dock.Serializer.UnitTests;

// Must be public: protobuf-net type discovery only considers public types.
public sealed class ProtobufCustomDocument : Document
{
}

public class ProtobufCustomSubtypeTests
{
    [Fact]
    public void SaveLoad_Roundtrip_PreservesCustomDockableSubtype()
    {
        var document = new ProtobufCustomDocument { Id = "Home", Title = "Home" };
        var documentDock = new DocumentDock
        {
            Id = "DocumentDock",
            VisibleDockables = new ObservableCollection<IDockable> { document },
            ActiveDockable = document,
        };
        IRootDock layout = new RootDock
        {
            Id = "Root",
            VisibleDockables = new ObservableCollection<IDockable> { documentDock },
            ActiveDockable = documentDock,
        };

        var serializer = new ProtobufDockSerializer();
        var text = serializer.Serialize(layout);
        var restored = serializer.Deserialize<IRootDock>(text);

        Assert.NotNull(restored);
        var restoredDocDock = Assert.IsType<DocumentDock>(restored!.VisibleDockables![0]);
        Assert.IsType<ProtobufCustomDocument>(restoredDocDock.VisibleDockables![0]);
    }
}
