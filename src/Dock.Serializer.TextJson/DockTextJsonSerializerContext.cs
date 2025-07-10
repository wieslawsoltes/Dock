using System.Text.Json.Serialization;

namespace Dock.Serializer.TextJson;

/// <summary>
/// Source generation context for DockTextJsonSerializer.
/// Add additional JsonSerializable attributes in your application to include types.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(object))]
public partial class DockTextJsonSerializerContext : JsonSerializerContext;
