using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dock.Serializer.SystemTextJson;
using Xunit;

[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.CollectionPayload))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.FieldPayload))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.IncludedPropertyPayload))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.PolymorphicCollectionPayload))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.RegisteredList<int>))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.RegisteredList<string>))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.RegisteredList<long>))]

namespace Dock.Serializer.SystemTextJson.SourceGenTests;

public class SourceGeneratedCollectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Payload_Lists_Preserve_Configured_Collection_Type(bool observable)
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer(observable ? typeof(ObservableCollection<>) : typeof(List<>));
        var source = CreatePayload();

        var restored = serializer.Deserialize<CollectionPayload>(serializer.Serialize(source));

        AssertPayload(restored);
        if (observable)
        {
            Assert.IsType<ObservableCollection<int>>(restored!.Numbers);
            Assert.IsType<ObservableCollection<string>>(restored.Names);
            Assert.IsType<ObservableCollection<long>>(restored.Children["child"].Values);
        }
        else
        {
            Assert.IsType<List<int>>(restored!.Numbers);
            Assert.IsType<List<string>>(restored.Names);
            Assert.IsType<List<long>>(restored.Children["child"].Values);
        }
    }

    [Fact]
    public void Registered_Custom_Lists_Use_Generated_Constructors()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer(typeof(RegisteredList<>));
        var source = CreatePayload();

        var restored = serializer.Deserialize<CollectionPayload>(serializer.Serialize(source));

        AssertPayload(restored);
        Assert.IsType<RegisteredList<int>>(restored!.Numbers);
        Assert.IsType<RegisteredList<string>>(restored.Names);
        Assert.IsType<RegisteredList<long>>(restored.Children["child"].Values);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JsonInclude_Fields_Including_Base_Fields_Use_Configured_Lists(bool useDefaultConstructor)
    {
        var serializer = useDefaultConstructor ? new DockSerializer() : DockSystemTextJsonGenerated.CreateSerializer();
        var source = new FieldPayload
        {
            Numbers = new List<double> { 1.5, 2.5 },
            BaseNumbers = new List<float> { 3.5f }
        };

        var restored = serializer.Deserialize<FieldPayload>(serializer.Serialize(source));

        Assert.Equal(source.Numbers, Assert.IsType<ObservableCollection<double>>(restored!.Numbers));
        Assert.Equal(source.BaseNumbers, Assert.IsType<ObservableCollection<float>>(restored.BaseNumbers));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JsonInclude_NonPublic_Properties_Including_Base_Properties_Use_Configured_Lists(bool useDefaultConstructor)
    {
        var serializer = useDefaultConstructor ? new DockSerializer() : DockSystemTextJsonGenerated.CreateSerializer();
        var source = new IncludedPropertyPayload
        {
            Values = new List<decimal> { 1.25m, 2.5m },
            BaseValues = new List<TimeSpan> { TimeSpan.FromMinutes(3) }
        };

        var restored = serializer.Deserialize<IncludedPropertyPayload>(serializer.Serialize(source));

        Assert.Equal(source.Values, Assert.IsType<ObservableCollection<decimal>>(restored!.Values));
        Assert.Equal(source.BaseValues, Assert.IsType<ObservableCollection<TimeSpan>>(restored.BaseValues));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JsonDerivedType_Payload_Properties_Use_Configured_Lists(bool useDefaultConstructor)
    {
        var serializer = useDefaultConstructor ? new DockSerializer() : DockSystemTextJsonGenerated.CreateSerializer();
        PolymorphicCollectionPayload source = new DerivedCollectionPayload
        {
            Values = new List<short> { 3, 7 }
        };

        var restored = Assert.IsType<DerivedCollectionPayload>(
            serializer.Deserialize<PolymorphicCollectionPayload>(serializer.Serialize(source)));

        Assert.Equal(new short[] { 3, 7 }, Assert.IsType<ObservableCollection<short>>(restored.Values));
    }

    [Fact]
    public void Default_Serializer_Preserves_Nested_Discriminators_And_Shared_References()
    {
        var serializer = new DockSerializer();
        var item = new DerivedCollectionPayload { Values = new List<short> { 3, 7 } };
        var source = new PolymorphicEnvelope { First = item, Second = item };

        var restored = serializer.Deserialize<PolymorphicEnvelope>(serializer.Serialize(source))!;

        var child = Assert.IsType<DerivedCollectionPayload>(restored.First);
        Assert.Equal(new short[] { 3, 7 }, Assert.IsType<ObservableCollection<short>>(child.Values));
        Assert.Same(restored.First, restored.Second);
    }

    [Fact]
    public void Default_Serializer_Preserves_Numeric_Discriminators_With_Custom_Property_Name()
    {
        var serializer = new DockSerializer();
        NumericPayload source = new NumericDerivedPayload { Name = "saved" };

        var restored = Assert.IsType<NumericDerivedPayload>(serializer.Deserialize<NumericPayload>(serializer.Serialize(source)));

        Assert.Equal("saved", restored.Name);
        // Older Newtonsoft files still use assembly-qualified $type names.
        var legacy = Assert.IsType<NumericDerivedPayload>(serializer.Deserialize<NumericPayload>(
            "{\"$type\":\"Dock.Serializer.SystemTextJson.SourceGenTests.NumericDerivedPayload, Dock.Serializer.SystemTextJson.SourceGenTests\",\"Name\":\"legacy\"}"));
        Assert.Equal("legacy", legacy.Name);
    }

    [Fact]
    public void Generated_Factory_Declines_Unregistered_Collection_Pairs()
    {
        IDockJsonCollectionFactory resolver = new DockSystemTextJsonResolver();
        Assert.Null(resolver.GetListFactory(typeof(IList<DateTime>), typeof(RegisteredList<>)));
        Assert.Null(resolver.GetListFactory(typeof(CollectionPayload), typeof(List<>)));
    }

    [Fact]
    public void Generated_Factory_Preserves_Compatible_Supplied_Instances()
    {
        IDockJsonCollectionFactory resolver = new DockSystemTextJsonResolver();
        var numbers = new ObservableCollection<int>();
        var names = new List<string>();
        var custom = new RegisteredList<long>();

        Assert.Same(numbers, resolver.GetListFactory(typeof(IList<int>), typeof(ObservableCollection<>), () => numbers)!());
        Assert.Same(names, resolver.GetListFactory(typeof(IList<string>), typeof(List<>), () => names)!());
        Assert.Same(custom, resolver.GetListFactory(typeof(IList<long>), typeof(RegisteredList<>), () => custom)!());
        Assert.IsType<ObservableCollection<int>>(resolver.GetListFactory(typeof(IList<int>), typeof(ObservableCollection<>), static () => new List<int>())!());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Supplied_Collection_Factories_Retain_Constructor_Behavior(bool exposeCollectionFactories)
    {
        var resolver = new SuppliedCollectionResolver();
        IJsonTypeInfoResolver metadata = exposeCollectionFactories ? resolver : resolver.WithAddedModifier(static _ => { });
        var serializer = new DockSerializer(typeof(RegisteredList<>), metadata);
        var source = CreatePayload();

        var restored = serializer.Deserialize<CollectionPayload>(serializer.Serialize(source));

        AssertPayload(restored);
        Assert.True(Assert.IsType<RegisteredList<int>>(restored!.Numbers).Supplied);
        Assert.True(Assert.IsType<RegisteredList<string>>(restored.Names).Supplied);
        Assert.True(Assert.IsType<RegisteredList<long>>(restored.Children["child"].Values).Supplied);
        Assert.Equal(3, resolver.FactoryCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void External_SourceGenerated_Context_Owns_Unknown_Collection_Factories(bool useDefaultListType)
    {
        // An external resolver without Dock factory metadata owns its existing creator,
        // including when the serializer's default list type differs from that creator.
        var serializer = useDefaultListType
            ? new DockSerializer(ExternalCollectionContext.Default)
            : new DockSerializer(typeof(List<>), ExternalCollectionContext.Default);
        var source = CreatePayload();

        var restored = serializer.Deserialize<CollectionPayload>(serializer.Serialize(source));

        AssertPayload(restored);
        Assert.IsType<List<int>>(restored!.Numbers);
        Assert.IsType<List<string>>(restored.Names);
        Assert.IsType<List<long>>(restored.Children["child"].Values);
    }

    [Fact]
    public void Registered_Factory_Constructs_Collections_When_External_Metadata_Has_No_Creator()
    {
        var resolver = ExternalCollectionContext.Default.WithAddedModifier(static info =>
        {
            if (info.Type == typeof(IList<int>) || info.Type == typeof(IList<string>) || info.Type == typeof(IList<long>))
                info.CreateObject = null;
        });
        var serializer = new DockSerializer(typeof(RegisteredList<>), resolver);
        var source = CreatePayload();

        var restored = serializer.Deserialize<CollectionPayload>(serializer.Serialize(source));

        AssertPayload(restored);
        Assert.IsType<RegisteredList<int>>(restored!.Numbers);
        Assert.IsType<RegisteredList<string>>(restored.Names);
        Assert.IsType<RegisteredList<long>>(restored.Children["child"].Values);
    }

    private sealed class SuppliedCollectionResolver : IJsonTypeInfoResolver, IDockJsonCollectionFactory
    {
        private readonly DockSystemTextJsonResolver _generated = new();
        public int FactoryCalls { get; private set; }

        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo? info = _generated.GetTypeInfo(type, options);
            if (info is null)
                return null;
            if (type == typeof(IList<int>))
                info.CreateObject = CreateSupplied<int>;
            else if (type == typeof(IList<string>))
                info.CreateObject = CreateSupplied<string>;
            else if (type == typeof(IList<long>))
                info.CreateObject = CreateSupplied<long>;
            return info;
        }

        public Func<object>? GetListFactory(Type collectionType, Type listType, Func<object>? suppliedFactory = null)
            => _generated.GetListFactory(collectionType, listType, suppliedFactory);

        private object CreateSupplied<T>()
        {
            FactoryCalls++;
            return new RegisteredList<T> { Supplied = true };
        }
    }

    private static CollectionPayload CreatePayload() => new()
    {
        Numbers = new List<int> { 1, 2, 3 },
        Names = new List<string> { "first", "second" },
        Children = new Dictionary<string, CollectionChild>
        {
            ["child"] = new CollectionChild { Values = new List<long> { 42, 84 } }
        }
    };

    private static void AssertPayload(CollectionPayload? restored)
    {
        Assert.NotNull(restored);
        Assert.Equal(new[] { 1, 2, 3 }, restored!.Numbers);
        Assert.Equal(new[] { "first", "second" }, restored.Names);
        Assert.Equal(new long[] { 42, 84 }, restored.Children["child"].Values);
    }
}

public sealed class CollectionPayload
{
    public IList<int> Numbers { get; set; } = new List<int>();
    public IList<string> Names { get; set; } = new List<string>();
    public Dictionary<string, CollectionChild> Children { get; set; } = new();
}

public sealed class CollectionChild
{
    public IList<long> Values { get; set; } = new List<long>();
}

public sealed class RegisteredList<T> : Collection<T>
{
    public bool Supplied { get; set; }
}

[JsonSerializable(typeof(CollectionPayload))]
internal partial class ExternalCollectionContext : JsonSerializerContext
{
}

public class FieldPayloadBase
{
    [JsonInclude] public IList<float> BaseNumbers = new List<float>();
}

public sealed class FieldPayload : FieldPayloadBase
{
    [JsonInclude] public IList<double> Numbers = new List<double>();
}

public class IncludedPropertyPayloadBase
{
    [JsonInclude] internal IList<TimeSpan> BaseValues { get; set; } = new List<TimeSpan>();
}

public sealed class IncludedPropertyPayload : IncludedPropertyPayloadBase
{
    [JsonInclude] internal IList<decimal> Values { get; set; } = new List<decimal>();
}

[JsonDerivedType(typeof(DerivedCollectionPayload), "derived")]
public class PolymorphicCollectionPayload
{
}

public sealed class DerivedCollectionPayload : PolymorphicCollectionPayload
{
    public IList<short> Values { get; set; } = new List<short>();
}

public sealed class PolymorphicEnvelope
{
    public PolymorphicCollectionPayload? First { get; set; }
    public PolymorphicCollectionPayload? Second { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
[JsonDerivedType(typeof(NumericDerivedPayload), 7)]
public class NumericPayload
{
}

public sealed class NumericDerivedPayload : NumericPayload
{
    public string? Name { get; set; }
}
