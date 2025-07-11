using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dock.Serializer.Yaml;
using Xunit;

namespace Dock.Serializer.UnitTests;

public class YamlDockSerializerTests
{
    private class Sample
    {
        public string? Name { get; set; }
        public IList<int>? Numbers { get; set; }
    }

    [Fact]
    public void SerializeDeserialize_DefaultListType_ObservableCollection()
    {
        var serializer = new DockYamlSerializer();
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 1, 2 } };

        var yaml = serializer.Serialize(sample);
        var result = serializer.Deserialize<Sample>(yaml);

        Assert.NotNull(result);
        Assert.Equal(sample.Name, result!.Name);
        Assert.IsType<ObservableCollection<int>>(result.Numbers);
        Assert.Equal(sample.Numbers, result.Numbers.ToList());
    }

    [Fact]
    public void CustomListType_List_DeserializesToList()
    {
        var serializer = new DockYamlSerializer(typeof(List<>));
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 7, 8 } };

        var yaml = serializer.Serialize(sample);
        var result = serializer.Deserialize<Sample>(yaml);

        Assert.NotNull(result);
        Assert.IsType<List<int>>(result!.Numbers);
    }
}
