using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Represents an execution recording with events and artifact references.
/// </summary>
/// <param name="Id">The execution id.</param>
/// <param name="Name">The execution name.</param>
/// <param name="StartedAt">The start timestamp.</param>
/// <param name="FinishedAt">The optional finish timestamp.</param>
/// <param name="Events">The timeline events.</param>
/// <param name="Artifacts">The artifact references.</param>
public sealed record AgentExecution(
    string Id,
    string Name,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    IReadOnlyList<AgentExecutionEvent> Events,
    IReadOnlyList<AgentArtifact> Artifacts);

/// <summary>
/// Represents one timeline event in an execution recording.
/// </summary>
/// <param name="Timestamp">The event timestamp.</param>
/// <param name="Kind">The event kind.</param>
/// <param name="Message">The event message.</param>
/// <param name="Data">The optional structured event data.</param>
public sealed record AgentExecutionEvent(DateTimeOffset Timestamp, string Kind, string Message, JsonElement? Data = null);

/// <summary>
/// Represents an artifact captured during execution.
/// </summary>
/// <param name="Id">The artifact id.</param>
/// <param name="Kind">The artifact kind.</param>
/// <param name="Path">The artifact path.</param>
/// <param name="Description">The optional artifact description.</param>
public sealed record AgentArtifact(string Id, string Kind, string Path, string? Description);

/// <summary>
/// Captures execution events, command logs, snapshots, screenshots, and other artifacts for later debugging.
/// </summary>
public sealed class AgentExecutionRecorder
{
    private readonly ConcurrentDictionary<string, MutableExecution> _executions = new();

    /// <summary>
    /// Starts recording a new execution.
    /// </summary>
    /// <param name="name">The execution name.</param>
    /// <returns>The execution snapshot.</returns>
    public AgentExecution Start(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Execution name is required.", nameof(name));
        }

        var mutable = new MutableExecution($"exec_{Guid.NewGuid():N}", name, DateTimeOffset.UtcNow);
        _executions[mutable.Id] = mutable;
        RecordEvent(mutable.Id, "execution.started", name);
        return Snapshot(mutable);
    }

    /// <summary>
    /// Stops recording an execution.
    /// </summary>
    /// <param name="executionId">The execution id.</param>
    /// <returns>The final execution snapshot.</returns>
    public AgentExecution Stop(string executionId)
    {
        var execution = GetMutable(executionId);
        lock (execution)
        {
            execution.Events.Add(new AgentExecutionEvent(DateTimeOffset.UtcNow, "execution.finished", "Execution finished"));
            execution.FinishedAt = DateTimeOffset.UtcNow;
            return Snapshot(execution);
        }
    }

    /// <summary>
    /// Gets an execution snapshot.
    /// </summary>
    /// <param name="executionId">The execution id.</param>
    /// <returns>The execution snapshot.</returns>
    public AgentExecution Get(string executionId) => Snapshot(GetMutable(executionId));

    /// <summary>
    /// Lists execution snapshots.
    /// </summary>
    /// <returns>The known executions.</returns>
    public IReadOnlyList<AgentExecution> List()
    {
        return _executions.Values.Select(Snapshot).ToArray();
    }

    /// <summary>
    /// Records an execution event.
    /// </summary>
    /// <param name="executionId">The execution id.</param>
    /// <param name="kind">The event kind.</param>
    /// <param name="message">The event message.</param>
    /// <param name="data">The optional structured event data.</param>
    public void RecordEvent(string executionId, string kind, string message, JsonElement? data = null)
    {
        var execution = GetMutable(executionId);
        lock (execution)
        {
            execution.Events.Add(new AgentExecutionEvent(DateTimeOffset.UtcNow, kind, message, data));
        }
    }

    /// <summary>
    /// Records an artifact reference.
    /// </summary>
    /// <param name="executionId">The execution id.</param>
    /// <param name="kind">The artifact kind.</param>
    /// <param name="path">The artifact path.</param>
    /// <param name="description">The optional artifact description.</param>
    /// <returns>The artifact reference.</returns>
    public AgentArtifact RecordArtifact(string executionId, string kind, string path, string? description = null)
    {
        var artifact = new AgentArtifact($"artifact_{Guid.NewGuid():N}", kind, path, description);
        var execution = GetMutable(executionId);
        lock (execution)
        {
            execution.Artifacts.Add(artifact);
        }

        return artifact;
    }

    private MutableExecution GetMutable(string executionId)
    {
        if (!_executions.TryGetValue(executionId, out var execution))
        {
            throw new InvalidOperationException($"Execution '{executionId}' was not found.");
        }

        return execution;
    }

    private static AgentExecution Snapshot(MutableExecution execution)
    {
        lock (execution)
        {
            return new AgentExecution(
                execution.Id,
                execution.Name,
                execution.StartedAt,
                execution.FinishedAt,
                execution.Events.ToArray(),
                execution.Artifacts.ToArray());
        }
    }

    private sealed class MutableExecution
    {
        public MutableExecution(string id, string name, DateTimeOffset startedAt)
        {
            Id = id;
            Name = name;
            StartedAt = startedAt;
        }

        public string Id { get; }

        public string Name { get; }

        public DateTimeOffset StartedAt { get; }

        public DateTimeOffset? FinishedAt { get; set; }

        public List<AgentExecutionEvent> Events { get; } = new();

        public List<AgentArtifact> Artifacts { get; } = new();
    }
}
