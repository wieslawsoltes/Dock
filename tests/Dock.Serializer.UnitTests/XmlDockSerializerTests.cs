using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

    [Fact]
    public void SaveLoad_Roundtrip_Works()
    {
        var serializer = new DockXmlSerializer();
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
    public async Task SaveLoadAsync_Roundtrip_Works()
    {
        var serializer = new DockXmlSerializer();
        var sample = new Sample { Name = "Async", Numbers = new List<int> { 9, 10 } };
        await using var stream = new NonClosingMemoryStream();

        await serializer.SaveAsync(stream, sample);
        Assert.True(stream.Length > 0);

        stream.Position = 0;
        var loaded = await serializer.LoadAsync<Sample>(stream);

        Assert.NotNull(loaded);
        Assert.Equal(sample.Name, loaded!.Name);
        Assert.IsType<ObservableCollection<int>>(loaded.Numbers);
        Assert.Equal(sample.Numbers, loaded.Numbers.ToList());
    }
}
