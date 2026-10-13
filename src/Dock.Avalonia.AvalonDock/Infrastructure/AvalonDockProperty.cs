using System;
using Avalonia;
using Avalonia.Data;

namespace AvalonDock.Infrastructure;

/// <summary>Applies WPF value and binding lifetimes to compatibility dependency properties.</summary>
internal static class AvalonDockProperty
{
    private const BindingValueBehavior Behavior =
        BindingValueBehavior.ReplaceNonWritableBindingOnSetValue |
        BindingValueBehavior.RemoveBindingOnClearValue |
        BindingValueBehavior.ResetCurrentValueOnSourceChange |
        BindingValueBehavior.IgnoreInactiveBindingSourceUpdates;

    internal static StyledPropertyMetadata<T> StyledMetadata<T>(T defaultValue = default!,
        BindingMode defaultBindingMode = BindingMode.OneWay, Func<AvaloniaObject, T, T>? coerce = null,
        bool enableDataValidation = false) =>
        new(defaultValue, defaultBindingMode, coerce, enableDataValidation)
        {
            BindingValueBehavior = Behavior,
        };

    internal static DirectPropertyMetadata<T> DirectMetadata<T>(T unsetValue = default!,
        BindingMode defaultBindingMode = BindingMode.OneWay, bool enableDataValidation = false) =>
        new(unsetValue, defaultBindingMode, enableDataValidation)
        {
            BindingValueBehavior = Behavior,
        };
}
