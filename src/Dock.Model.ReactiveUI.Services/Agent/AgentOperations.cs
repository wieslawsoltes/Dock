using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Represents the lifecycle status of an agent operation.
/// </summary>
public enum AgentOperationStatus
{
    /// <summary>The operation has been created but has not started.</summary>
    Queued,
    /// <summary>The operation is running.</summary>
    Running,
    /// <summary>The operation completed successfully.</summary>
    Completed,
    /// <summary>The operation failed.</summary>
    Failed,
    /// <summary>The operation was cancelled.</summary>
    Cancelled
}

/// <summary>
/// Represents progress reported by an agent command or scenario step.
/// </summary>
/// <param name="Phase">The current phase.</param>
/// <param name="Percent">The optional percent complete.</param>
public sealed record AgentProgress(string Phase, double? Percent = null);

/// <summary>
/// Represents a point-in-time operation snapshot.
/// </summary>
/// <param name="Id">The operation identifier.</param>
/// <param name="Instruction">The original instruction.</param>
/// <param name="Status">The operation status.</param>
/// <param name="Phase">The current phase.</param>
/// <param name="Progress">The optional progress.</param>
/// <param name="Result">The operation result.</param>
/// <param name="Error">The operation error.</param>
/// <param name="ActiveOperations">The active UI operation names.</param>
public sealed record AgentOperationSnapshot(
    string Id,
    string Instruction,
    AgentOperationStatus Status,
    string? Phase,
    double? Progress,
    object? Result,
    string? Error,
    IReadOnlyList<string> ActiveOperations);

/// <summary>
/// Plans natural-language instructions into deterministic operation steps.
/// </summary>
public interface IAgentInstructionPlanner
{
    /// <summary>
    /// Creates a plan for an instruction.
    /// </summary>
    /// <param name="instruction">The natural-language instruction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation plan.</returns>
    Task<AgentOperationPlan> PlanAsync(string instruction, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a deterministic plan created from an instruction.
/// </summary>
/// <param name="Steps">The ordered plan steps.</param>
public sealed record AgentOperationPlan(IReadOnlyList<IAgentOperationStep> Steps);

/// <summary>
/// Represents one executable operation step.
/// </summary>
public interface IAgentOperationStep
{
    /// <summary>
    /// Gets a human-readable step description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the step.
    /// </summary>
    /// <param name="progress">The progress sink.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the step finishes.</returns>
    Task ExecuteAsync(IProgress<AgentProgress> progress, CancellationToken cancellationToken = default);
}

/// <summary>
/// Runs natural-language operations asynchronously and exposes pollable snapshots.
/// </summary>
public sealed class AgentOperationRunner
{
    private readonly ConcurrentDictionary<string, MutableOperation> _operations = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellations = new();
    private readonly UiActivityTracker _activityTracker;
    private readonly IAgentInstructionPlanner _planner;
    private readonly TimeSpan _idleTimeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentOperationRunner"/> class.
    /// </summary>
    /// <param name="activityTracker">The activity tracker.</param>
    /// <param name="planner">The instruction planner.</param>
    /// <param name="idleTimeout">The idle timeout after each step.</param>
    public AgentOperationRunner(UiActivityTracker activityTracker, IAgentInstructionPlanner planner, TimeSpan? idleTimeout = null)
    {
        _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
        _planner = planner ?? throw new ArgumentNullException(nameof(planner));
        _idleTimeout = idleTimeout ?? TimeSpan.FromSeconds(20);
    }

    /// <summary>
    /// Starts an operation in the background.
    /// </summary>
    /// <param name="instruction">The instruction to execute.</param>
    /// <returns>The initial operation snapshot.</returns>
    public AgentOperationSnapshot Start(string instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction))
        {
            throw new ArgumentException("Instruction is required.", nameof(instruction));
        }

        var id = $"op_{Guid.NewGuid():N}";
        var operation = new MutableOperation(id, instruction)
        {
            Status = AgentOperationStatus.Queued,
            Phase = "Queued",
            Progress = 0
        };

        _operations[id] = operation;
        var cts = new CancellationTokenSource();
        _cancellations[id] = cts;
        _ = Task.Run(() => RunAsync(operation, cts.Token));
        return Snapshot(operation);
    }

    /// <summary>
    /// Gets a snapshot by operation id.
    /// </summary>
    /// <param name="id">The operation id.</param>
    /// <returns>The operation snapshot, or <see langword="null"/>.</returns>
    public AgentOperationSnapshot? Get(string id)
    {
        return _operations.TryGetValue(id, out var operation) ? Snapshot(operation) : null;
    }

    /// <summary>
    /// Requests cancellation of a running operation.
    /// </summary>
    /// <param name="id">The operation id.</param>
    /// <returns><see langword="true"/> when a cancellation source was found.</returns>
    public bool Cancel(string id)
    {
        if (!_cancellations.TryGetValue(id, out var cts))
        {
            return false;
        }

        cts.Cancel();
        return true;
    }

    private async Task RunAsync(MutableOperation operation, CancellationToken cancellationToken)
    {
        using var activity = _activityTracker.Begin($"AgentInstruction:{operation.Instruction}");

        try
        {
            Update(operation, AgentOperationStatus.Running, "Planning", 5, null, null);
            var plan = await _planner.PlanAsync(operation.Instruction, cancellationToken).ConfigureAwait(false);
            Update(operation, AgentOperationStatus.Running, "Executing", 15, null, null);

            foreach (var step in plan.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Update(operation, AgentOperationStatus.Running, step.Description, null, null, null);

                await step.ExecuteAsync(
                    new Progress<AgentProgress>(progress => Update(operation, AgentOperationStatus.Running, progress.Phase, progress.Percent, null, null)),
                    cancellationToken).ConfigureAwait(false);

                await _activityTracker.WaitForIdleAsync(_idleTimeout, cancellationToken).ConfigureAwait(false);
            }

            await _activityTracker.WaitForIdleAsync(_idleTimeout, cancellationToken).ConfigureAwait(false);
            Update(operation, AgentOperationStatus.Completed, "Completed", 100, operation.Result, null);
        }
        catch (OperationCanceledException)
        {
            Update(operation, AgentOperationStatus.Cancelled, "Cancelled", null, null, "Operation cancelled.");
        }
        catch (Exception ex)
        {
            Update(operation, AgentOperationStatus.Failed, "Failed", null, null, ex.Message);
        }
        finally
        {
            if (_cancellations.TryRemove(operation.Id, out var cts))
            {
                cts.Dispose();
            }
        }
    }

    private AgentOperationSnapshot Snapshot(MutableOperation operation)
    {
        lock (operation)
        {
            return new AgentOperationSnapshot(
                operation.Id,
                operation.Instruction,
                operation.Status,
                operation.Phase,
                operation.Progress,
                operation.Result,
                operation.Error,
                _activityTracker.ActiveOperations);
        }
    }

    private static void Update(
        MutableOperation operation,
        AgentOperationStatus status,
        string? phase,
        double? progress,
        object? result,
        string? error)
    {
        lock (operation)
        {
            operation.Status = status;
            operation.Phase = phase;
            operation.Progress = progress;
            operation.Result = result ?? operation.Result;
            operation.Error = error;
        }
    }

    private sealed class MutableOperation
    {
        public MutableOperation(string id, string instruction)
        {
            Id = id;
            Instruction = instruction;
        }

        public string Id { get; }

        public string Instruction { get; }

        public AgentOperationStatus Status { get; set; }

        public string? Phase { get; set; }

        public double? Progress { get; set; }

        public object? Result { get; set; }

        public string? Error { get; set; }
    }
}
