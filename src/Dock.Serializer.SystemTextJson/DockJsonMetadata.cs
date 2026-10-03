// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// Connects compile-time generated Dock contracts to the existing serializer constructors.
/// This infrastructure is called by generated module initializers, not application code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DockJsonMetadata
{
    // Registration is the single process-wide bridge required by parameterless constructors.
    // Each resolver takes a snapshot; serialization never mutates shared contracts.
    private static readonly object s_gate = new();
    private static readonly List<Registration> s_registrations = new();

    /// <summary>Registers a generated contract and its statically compiled collection creators.</summary>
    /// <param name="resolver">The generated metadata resolver.</param>
    /// <param name="types">The generated types and their legacy contract modifiers.</param>
    /// <param name="collections">Creators indexed by interface collection type and concrete list type.</param>
    public static void Register(
        IJsonTypeInfoResolver resolver,
        IReadOnlyDictionary<Type, DockJsonType> types,
        IReadOnlyDictionary<Type, IReadOnlyDictionary<Type, Func<object>>> collections)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(types);
        ArgumentNullException.ThrowIfNull(collections);
        lock (s_gate)
        {
            s_registrations.Add(new Registration(resolver, types, collections));
        }
    }

    internal static MetadataResolver CreateResolver(Type listType, IServiceProvider? provider = null, bool legacy = false)
    {
        lock (s_gate)
        {
            return new MetadataResolver(s_registrations.ToArray(), listType, provider, legacy);
        }
    }

    internal static bool TryGetCollectionCreator(Type type, Type listType, out Func<object>? creator)
    {
        lock (s_gate)
        {
            for (int i = s_registrations.Count - 1; i >= 0; i--)
            {
                if (s_registrations[i].Collections.TryGetValue(type, out IReadOnlyDictionary<Type, Func<object>>? creators))
                {
                    if (creators.TryGetValue(listType, out creator))
                    {
                        return true;
                    }
                    throw new NotSupportedException($"Dock JSON list type '{listType}' was not generated for '{type}'.");
                }
            }
        }
        creator = null;
        return false;
    }

    internal sealed record Registration(
        IJsonTypeInfoResolver Resolver,
        IReadOnlyDictionary<Type, DockJsonType> Types,
        IReadOnlyDictionary<Type, IReadOnlyDictionary<Type, Func<object>>> Collections);

    internal sealed class MetadataResolver : IJsonTypeInfoResolver
    {
        private readonly Registration[] _registrations;
        private readonly Type _listType;
        private readonly IServiceProvider? _provider;
        private readonly bool _legacy;
        private readonly Dictionary<Type, DockJsonType> _types = new();
        private readonly Dictionary<string, Type> _names = new(StringComparer.Ordinal);

        internal MetadataResolver(Registration[] registrations, Type listType, IServiceProvider? provider, bool legacy)
        {
            _registrations = registrations;
            _listType = listType;
            _provider = provider;
            _legacy = legacy;
            foreach (Registration registration in registrations)
            {
                foreach (KeyValuePair<Type, DockJsonType> entry in registration.Types)
                {
                    _types[entry.Key] = entry.Value;
                    _names[LegacyDockJson.NormalizeTypeName(entry.Value.WireName)] = entry.Key;
                    _names[LegacyDockJson.NormalizeTypeName(entry.Key.FullName ?? entry.Key.Name)] = entry.Key;
                }
            }
        }

        internal DockJsonType? GetContract(Type type) => _types.TryGetValue(type, out DockJsonType? contract) ? contract : null;

        internal Type? ResolveName(string name)
        {
            string normalized = LegacyDockJson.NormalizeTypeName(name);
            if (_names.TryGetValue(normalized, out Type? type))
            {
                return type;
            }
            int depth = 0;
            for (int i = 0; i < normalized.Length; i++)
            {
                if (normalized[i] == '[')
                {
                    depth++;
                }
                else if (normalized[i] == ']')
                {
                    depth--;
                }
                else if (normalized[i] == ',' && depth == 0)
                {
                    // Reference assemblies and their runtime implementations can have different names.
                    return _names.TryGetValue(normalized[..i], out type) ? type : null;
                }
            }
            return null;
        }

        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo? info = null;
            for (int i = _registrations.Length - 1; i >= 0 && info is null; i--)
            {
                info = _registrations[i].Resolver.GetTypeInfo(type, options);
            }

            if (info is null)
            {
                throw new NotSupportedException($"Dock JSON metadata for '{type}' was not generated. Reference the Dock serializer analyzer and register dynamically selected payloads with DockJsonSerializableAttribute.");
            }

            if (info.Kind == JsonTypeInfoKind.Enumerable)
            {
                for (int i = _registrations.Length - 1; i >= 0; i--)
                {
                    if (_registrations[i].Collections.TryGetValue(type, out IReadOnlyDictionary<Type, Func<object>>? creators))
                    {
                        if (!creators.TryGetValue(_listType, out Func<object>? creator))
                        {
                            throw new NotSupportedException($"Dock JSON collection '{type}' with list type '{_listType}' was not generated.");
                        }
                        info.CreateObject = creator;
                        break;
                    }
                }
            }

            if (info.Kind != JsonTypeInfoKind.Object)
            {
                return info;
            }

            if (_legacy && _types.TryGetValue(type, out DockJsonType? contract))
            {
                contract.ApplyLegacyContract(info);
            }

            if (_provider is not null && _types.TryGetValue(type, out DockJsonType? concreteContract) && concreteContract.ElementType is null)
            {
                Func<object>? generatedCreator = info.CreateObject;
                info.CreateObject = () => _provider.GetService(type)
                    ?? generatedCreator?.Invoke()
                    ?? throw new NotSupportedException($"No generated constructor or service registration exists for '{type}'.");
            }

            if (info.PolymorphismOptions is { } polymorphism)
            {
                // Each module contributes its own derived types. Compose them without assembly scanning.
                foreach (KeyValuePair<Type, DockJsonType> entry in _types)
                {
                    if (!entry.Value.CanAssignTo(type) || entry.Key == type)
                    {
                        continue;
                    }
                    bool exists = false;
                    foreach (JsonDerivedType derived in polymorphism.DerivedTypes)
                    {
                        if (derived.DerivedType == entry.Key)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        polymorphism.DerivedTypes.Add(new JsonDerivedType(entry.Key, entry.Key.FullName ?? entry.Key.Name));
                    }
                }
                polymorphism.UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization;
                polymorphism.IgnoreUnrecognizedTypeDiscriminators = false;
            }
            return info;
        }
    }
}

/// <summary>Immutable generated type information used to preserve the legacy JSON contract.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DockJsonType
{
    /// <summary>Creates generated type information.</summary>
    /// <param name="wireName">The legacy assembly-qualified type name.</param>
    /// <param name="applyLegacyContract">A generated modifier for data member names and inclusion.</param>
    /// <param name="elementType">The element type for collection contracts, when applicable.</param>
    /// <param name="canAssignTo">A compiled check of assignable types.</param>
    /// <param name="dictionaryValueType">The dictionary value type, when applicable.</param>
    public DockJsonType(string wireName, Action<JsonTypeInfo> applyLegacyContract, Type? elementType = null, Func<Type, bool>? canAssignTo = null, Type? dictionaryValueType = null)
    {
        WireName = wireName;
        ApplyLegacyContract = applyLegacyContract;
        ElementType = elementType;
        CanAssignTo = canAssignTo ?? (static _ => false);
        DictionaryValueType = dictionaryValueType;
    }

    /// <summary>Gets the legacy type discriminator.</summary>
    public string WireName { get; }
    /// <summary>Gets the compiled legacy contract modifier.</summary>
    public Action<JsonTypeInfo> ApplyLegacyContract { get; }
    /// <summary>Gets the collection element type.</summary>
    public Type? ElementType { get; }
    /// <summary>Gets the compiled assignability check, without runtime type inspection.</summary>
    public Func<Type, bool> CanAssignTo { get; }
    /// <summary>Gets the dictionary value type.</summary>
    public Type? DictionaryValueType { get; }
}
