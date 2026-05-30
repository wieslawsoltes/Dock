using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Represents a deterministic snapshot of the agent-visible application state.
/// </summary>
/// <param name="Route">The current route or view model name.</param>
/// <param name="State">The exported state values.</param>
/// <param name="ActiveOperations">The active operation names.</param>
public sealed record AgentStateSnapshot(
    string? Route,
    IReadOnlyDictionary<string, JsonElement> State,
    IReadOnlyList<string> ActiveOperations);

/// <summary>
/// Produces current state snapshots for agent assertions and reports.
/// </summary>
public interface IAgentStateProvider
{
    /// <summary>
    /// Gets a state snapshot.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The state snapshot.</returns>
    Task<AgentStateSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Creates state snapshots from supplied delegates without reflection.
/// </summary>
public sealed class DelegateAgentStateProvider : IAgentStateProvider
{
    private readonly Func<string?> _getRoute;
    private readonly Func<IReadOnlyDictionary<string, JsonElement>> _getState;
    private readonly UiActivityTracker? _activityTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateAgentStateProvider"/> class.
    /// </summary>
    /// <param name="getRoute">The route provider.</param>
    /// <param name="getState">The state provider.</param>
    /// <param name="activityTracker">The optional activity tracker.</param>
    public DelegateAgentStateProvider(
        Func<string?> getRoute,
        Func<IReadOnlyDictionary<string, JsonElement>> getState,
        UiActivityTracker? activityTracker = null)
    {
        _getRoute = getRoute ?? throw new ArgumentNullException(nameof(getRoute));
        _getState = getState ?? throw new ArgumentNullException(nameof(getState));
        _activityTracker = activityTracker;
    }

    /// <inheritdoc />
    public Task<AgentStateSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<string> activeOperations = _activityTracker?.ActiveOperations ?? Array.Empty<string>();
        var snapshot = new AgentStateSnapshot(_getRoute(), _getState(), activeOperations);
        return Task.FromResult(snapshot);
    }
}
