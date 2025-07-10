using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dock.Model.Core;

namespace Dock.Serializer.TextJson;

/// <summary>
/// Serializer implementation based on System.Text.Json.
/// </summary>
public sealed class DockTextJsonSerializer : IDockSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly JsonSerializerContext? _context;

    /// <summary>
    /// Initializes a new instance with optional serializer options and context.
    /// </summary>
    /// <param name="options">Options to configure serialization.</param>
    /// <param name="context">Generated context for fast serialization.</param>
    public DockTextJsonSerializer(JsonSerializerOptions? options = null, JsonSerializerContext? context = null)
    {
        _options = options ?? new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = false,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };
        _context = context;
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        return _context is null
            ? JsonSerializer.Serialize(value, _options)
            : JsonSerializer.Serialize(value, _context);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        return _context is null
            ? JsonSerializer.Deserialize<T>(text, _options)
            : JsonSerializer.Deserialize<T>(text, _context);
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var json = reader.ReadToEnd();
        return Deserialize<T>(json);
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        var json = Serialize(value);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(json);
    }
}
