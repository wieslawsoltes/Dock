// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Dock.Model.Core;

namespace Dock.Avalonia.Data;

/// <summary>
/// Selects the optional policy observed by a <see cref="DockCapabilityBindingExtension"/>.
/// </summary>
public enum DockCapabilityBindingSource
{
    /// <summary>The current dockable's overrides.</summary>
    DockableOverrides,
    /// <summary>The active dockable's overrides.</summary>
    ActiveDockableOverrides,
    /// <summary>The current dock's policy.</summary>
    DockPolicy,
    /// <summary>The owning dock's policy.</summary>
    OwnerDockPolicy
}

/// <summary>
/// Creates a one-way compiled binding to an optional capability value, treating missing
/// dockables and policies as inheritance without producing null-path binding warnings.
/// </summary>
/// <remarks>
/// Observes both policy replacement and property notifications from custom policies.
/// Use alongside the capability converters, which resolve the effective value.
/// </remarks>
public sealed class DockCapabilityBindingExtension : MarkupExtension
{
    // An immutable marker keeps intermediate binding nodes non-null. It is never stored in a model.
    private static readonly object s_missing = new();

    /// <summary>Gets or sets the capability to observe.</summary>
    public DockCapability Capability { get; set; }

    /// <summary>Gets or sets the policy source relative to the data context.</summary>
    public DockCapabilityBindingSource Source { get; set; }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var path = new CompiledBindingPathBuilder();
        switch (Source)
        {
            case DockCapabilityBindingSource.DockableOverrides:
                AddOverrides(path);
                break;
            case DockCapabilityBindingSource.ActiveDockableOverrides:
                AddProperty(path, nameof(IDock.ActiveDockable), static o => (o as IDock)?.ActiveDockable ?? s_missing);
                AddOverrides(path);
                break;
            case DockCapabilityBindingSource.DockPolicy:
                AddPolicy(path);
                break;
            case DockCapabilityBindingSource.OwnerDockPolicy:
                AddProperty(path, nameof(IDockable.Owner), static o => (o as IDockable)?.Owner ?? s_missing);
                AddPolicy(path);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Source), Source, null);
        }

        switch (Capability)
        {
            case DockCapability.Close:
                AddCapability(path, nameof(IDockCapabilityPolicy.CanClose), static o => (o as IDockCapabilityPolicy)?.CanClose);
                break;
            case DockCapability.Pin:
                AddCapability(path, nameof(IDockCapabilityPolicy.CanPin), static o => (o as IDockCapabilityPolicy)?.CanPin);
                break;
            case DockCapability.Float:
                AddCapability(path, nameof(IDockCapabilityPolicy.CanFloat), static o => (o as IDockCapabilityPolicy)?.CanFloat);
                break;
            case DockCapability.Drag:
                AddCapability(path, nameof(IDockCapabilityPolicy.CanDrag), static o => (o as IDockCapabilityPolicy)?.CanDrag);
                break;
            case DockCapability.Drop:
                AddCapability(path, nameof(IDockCapabilityPolicy.CanDrop), static o => (o as IDockCapabilityPolicy)?.CanDrop);
                break;
            case DockCapability.DockAsDocument:
                AddCapability(path, nameof(IDockCapabilityPolicy.CanDockAsDocument), static o => (o as IDockCapabilityPolicy)?.CanDockAsDocument);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Capability), Capability, null);
        }

        return new CompiledBindingExtension(path.Build())
        {
            Mode = BindingMode.OneWay
        }.ProvideValue(serviceProvider);
    }

    private static void AddOverrides(CompiledBindingPathBuilder path) =>
        AddProperty(path, nameof(IDockable.DockCapabilityOverrides), static o => (o as IDockable)?.DockCapabilityOverrides ?? s_missing);

    private static void AddPolicy(CompiledBindingPathBuilder path) =>
        AddProperty(path, nameof(IDock.DockCapabilityPolicy), static o => (o as IDock)?.DockCapabilityPolicy ?? s_missing);

    private static void AddProperty(CompiledBindingPathBuilder path, string name, Func<object, object?> getter) =>
        path.Property(new ClrPropertyInfo(name, getter, null, typeof(object)), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor);

    private static void AddCapability(CompiledBindingPathBuilder path, string name, Func<object, object?> getter) =>
        path.Property(new ClrPropertyInfo(name, getter, null, typeof(bool?)), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor);
}
