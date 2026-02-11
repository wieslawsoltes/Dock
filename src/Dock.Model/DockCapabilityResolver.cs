// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Resolves effective dock capability values using policy precedence.
/// </summary>
public static class DockCapabilityResolver
{
    /// <summary>
    /// Evaluates a capability value for a dockable using the effective policy context.
    /// </summary>
    /// <param name="dockable">The dockable.</param>
    /// <param name="capability">The capability to evaluate.</param>
    /// <returns>The resolved capability evaluation.</returns>
    public static DockCapabilityEvaluation Evaluate(IDockable dockable, DockCapability capability)
    {
        var dockContext = ResolveOperationDock(dockable);
        return Evaluate(dockable, capability, dockContext);
    }

    /// <summary>
    /// Evaluates a capability value for a dockable using an explicit dock context.
    /// </summary>
    /// <param name="dockable">The dockable.</param>
    /// <param name="capability">The capability to evaluate.</param>
    /// <param name="dockContext">The dock policy context.</param>
    /// <returns>The resolved capability evaluation.</returns>
    public static DockCapabilityEvaluation Evaluate(IDockable dockable, DockCapability capability, IDock? dockContext)
    {
        var baseValue = GetDockableCapability(dockable, capability);

        var root = ResolveRoot(dockable, dockContext);
        var rootPolicyValue = root?.RootDockCapabilityPolicy?.Get(capability);
        var dockPolicyValue = dockContext?.DockCapabilityPolicy?.Get(capability);
        var dockableOverrideValue = dockable.DockCapabilityOverrides?.Get(capability);

        var effectiveValue = baseValue;
        var source = DockCapabilityValueSource.Dockable;

        if (rootPolicyValue.HasValue)
        {
            effectiveValue = rootPolicyValue.Value;
            source = DockCapabilityValueSource.RootPolicy;
        }

        if (dockPolicyValue.HasValue)
        {
            effectiveValue = dockPolicyValue.Value;
            source = DockCapabilityValueSource.DockPolicy;
        }

        if (dockableOverrideValue.HasValue)
        {
            effectiveValue = dockableOverrideValue.Value;
            source = DockCapabilityValueSource.DockableOverride;
        }

        var message = BuildDiagnosticMessage(capability, effectiveValue, source);
        return new DockCapabilityEvaluation(
            capability,
            baseValue,
            rootPolicyValue,
            dockPolicyValue,
            dockableOverrideValue,
            effectiveValue,
            source,
            message);
    }

    /// <summary>
    /// Gets a value indicating whether a capability is enabled.
    /// </summary>
    /// <param name="dockable">The dockable.</param>
    /// <param name="capability">The capability to evaluate.</param>
    /// <param name="dockContext">Optional dock policy context.</param>
    /// <returns>True when enabled; otherwise false.</returns>
    public static bool IsEnabled(IDockable dockable, DockCapability capability, IDock? dockContext = null)
    {
        return Evaluate(dockable, capability, dockContext).EffectiveValue;
    }

    /// <summary>
    /// Resolves the dock policy context for a source operation.
    /// </summary>
    /// <param name="dockable">The dockable.</param>
    /// <returns>The resolved dock context, or null.</returns>
    public static IDock? ResolveOperationDock(IDockable dockable)
    {
        return dockable as IDock ?? dockable.Owner as IDock;
    }

    /// <summary>
    /// Resolves the dock policy context for a drop target.
    /// </summary>
    /// <param name="targetDockable">The drop target dockable.</param>
    /// <returns>The resolved dock context, or null.</returns>
    public static IDock? ResolveDropTargetDock(IDockable targetDockable)
    {
        return targetDockable as IDock ?? targetDockable.Owner as IDock;
    }

    private static IRootDock? ResolveRoot(IDockable dockable, IDock? dockContext)
    {
        var root = dockable.Factory?.FindRoot(dockable, _ => true);
        if (root is not null)
        {
            return root;
        }

        if (dockContext is not null)
        {
            root = dockContext.Factory?.FindRoot(dockContext, _ => true);
            if (root is not null)
            {
                return root;
            }
        }

        for (IDockable? current = dockable; current is not null; current = current.Owner)
        {
            if (current is IRootDock ownerRoot)
            {
                return ownerRoot;
            }
        }

        if (dockContext is not null)
        {
            for (IDockable? current = dockContext; current is not null; current = current.Owner)
            {
                if (current is IRootDock ownerRoot)
                {
                    return ownerRoot;
                }
            }
        }

        return null;
    }

    private static bool GetDockableCapability(IDockable dockable, DockCapability capability)
    {
        return capability switch
        {
            DockCapability.Close => dockable.CanClose,
            DockCapability.Pin => dockable.CanPin,
            DockCapability.Float => dockable.CanFloat,
            DockCapability.Drag => dockable.CanDrag,
            DockCapability.Drop => dockable.CanDrop,
            DockCapability.DockAsDocument => dockable.CanDockAsDocument,
            _ => false
        };
    }

    private static string BuildDiagnosticMessage(DockCapability capability, bool effectiveValue, DockCapabilityValueSource source)
    {
        var capabilityName = capability switch
        {
            DockCapability.Close => nameof(IDockable.CanClose),
            DockCapability.Pin => nameof(IDockable.CanPin),
            DockCapability.Float => nameof(IDockable.CanFloat),
            DockCapability.Drag => nameof(IDockable.CanDrag),
            DockCapability.Drop => nameof(IDockable.CanDrop),
            DockCapability.DockAsDocument => nameof(IDockable.CanDockAsDocument),
            _ => capability.ToString()
        };

        var sourceName = source switch
        {
            DockCapabilityValueSource.Dockable => "dockable flag",
            DockCapabilityValueSource.RootPolicy => "root capability policy",
            DockCapabilityValueSource.DockPolicy => "dock capability policy",
            DockCapabilityValueSource.DockableOverride => "dockable override",
            _ => "unknown"
        };

        return effectiveValue
            ? $"Capability '{capabilityName}' allowed by {sourceName}."
            : $"Capability '{capabilityName}' blocked by {sourceName}.";
    }
}
