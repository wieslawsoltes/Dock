using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Json;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class AvaloniaDockSerializerTests
{
    private class Sample
    {
        public string? Name { get; set; }
        public IList<int>? Numbers { get; set; }
        public string ReadOnly => "skip";
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

    [AvaloniaFact]
    public void SerializeDeserialize_DefaultListType_AvaloniaList()
    {
        var serializer = new AvaloniaDockSerializer();
        var sample = new Sample { Name = "Test", Numbers = new AvaloniaList<int> { 1, 2 } };

        var json = serializer.Serialize(sample);

        Assert.Contains("\"Name\"", json);
        Assert.DoesNotContain("ReadOnly", json);

        var result = serializer.Deserialize<Sample>(json);
        Assert.NotNull(result);
        Assert.Equal(sample.Name, result!.Name);
        Assert.IsType<AvaloniaList<int>>(result.Numbers);
        Assert.Equal(sample.Numbers, result.Numbers.ToList());
    }

    [AvaloniaFact]
    public void SaveLoad_Roundtrip_Works()
    {
        var serializer = new AvaloniaDockSerializer();
        var sample = new Sample { Name = "Test", Numbers = new AvaloniaList<int> { 3, 4, 5 } };
        using var stream = new NonClosingMemoryStream();

        serializer.Save(stream, sample);
        Assert.True(stream.Length > 0);

        stream.Position = 0;
        var loaded = serializer.Load<Sample>(stream);

        Assert.NotNull(loaded);
        Assert.Equal(sample.Name, loaded!.Name);
        Assert.IsType<AvaloniaList<int>>(loaded.Numbers);
        Assert.Equal(sample.Numbers, loaded.Numbers.ToList());
    }
}
