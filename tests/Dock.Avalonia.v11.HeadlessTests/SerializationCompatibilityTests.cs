using System.Collections.ObjectModel;
using Avalonia.Headless.XUnit;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Xunit;
using JsonDockSerializer = Dock.Serializer.SystemTextJson.DockSerializer;

namespace Dock.Avalonia.v11.HeadlessTests;

public class SerializationCompatibilityTests
{
    [AvaloniaFact]
    public void Avalonia11ReactiveLayout_PreviouslySavedJson_PreservesDockableIdentity()
    {
        const string json = """
            {
              "$id":"1",
              "$type":"Dock.Model.ReactiveUI.Controls.RootDock, Dock.Model.ReactiveUI",
              "Id":"Root",
              "VisibleDockables":[
                {
                  "$id":"2",
                  "$type":"Dock.Model.ReactiveUI.Controls.Document, Dock.Model.ReactiveUI",
                  "Id":"Document",
                  "Owner":{"$ref":"1"}
                }
              ],
              "ActiveDockable":{"$ref":"2"}
            }
            """;
        var serializer = new JsonDockSerializer();

        RootDock? restored = serializer.Deserialize<RootDock>(json);

        Assert.NotNull(restored);
        Assert.IsType<ObservableCollection<IDockable>>(restored.VisibleDockables);
        Assert.IsType<Document>(restored.VisibleDockables![0]);
        Assert.Same(restored.VisibleDockables[0], restored.ActiveDockable);
        Assert.Same(restored, restored.ActiveDockable!.Owner);
        Assert.NotNull(serializer.Deserialize<RootDock>(serializer.Serialize(restored)));
    }
}
