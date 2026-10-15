using System;
using Avalonia;
using Avalonia.Data;

namespace AvalonDock.Infrastructure;

/// <summary>Creates metadata for compatibility properties added to existing Avalonia properties.</summary>
internal static class AvalonDockProperty
{
    // Binding ownership follows official Avalonia until the opt-in WPF contracts
    // tracked in Dock #1186 are available in a published framework release.
    internal static StyledPropertyMetadata<T> StyledMetadata<T>(T defaultValue = default!,
        BindingMode defaultBindingMode = BindingMode.OneWay, Func<AvaloniaObject, T, T>? coerce = null,
        bool enableDataValidation = false) =>
        new(defaultValue, defaultBindingMode, coerce, enableDataValidation);
}
