using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Dock.Model.Inpc.Controls;
using Newtonsoft.Json;
using Xunit;

namespace Dock.Serializer.UnitTests;

public class LegacyContractRegressionTests
{
    private static readonly JsonSerializerSettings OldSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Objects,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        ContractResolver = new ListContractResolver(typeof(ObservableCollection<>)),
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new Newtonsoft.Json.Converters.KeyValuePairConverter() }
    };

    [Fact]
    public void LegacyNestedCollectionPayload_Loads()
    {
        var item = new LegacyContractItem { Value = "saved" };
        var source = new LegacyContractCollectionDocument
        {
            Items = new() { item },
            Array = new[] { item },
            Dictionary = new() { ["saved"] = item }
        };
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Equal("saved", JsonConvert.DeserializeObject<LegacyContractCollectionDocument>(json, OldSettings)!.Items![0].Value);
        var restored = new DockSerializer().Deserialize<LegacyContractCollectionDocument>(json);
        Assert.Equal("saved", restored!.Items![0].Value);
        Assert.Same(restored.Items[0], restored.Array![0]);
        Assert.Same(restored.Items[0], restored.Dictionary!["saved"]);
        var replay = new DockSerializer().Deserialize<LegacyContractCollectionDocument>(new DockSerializer().Serialize(restored));
        Assert.Equal("saved", replay!.Items![0].Value);
    }

    [Fact]
    public void LegacyDataMemberPrivateSetter_Loads()
    {
        string json = JsonConvert.SerializeObject(new LegacyContractPrivateSetterDocument("saved"), OldSettings);
        Assert.Equal("saved", JsonConvert.DeserializeObject<LegacyContractPrivateSetterDocument>(json, OldSettings)!.Value);
        var restored = new DockSerializer().Deserialize<LegacyContractPrivateSetterDocument>(json);
        Assert.Equal("saved", restored!.Value);
        Assert.Equal("saved", new DockSerializer().Deserialize<LegacyContractPrivateSetterDocument>(new DockSerializer().Serialize(restored))!.Value);
    }

    [Fact]
    public void LegacyDataMemberField_Loads()
    {
        string json = JsonConvert.SerializeObject(new LegacyContractFieldDocument { Value = "saved" }, OldSettings);
        Assert.Equal("saved", JsonConvert.DeserializeObject<LegacyContractFieldDocument>(json, OldSettings)!.Value);
        var restored = new DockSerializer().Deserialize<LegacyContractFieldDocument>(json);
        Assert.Equal("saved", restored!.Value);
        Assert.Equal("saved", new DockSerializer().Deserialize<LegacyContractFieldDocument>(new DockSerializer().Serialize(restored))!.Value);
    }

    [Fact]
    public void LegacyPrivateFieldAndInheritedGenericSetter_LoadAndReplay()
    {
        var source = new LegacyContractDerivedDocument("saved", "private");
        string json = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Contains("field_name", json);
        Assert.Equal("private", JsonConvert.DeserializeObject<LegacyContractDerivedDocument>(json, OldSettings)!.ReadField());
        var serializer = new DockSerializer();
        var restored = serializer.Deserialize<LegacyContractDerivedDocument>(json);
        Assert.Equal("saved", restored!.Value);
        Assert.Equal("private", restored.ReadField());
        Assert.Equal("private property", restored.ReadHiddenProperty());
        var replay = serializer.Deserialize<LegacyContractDerivedDocument>(serializer.Serialize(restored));
        Assert.Equal("saved", replay!.Value);
        Assert.Equal("private", replay.ReadField());
        Assert.Equal("private property", replay.ReadHiddenProperty());
    }

    [Fact]
    public void LegacyDataMemberFields_KeepNamesDefaultsAndStructValues()
    {
        var source = new LegacyContractValueDocument { Payload = new LegacyContractValue { Number = 42 } };
        var serializer = new DockSerializer();
        string oldJson = JsonConvert.SerializeObject(source, OldSettings);
        Assert.Equal(42, JsonConvert.DeserializeObject<LegacyContractValueDocument>(oldJson, OldSettings)!.Payload.Number);
        var restored = serializer.Deserialize<LegacyContractValueDocument>(oldJson);
        Assert.Equal(42, restored!.Payload.Number);
        string currentJson = serializer.Serialize(restored);
        Assert.Contains("number", currentJson);
        Assert.DoesNotContain("omitted", currentJson);
        Assert.Equal(42, serializer.Deserialize<LegacyContractValueDocument>(currentJson)!.Payload.Number);
    }

    [Fact]
    public void LegacyObjectFields_KeepPayloadTypeAndSharedReferences()
    {
        var payload = new CompatibilityPayload { Name = "saved" };
        var source = new LegacyContractObjectFieldsDocument { First = payload, Second = payload };
        var serializer = new DockSerializer();
        var restored = serializer.Deserialize<LegacyContractObjectFieldsDocument>(JsonConvert.SerializeObject(source, OldSettings));
        Assert.Equal("saved", Assert.IsType<CompatibilityPayload>(restored!.First).Name);
        Assert.Same(restored.First, restored.Second);
        var replay = serializer.Deserialize<LegacyContractObjectFieldsDocument>(serializer.Serialize(restored));
        Assert.Equal("saved", Assert.IsType<CompatibilityPayload>(replay!.First).Name);
        Assert.Same(replay.First, replay.Second);
    }
}

[DataContract]
public class LegacyContractCollectionDocument : Document
{
    [DataMember] public List<LegacyContractItem>? Items { get; set; }
    [DataMember] public LegacyContractItem[]? Array { get; set; }
    [DataMember] public Dictionary<string, LegacyContractItem>? Dictionary { get; set; }
}

public class LegacyContractItem
{
    public string? Value { get; set; }
}

[DataContract]
public class LegacyContractPrivateSetterDocument : Document
{
    public LegacyContractPrivateSetterDocument() { }
    public LegacyContractPrivateSetterDocument(string value) { Value = value; }
    [DataMember] public string? Value { get; private set; }
}

[DataContract]
public class LegacyContractFieldDocument : Document
{
    [DataMember] public string? Value;
}

[DataContract]
public class LegacyContractGenericDocument<T> : Document where T : class
{
    public LegacyContractGenericDocument() { }
    protected LegacyContractGenericDocument(T value) { Value = value; }
    [DataMember] public T? Value { get; private set; }
}

[DataContract]
public class LegacyContractDerivedDocument : LegacyContractGenericDocument<string>
{
    public LegacyContractDerivedDocument() { }
    public LegacyContractDerivedDocument(string value, string field) : base(value) { _field = field; Hidden = field + " property"; }
    [DataMember(Name = "field_name", Order = 1)] private string? _field;
    public string? ReadField() => _field;
    [DataMember(Name = "hidden_property")] private string? Hidden { get; set; }
    public string? ReadHiddenProperty() => Hidden;
}

[DataContract]
public class LegacyContractValueDocument : Document
{
    [DataMember] public LegacyContractValue Payload;
}

[DataContract]
public struct LegacyContractValue
{
    [DataMember(Name = "number")] public int Number;
    [DataMember(Name = "omitted", EmitDefaultValue = false)] public int Omitted;
}

[DataContract]
public class LegacyContractObjectFieldsDocument : Document
{
    [DataMember] public object? First;
    [DataMember] public object? Second;
}
