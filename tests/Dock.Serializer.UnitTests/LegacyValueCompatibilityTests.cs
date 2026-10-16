using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.Serialization;
using Dock.Model.Inpc.Controls;
using Dock.Serializer.SystemTextJson;
using Newtonsoft.Json;
using Xunit;

[assembly: DockJsonSerializable(typeof(Dock.Serializer.UnitTests.DerivedLayoutMetadata))]

namespace Dock.Serializer.UnitTests;

public class LegacyValueCompatibilityTests
{
    private static JsonSerializerSettings OldSettings => new()
    {
        TypeNameHandling = TypeNameHandling.Objects,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        ContractResolver = new ListContractResolver(typeof(ObservableCollection<>)),
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new Newtonsoft.Json.Converters.KeyValuePairConverter() }
    };

    [Theory]
    [InlineData("[1, 2]")]
    [InlineData("{\"$type\":\"Removed.Type, Removed.Assembly\",\"Items\":[1,2]}")]
    [InlineData("\"obsolete\"")]
    public void RemovedProperty_IsIgnoredInOldLayout(string obsoleteValue)
    {
        string json = $$"""
            { "$type": "Dock.Model.Inpc.Controls.RootDock, Dock.Model.Inpc", "RemovedItems": {{obsoleteValue}}, "Id": "saved" }
            """;
        Assert.Equal("saved", JsonConvert.DeserializeObject<RootDock>(json, OldSettings)!.Id);
        Assert.Equal("saved", new DockSerializer().Deserialize<RootDock>(json)!.Id);
    }

    [Fact]
    public void ScalarObjectPayload_LegacyReadPreservesString()
    {
        var source = new CompatibilityTemplate { Content = "saved" };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Equal("saved", Assert.IsType<string>(JsonConvert.DeserializeObject<CompatibilityTemplate>(json, OldSettings)!.Content));
        Assert.Equal("saved", Assert.IsType<string>(new DockSerializer().Deserialize<CompatibilityTemplate>(json)!.Content));
    }

    [Fact]
    public void ScalarObjectPayload_CanStillBeSaved()
    {
        var source = new CompatibilityTemplate { Content = "saved" };
        var serializer = new DockSerializer();
        Assert.Contains("saved", JsonConvert.SerializeObject(source, OldSettings));
        Assert.Contains("saved", serializer.Serialize(source));
    }

    [Fact]
    public void ScalarObjectPayloads_MatchLegacyTypesAndValues()
    {
        object?[] values =
        {
            null, "saved", "2025-01-02", "2025-01-02T03:04:05", true, false, 'x', (byte)1, (sbyte)-1, (short)-2, (ushort)2,
            42, 42U, long.MaxValue, ulong.MaxValue, 1.0f, 1.25f, 1.0d, 1.25d, 1m, 1.25m,
            double.NaN, double.PositiveInfinity, float.NegativeInfinity,
            new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)),
            Guid.Parse("6392eaa2-bd9a-471c-b411-de8d7cb43a22"), TimeSpan.FromMinutes(5),
            new Uri("https://example.com/layout"), BigInteger.Parse("123456789012345678901234567890")
        };
        var serializer = new DockSerializer();
        foreach (object? value in values)
        {
            var source = new CompatibilityTemplate { Content = value };
            string oldJson = JsonConvert.SerializeObject(source, OldSettings);
            object? expected = JsonConvert.DeserializeObject<CompatibilityTemplate>(oldJson, OldSettings)!.Content;
            object? loaded = serializer.Deserialize<CompatibilityTemplate>(oldJson)!.Content;
            Assert.Equal(expected?.GetType(), loaded?.GetType());
            Assert.Equal(expected, loaded);
            object? replay = serializer.Deserialize<CompatibilityTemplate>(serializer.Serialize(source))!.Content;
            Assert.Equal(expected?.GetType(), replay?.GetType());
            Assert.Equal(expected, replay);
        }
    }

    [Fact]
    public void LegacyConcreteBaseProperty_KeepsRegisteredDerivedType()
    {
        var metadata = new DerivedLayoutMetadata { BaseValue = "base", Extra = "saved" };
        var source = new MetadataDocument { Metadata = metadata, SharedMetadata = metadata };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Equal("saved", Assert.IsType<DerivedLayoutMetadata>(JsonConvert.DeserializeObject<MetadataDocument>(json, OldSettings)!.Metadata).Extra);
        var serializer = new DockSerializer();
        var restored = serializer.Deserialize<MetadataDocument>(json)!;
        Assert.Equal("saved", Assert.IsType<DerivedLayoutMetadata>(restored.Metadata).Extra);
        Assert.Same(restored.Metadata, restored.SharedMetadata);
        var replay = serializer.Deserialize<MetadataDocument>(serializer.Serialize(restored))!;
        Assert.Equal("saved", Assert.IsType<DerivedLayoutMetadata>(replay.Metadata).Extra);
        Assert.Same(replay.Metadata, replay.SharedMetadata);
    }

    [Fact]
    public void SaveThroughConcreteBase_KeepsDerivedDocument()
    {
        Document source = new DerivedLayoutDocument { Id = "document", Extra = "saved" };
        Assert.Equal("saved", Assert.IsType<DerivedLayoutDocument>(JsonConvert.DeserializeObject<Document>(JsonConvert.SerializeObject(source, OldSettings), OldSettings)).Extra);
        var serializer = new DockSerializer();
        Assert.Equal("saved", Assert.IsType<DerivedLayoutDocument>(serializer.Deserialize<Document>(serializer.Serialize(source))).Extra);
    }

    [Fact]
    public void ExactBaseInstance_LoadsAndReplaysWithDerivedRegistrations()
    {
        var source = new MetadataDocument { Metadata = new LayoutMetadata { BaseValue = "saved" } };
        var serializer = new DockSerializer();
        var restored = serializer.Deserialize<MetadataDocument>(JsonConvert.SerializeObject(source, OldSettings))!;
        Assert.Equal("saved", Assert.IsType<LayoutMetadata>(restored.Metadata).BaseValue);
        var replay = serializer.Deserialize<MetadataDocument>(serializer.Serialize(restored))!;
        Assert.Equal("saved", Assert.IsType<LayoutMetadata>(replay.Metadata).BaseValue);
    }

    [Fact]
    public void ExtensionData_PreservesUnmodeledJsonWithoutResolvingItsTypes()
    {
        const string json = """
            {"Name":"saved","Extra":{"$type":"Unknown.Type, Removed.Assembly","Values":[1,2]}}
            """;
        var restored = new DockSerializer().Deserialize<ExtensionDataPayload>(json)!;
        Assert.Equal("saved", restored.Name);
        Assert.Equal("Unknown.Type, Removed.Assembly", restored.Additional!["Extra"].GetProperty("$type").GetString());
        Assert.Equal(2, restored.Additional["Extra"].GetProperty("Values").GetArrayLength());
    }
}

[DataContract]
public class MetadataDocument : Document
{
    [DataMember] public LayoutMetadata? Metadata { get; set; }
    [DataMember] public LayoutMetadata? SharedMetadata { get; set; }
}

public class LayoutMetadata
{
    public string? BaseValue { get; set; }
}

public class DerivedLayoutMetadata : LayoutMetadata
{
    public string? Extra { get; set; }
}

[DataContract]
public class DerivedLayoutDocument : Document
{
    [DataMember] public string? Extra { get; set; }
}

public class ExtensionDataPayload
{
    public string? Name { get; set; }
    [System.Text.Json.Serialization.JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? Additional { get; set; }
}
