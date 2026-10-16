using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text.Json;
using Dock.Model.Inpc.Controls;
using Dock.Serializer.SystemTextJson;
using Newtonsoft.Json;
using Xunit;

[assembly: DockJsonSerializable(typeof(Dock.Serializer.UnitTests.LegacyPublicFieldPayload))]

namespace Dock.Serializer.UnitTests;

public class LegacyMemberAndCollectionTests
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

    [Fact]
    public void PublicPayloadField_PreservesOldValue()
    {
        var source = new LegacyMemberCollectionDocument { Payload = new LegacyPublicFieldPayload { Value = "saved" } };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Equal("saved", JsonConvert.DeserializeObject<LegacyMemberCollectionDocument>(json, OldSettings)!.Payload!.Value);
        Assert.Equal("saved", new DockSerializer().Deserialize<LegacyMemberCollectionDocument>(json)!.Payload!.Value);
    }

    [Fact]
    public void NewtonsoftPropertyName_PreservesOldValue()
    {
        var source = new LegacyMemberCollectionDocument { Named = new LegacyNamedPayload { Value = "saved" } };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Contains("persisted_name", json);
        Assert.Equal("saved", JsonConvert.DeserializeObject<LegacyMemberCollectionDocument>(json, OldSettings)!.Named!.Value);
        Assert.Equal("saved", new DockSerializer().Deserialize<LegacyMemberCollectionDocument>(json)!.Named!.Value);
    }

    [Fact]
    public void ArrayInObjectSlot_LoadsOldJson()
    {
        var source = new CompatibilityTemplate { Content = new[] { 1, 2 } };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.IsType<Newtonsoft.Json.Linq.JArray>(JsonConvert.DeserializeObject<CompatibilityTemplate>(json, OldSettings)!.Content);
        var serializer = new DockSerializer();
        var restored = serializer.Deserialize<CompatibilityTemplate>(json)!;
        Assert.Equal(2, Assert.IsType<JsonElement>(restored.Content).GetArrayLength());
        var replay = serializer.Deserialize<CompatibilityTemplate>(serializer.Serialize(restored))!;
        Assert.Equal(1, Assert.IsType<JsonElement>(replay.Content)[0].GetInt32());
    }

    [Fact]
    public void ObjectListElement_PreservesPayloadType()
    {
        var source = new LegacyMemberCollectionDocument { Items = new() { new LegacyPublicFieldPayload { Value = "saved" } } };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.IsType<LegacyPublicFieldPayload>(JsonConvert.DeserializeObject<LegacyMemberCollectionDocument>(json, OldSettings)!.Items![0]);
        Assert.IsType<LegacyPublicFieldPayload>(new DockSerializer().Deserialize<LegacyMemberCollectionDocument>(json)!.Items![0]);
    }

    [Fact]
    public void HashSetProperty_LoadsOldArray()
    {
        var source = new LegacyMemberCollectionDocument { Tags = new() { "saved" } };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Contains("saved", JsonConvert.DeserializeObject<LegacyMemberCollectionDocument>(json, OldSettings)!.Tags!);
        Assert.Contains("saved", new DockSerializer().Deserialize<LegacyMemberCollectionDocument>(json)!.Tags!);
    }

    [Fact]
    public void NewtonsoftIgnore_DoesNotPersistExcludedMember()
    {
        var source = new LegacyMemberCollectionDocument { Named = new LegacyNamedPayload { Ignored = "excluded" } };
        Assert.DoesNotContain("excluded", JsonConvert.SerializeObject(source, OldSettings));
        Assert.DoesNotContain("excluded", new DockSerializer().Serialize(source));
    }

    [Fact]
    public void ObjectCollections_PreserveValuesAndSharedReferencesOnReplay()
    {
        var payload = new LegacyPublicFieldPayload { Value = "saved" };
        var source = new LegacyMemberCollectionDocument
        {
            Items = new() { payload, payload, "text", 42L, true },
            Map = new() { ["shared"] = payload }
        };
        var serializer = new DockSerializer();
        string json = JsonConvert.SerializeObject(source, OldSettings);
        for (int i = 0; i < 2; i++)
        {
            var restored = serializer.Deserialize<LegacyMemberCollectionDocument>(json)!;
            Assert.Equal("saved", Assert.IsType<LegacyPublicFieldPayload>(restored.Items![0]).Value);
            Assert.Same(restored.Items[0], restored.Items[1]);
            Assert.Same(restored.Items[0], restored.Map!["shared"]);
            Assert.Equal("text", restored.Items[2]);
            Assert.Equal(42L, Assert.IsType<long>(restored.Items[3]));
            Assert.True(Assert.IsType<bool>(restored.Items[4]));
            json = serializer.Serialize(restored);
        }
    }

    [Fact]
    public void AdditionalCollectionShapes_PreserveElements()
    {
        var source = new LegacyMemberCollectionDocument
        {
            Tags = new() { "saved" }, Queue = new(new[] { "first", "last" }),
            Custom = new() { new LegacyPublicFieldPayload { Value = "custom" } },
            ReadOnly = new[] { "readonly" }
        };
        var serializer = new DockSerializer();
        string json = JsonConvert.SerializeObject(source, OldSettings);
        for (int i = 0; i < 2; i++)
        {
            var restored = serializer.Deserialize<LegacyMemberCollectionDocument>(json)!;
            Assert.Contains("saved", restored.Tags!);
            Assert.Equal(new[] { "first", "last" }, restored.Queue!);
            Assert.Equal("custom", restored.Custom![0].Value);
            Assert.Equal(new[] { "readonly" }, restored.ReadOnly!);
            json = serializer.Serialize(restored);
        }
    }

    [Fact]
    public void RootArray_IsDiscoveredFromSerializerCalls()
    {
        var source = new[] { Guid.Parse("6392eaa2-bd9a-471c-b411-de8d7cb43a22") };
        var serializer = new DockSerializer();
        Assert.Equal(source, serializer.Deserialize<Guid[]>(JsonConvert.SerializeObject(source, OldSettings)));
        Assert.Equal(source, serializer.Deserialize<Guid[]>(serializer.Serialize(source)));
    }

    [Fact]
    public void NewtonsoftAttributedMembers_RoundTripNamesFieldsAndPrivateSetters()
    {
        var source = new LegacyNewtonsoftMembers("saved");
        var serializer = new DockSerializer();
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Equal("saved", JsonConvert.DeserializeObject<LegacyNewtonsoftMembers>(json, OldSettings)!.Value);
        for (int i = 0; i < 2; i++)
        {
            var restored = serializer.Deserialize<LegacyNewtonsoftMembers>(json)!;
            Assert.Equal("saved", restored.Value);
            Assert.Equal("saved", restored.Field);
            Assert.Equal("saved", restored.ReadPrivate());
            json = serializer.Serialize(restored);
            Assert.Contains("renamed", json);
            Assert.DoesNotContain("excluded", json);
        }
    }

    [Fact]
    public void JsonObjectOptIn_OnlyPersistsExplicitMembers()
    {
        var source = new LegacyOptInPayload { Included = "saved", Excluded = "excluded" };
        var serializer = new DockSerializer();
        string old = JsonConvert.SerializeObject(source, OldSettings);
        Assert.DoesNotContain("excluded", old);
        Assert.Equal("saved", serializer.Deserialize<LegacyOptInPayload>(old)!.Included);
        Assert.DoesNotContain("excluded", serializer.Serialize(source));
    }

    [Fact]
    public void DataMemberWithoutDataContract_KeepsNewtonsoftMemberNames()
    {
        var source = new LegacyUnscopedDataMember { Value = "saved", Field = "field" };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Contains("\"Value\"", json);
        var serializer = new DockSerializer();
        var restored = serializer.Deserialize<LegacyUnscopedDataMember>(json)!;
        Assert.Equal("saved", restored.Value);
        Assert.Equal("field", restored.Field);
        string replay = serializer.Serialize(restored);
        Assert.DoesNotContain("ignored_name", replay);
    }

    [Fact]
    public void JsonObjectFields_RestoresPrivateAndPublicFields()
    {
        var source = new LegacyFieldModePayload("saved") { Count = 42 };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Equal("saved", JsonConvert.DeserializeObject<LegacyFieldModePayload>(json, OldSettings)!.ReadSecret());
        var serializer = new DockSerializer();
        for (int i = 0; i < 2; i++)
        {
            var restored = serializer.Deserialize<LegacyFieldModePayload>(json)!;
            Assert.Equal("saved", restored.ReadSecret());
            Assert.Equal(42, restored.Count);
            json = serializer.Serialize(restored);
        }
    }

    [Fact]
    public void ObjectRootAndScalarDictionary_RoundTripGeneratedPayloads()
    {
        object source = new LegacyPublicFieldPayload { Value = "saved" };
        var serializer = new DockSerializer();
        var restored = Assert.IsType<LegacyPublicFieldPayload>(serializer.Deserialize<object>(serializer.Serialize(source)));
        Assert.Equal("saved", restored.Value);
        var values = new Dictionary<string, object> { ["text"] = "saved", ["number"] = 1.25, ["bytes"] = new byte[] { 1, 2 } };
        var replay = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(values))!;
        Assert.Equal("saved", Assert.IsType<string>(replay["text"]));
        Assert.Equal(1.25, Assert.IsType<double>(replay["number"]));
        Assert.Equal("AQI=", Assert.IsType<string>(replay["bytes"]));
    }
}

[DataContract]
public class LegacyMemberCollectionDocument : Document
{
    [DataMember] public LegacyPublicFieldPayload? Payload { get; set; }
    [DataMember] public LegacyNamedPayload? Named { get; set; }
    [DataMember] public List<object>? Items { get; set; }
    [DataMember] public HashSet<string>? Tags { get; set; }
    [DataMember] public Dictionary<string, object>? Map { get; set; }
    [DataMember] public Queue<string>? Queue { get; set; }
    [DataMember] public LegacyPayloadList? Custom { get; set; }
    [DataMember] public IReadOnlyCollection<string>? ReadOnly { get; set; }
}

public sealed class LegacyPayloadList : List<LegacyPublicFieldPayload>;

[DataContract]
public sealed class LegacyNewtonsoftMembers
{
    public LegacyNewtonsoftMembers() { }
    public LegacyNewtonsoftMembers(string value) { Value = value; Field = value; _private = value; }
    [JsonProperty("renamed")] public string? Value { get; private set; }
    [JsonProperty("field")] public string? Field;
    [JsonProperty("private")] private string? _private;
    [DataMember, JsonIgnore] public string Ignored = "excluded";
    public string? ReadPrivate() => _private;
}

public class LegacyPublicFieldPayload
{
    public string? Value;
}

public class LegacyNamedPayload
{
    [JsonProperty("persisted_name")] public string? Value { get; set; }
    [JsonIgnore] public string? Ignored { get; set; }
}

[JsonObject(MemberSerialization.OptIn)]
public class LegacyOptInPayload
{
    [JsonProperty("included")] public string? Included { get; set; }
    public string? Excluded { get; set; }
    [DataMember] public string DataMemberOnly { get; set; } = "excluded data member";
}

public class LegacyUnscopedDataMember
{
    [DataMember(Name = "ignored_name")] public string? Value { get; set; }
    [DataMember(Name = "ignored_field_name")] public string? Field;
}

[JsonObject(MemberSerialization.Fields)]
public class LegacyFieldModePayload
{
    public LegacyFieldModePayload() { }
    public LegacyFieldModePayload(string secret) { _secret = secret; }
    private string? _secret;
    public int Count;
    public string? ReadSecret() => _secret;
}
