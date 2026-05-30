using System;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Marks a view model command as available through an agent command registry.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public sealed class AgentCommandAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentCommandAttribute"/> class.
    /// </summary>
    /// <param name="description">A planner-friendly command description.</param>
    public AgentCommandAttribute(string description)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    /// <summary>
    /// Gets the stable command identifier. When omitted, generated registries should derive one from the view model and member name.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the display name exposed to agent clients.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the command description exposed to agent clients.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets a value indicating whether invoking the command may mutate or delete user data.
    /// </summary>
    public bool Destructive { get; init; }

    /// <summary>
    /// Gets a value indicating whether clients should ask for confirmation before invocation.
    /// </summary>
    public bool RequiresConfirmation { get; init; }
}

/// <summary>
/// Marks a view model property as part of an agent-readable state snapshot.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class AgentStateAttribute : Attribute
{
    /// <summary>
    /// Gets the exported state name. When omitted, generated snapshots should use the property name.
    /// </summary>
    public string? Name { get; init; }
}
