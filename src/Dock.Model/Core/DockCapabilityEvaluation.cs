// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Represents a resolved effective capability value and diagnostics.
/// </summary>
public sealed class DockCapabilityEvaluation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockCapabilityEvaluation"/> class.
    /// </summary>
    /// <param name="capability">The evaluated capability.</param>
    /// <param name="baseValue">The base dockable flag value.</param>
    /// <param name="rootPolicyValue">The root policy value, if set.</param>
    /// <param name="dockPolicyValue">The dock policy value, if set.</param>
    /// <param name="dockableOverrideValue">The dockable override value, if set.</param>
    /// <param name="effectiveValue">The effective resolved value.</param>
    /// <param name="effectiveSource">The source of the effective value.</param>
    /// <param name="diagnosticMessage">Human-readable diagnostics string.</param>
    public DockCapabilityEvaluation(
        DockCapability capability,
        bool baseValue,
        bool? rootPolicyValue,
        bool? dockPolicyValue,
        bool? dockableOverrideValue,
        bool effectiveValue,
        DockCapabilityValueSource effectiveSource,
        string diagnosticMessage)
    {
        Capability = capability;
        BaseValue = baseValue;
        RootPolicyValue = rootPolicyValue;
        DockPolicyValue = dockPolicyValue;
        DockableOverrideValue = dockableOverrideValue;
        EffectiveValue = effectiveValue;
        EffectiveSource = effectiveSource;
        DiagnosticMessage = diagnosticMessage;
    }

    /// <summary>
    /// Gets the evaluated capability.
    /// </summary>
    public DockCapability Capability { get; }

    /// <summary>
    /// Gets the base dockable flag value.
    /// </summary>
    public bool BaseValue { get; }

    /// <summary>
    /// Gets the root policy value, if set.
    /// </summary>
    public bool? RootPolicyValue { get; }

    /// <summary>
    /// Gets the dock policy value, if set.
    /// </summary>
    public bool? DockPolicyValue { get; }

    /// <summary>
    /// Gets the dockable override value, if set.
    /// </summary>
    public bool? DockableOverrideValue { get; }

    /// <summary>
    /// Gets the effective resolved value.
    /// </summary>
    public bool EffectiveValue { get; }

    /// <summary>
    /// Gets the source of the effective resolved value.
    /// </summary>
    public DockCapabilityValueSource EffectiveSource { get; }

    /// <summary>
    /// Gets a human-readable diagnostic message.
    /// </summary>
    public string DiagnosticMessage { get; }
}
