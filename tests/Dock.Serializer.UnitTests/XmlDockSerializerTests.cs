using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dock.Serializer.Xml;
using Xunit;

namespace Dock.Serializer.UnitTests;

public class XmlDockSerializerTests
{
    [System.Runtime.Serialization.DataContract]
    private class Sample
    {
        [System.Runtime.Serialization.DataMember]
        public string? Name { get; set; }
        [System.Runtime.Serialization.DataMember]
        public IList<int>? Numbers { get; set; }
    }

    [Fact]
    public void SerializeDeserialize_DefaultListType_ObservableCollection()
    {
        var serializer = new DockXmlSerializer();
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 1, 2 } };

        var xml = serializer.Serialize(sample);
        var result = serializer.Deserialize<Sample>(xml);

        Assert.NotNull(result);
        Assert.Equal(sample.Name, result!.Name);
        Assert.IsType<ObservableCollection<int>>(result.Numbers);
        Assert.Equal(sample.Numbers, result.Numbers.ToList());
    }

    [Fact]
    public void CustomListType_List_DeserializesToList()
    {
        var serializer = new DockXmlSerializer(typeof(List<>));
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 7, 8 } };

        var xml = serializer.Serialize(sample);
        var result = serializer.Deserialize<Sample>(xml);

        Assert.NotNull(result);
        Assert.IsType<List<int>>(result!.Numbers);
    }
}
