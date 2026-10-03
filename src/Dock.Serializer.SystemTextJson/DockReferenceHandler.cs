// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Dock.Serializer.SystemTextJson;

/// <summary>Shares references with generated payload converters for one serialization operation.</summary>
internal sealed class DockReferenceHandler : ReferenceHandler
{
    private readonly AsyncLocal<ReferenceResolver?> _current = new();

    internal Scope BeginOperation()
    {
        ReferenceResolver? previous = _current.Value;
        _current.Value = new Resolver();
        return new Scope(this, previous);
    }

    public override ReferenceResolver CreateResolver() => _current.Value ?? new Resolver();

    internal readonly struct Scope(DockReferenceHandler handler, ReferenceResolver? previous) : IDisposable
    {
        public void Dispose() => handler._current.Value = previous;
    }

    private sealed class Resolver : ReferenceResolver
    {
        private readonly Dictionary<string, object> _read = new(StringComparer.Ordinal);
        private readonly Dictionary<object, string> _write = new(ReferenceEqualityComparer.Instance);

        public override void AddReference(string referenceId, object value)
        {
            if (!_read.TryAdd(referenceId, value))
            {
                throw new JsonException($"Duplicate Dock JSON reference '{referenceId}'.");
            }
        }

        public override string GetReference(object value, out bool alreadyExists)
        {
            if (_write.TryGetValue(value, out string? reference))
            {
                alreadyExists = true;
                return reference;
            }
            alreadyExists = false;
            reference = (_write.Count + 1).ToString(CultureInfo.InvariantCulture);
            _write.Add(value, reference);
            return reference;
        }

        public override object ResolveReference(string referenceId) => _read.TryGetValue(referenceId, out object? value)
            ? value
            : throw new JsonException($"Unknown Dock JSON reference '{referenceId}'.");
    }
}
