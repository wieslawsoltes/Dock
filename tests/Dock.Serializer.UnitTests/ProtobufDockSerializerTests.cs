using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dock.Serializer.Protobuf;
using Xunit;

namespace Dock.Serializer.UnitTests;

public class ProtobufDockSerializerTests
{
    [ProtoBuf.ProtoContract]
    private class Sample
    {
        [ProtoBuf.ProtoMember(1)]
        public string? Name { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public IList<int>? Numbers { get; set; }
    }

    [Fact]
    public void SerializeDeserialize_DefaultListType_ObservableCollection()
    {
        var serializer = new ProtobufDockSerializer();
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 1, 2 } };

        var data = serializer.Serialize(sample);
        var result = serializer.Deserialize<Sample>(data);

        Assert.NotNull(result);
        Assert.Equal(sample.Name, result!.Name);
        Assert.IsType<ObservableCollection<int>>(result.Numbers);
        Assert.Equal(sample.Numbers, result.Numbers.ToList());
    }

    [Fact]
    public void CustomListType_List_DeserializesToList()
    {
        var serializer = new ProtobufDockSerializer(typeof(List<>));
        var sample = new Sample { Name = "Test", Numbers = new List<int> { 7, 8 } };

        var data = serializer.Serialize(sample);
        var result = serializer.Deserialize<Sample>(data);

        Assert.NotNull(result);
        Assert.IsType<List<int>>(result!.Numbers);
    }
}
