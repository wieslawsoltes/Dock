using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Represents a deterministic scenario script.
/// </summary>
/// <param name="Name">The script name.</param>
/// <param name="Timeout">The total timeout.</param>
/// <param name="Steps">The ordered script steps.</param>
public sealed record AgentScript(string Name, TimeSpan Timeout, IReadOnlyList<AgentScriptStep> Steps);

/// <summary>
/// Base type for script steps.
/// </summary>
public abstract record AgentScriptStep;

/// <summary>
/// Invokes an agent command.
/// </summary>
/// <param name="Command">The command id.</param>
/// <param name="Input">The optional input payload.</param>
public sealed record InvokeCommandStep(string Command, JsonElement? Input = null) : AgentScriptStep;

/// <summary>
/// Waits for the UI to become idle.
/// </summary>
/// <param name="Timeout">The idle timeout.</param>
public sealed record WaitIdleStep(TimeSpan Timeout) : AgentScriptStep;

/// <summary>
/// Asserts current route and state values.
/// </summary>
/// <param name="Route">The optional expected route.</param>
/// <param name="State">The optional expected state values.</param>
public sealed record AssertStateStep(string? Route, IReadOnlyDictionary<string, JsonElement>? State = null) : AgentScriptStep;

/// <summary>
/// Captures a screenshot artifact.
/// </summary>
/// <param name="Name">The screenshot name.</param>
public sealed record ScreenshotStep(string Name) : AgentScriptStep;

/// <summary>
/// Delays script execution.
/// </summary>
/// <param name="Duration">The delay duration.</param>
public sealed record DelayStep(TimeSpan Duration) : AgentScriptStep;

/// <summary>
/// Represents a script run result.
/// </summary>
/// <param name="Success">A value indicating whether all steps passed.</param>
/// <param name="ExecutionId">The optional evidence execution id.</param>
/// <param name="Steps">The per-step results.</param>
public sealed record AgentScriptResult(bool Success, string? ExecutionId, IReadOnlyList<AgentScriptStepResult> Steps);

/// <summary>
/// Represents one script step result.
/// </summary>
/// <param name="Index">The zero-based step index.</param>
/// <param name="Step">The step type.</param>
/// <param name="Success">A value indicating whether the step passed.</param>
/// <param name="Error">The error message, when any.</param>
/// <param name="Duration">The elapsed step duration.</param>
public sealed record AgentScriptStepResult(int Index, string Step, bool Success, string? Error, TimeSpan Duration);

/// <summary>
/// Runs deterministic scripts with command invocation, idle waits, assertions, and evidence capture.
/// </summary>
public sealed class AgentScriptRunner
{
    private readonly IAgentCommandRegistry _commands;
    private readonly ICurrentViewModelProvider _currentViewModelProvider;
    private readonly UiActivityTracker _activityTracker;
    private readonly IAgentStateProvider _stateProvider;
    private readonly IAgentScreenshotService? _screenshotService;
    private readonly AgentExecutionRecorder? _executionRecorder;
    private readonly ScreenshotPolicy _screenshotPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentScriptRunner"/> class.
    /// </summary>
    /// <param name="commands">The command registry.</param>
    /// <param name="currentViewModelProvider">The current view model provider.</param>
    /// <param name="activityTracker">The activity tracker.</param>
    /// <param name="stateProvider">The state provider.</param>
    /// <param name="screenshotService">The optional screenshot service.</param>
    /// <param name="executionRecorder">The optional execution recorder.</param>
    /// <param name="screenshotPolicy">The screenshot policy.</param>
    public AgentScriptRunner(
        IAgentCommandRegistry commands,
        ICurrentViewModelProvider currentViewModelProvider,
        UiActivityTracker activityTracker,
        IAgentStateProvider stateProvider,
        IAgentScreenshotService? screenshotService = null,
        AgentExecutionRecorder? executionRecorder = null,
        ScreenshotPolicy? screenshotPolicy = null)
    {
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _currentViewModelProvider = currentViewModelProvider ?? throw new ArgumentNullException(nameof(currentViewModelProvider));
        _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
        _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
        _screenshotService = screenshotService;
        _executionRecorder = executionRecorder;
        _screenshotPolicy = screenshotPolicy ?? new ScreenshotPolicy();
    }

    /// <summary>
    /// Runs a script.
    /// </summary>
    /// <param name="script">The script.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The script result.</returns>
    public async Task<AgentScriptResult> RunAsync(AgentScript script, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(script);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(script.Timeout);

        AgentExecution? execution = _executionRecorder?.Start(script.Name);
        var results = new List<AgentScriptStepResult>(script.Steps.Count);

        for (var i = 0; i < script.Steps.Count; i++)
        {
            var step = script.Steps[i];
            var startedAt = DateTimeOffset.UtcNow;

            try
            {
                if (_screenshotPolicy.BeforeEachStep)
                {
                    await CaptureScreenshotAsync(execution?.Id, $"before-step-{i}", timeout.Token).ConfigureAwait(false);
                }

                await ExecuteStepAsync(step, execution?.Id, timeout.Token).ConfigureAwait(false);

                if (_screenshotPolicy.AfterEachStep)
                {
                    await CaptureScreenshotAsync(execution?.Id, $"after-step-{i}", timeout.Token).ConfigureAwait(false);
                }

                _executionRecorder?.RecordEvent(execution!.Id, "step.completed", step.GetType().Name);
                results.Add(new AgentScriptStepResult(i, step.GetType().Name, true, null, DateTimeOffset.UtcNow - startedAt));
            }
            catch (Exception ex)
            {
                if (_screenshotPolicy.OnFailure)
                {
                    await CaptureScreenshotAsync(execution?.Id, $"failure-step-{i}", CancellationToken.None).ConfigureAwait(false);
                }

                if (execution is not null)
                {
                    _executionRecorder?.RecordEvent(execution.Id, "step.failed", ex.Message);
                    _executionRecorder?.Stop(execution.Id);
                }

                results.Add(new AgentScriptStepResult(i, step.GetType().Name, false, ex.Message, DateTimeOffset.UtcNow - startedAt));
                return new AgentScriptResult(false, execution?.Id, results);
            }
        }

        if (execution is not null)
        {
            _executionRecorder?.Stop(execution.Id);
        }

        return new AgentScriptResult(true, execution?.Id, results);
    }

    private async Task ExecuteStepAsync(AgentScriptStep step, string? executionId, CancellationToken cancellationToken)
    {
        switch (step)
        {
            case InvokeCommandStep invoke:
                await _commands.InvokeAsync(invoke.Command, _currentViewModelProvider.CurrentViewModel, invoke.Input, cancellationToken).ConfigureAwait(false);
                await _activityTracker.WaitForIdleAsync(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);
                break;
            case WaitIdleStep wait:
                await _activityTracker.WaitForIdleAsync(wait.Timeout, cancellationToken).ConfigureAwait(false);
                break;
            case AssertStateStep assert:
                await AssertStateAsync(assert, cancellationToken).ConfigureAwait(false);
                if (_screenshotPolicy.OnNavigation && assert.Route is not null)
                {
                    await CaptureScreenshotAsync(executionId, $"navigation-{assert.Route}", cancellationToken).ConfigureAwait(false);
                }

                break;
            case ScreenshotStep screenshot:
                await CaptureScreenshotAsync(executionId, screenshot.Name, cancellationToken).ConfigureAwait(false);
                break;
            case DelayStep delay:
                await Task.Delay(delay.Duration, cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new NotSupportedException($"Unsupported script step '{step.GetType().Name}'.");
        }
    }

    private async Task AssertStateAsync(AssertStateStep assert, CancellationToken cancellationToken)
    {
        var snapshot = await _stateProvider.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        if (assert.Route is not null && !StringComparer.Ordinal.Equals(snapshot.Route, assert.Route))
        {
            throw new InvalidOperationException($"Expected route '{assert.Route}', got '{snapshot.Route}'.");
        }

        if (assert.State is null)
        {
            return;
        }

        foreach (var expected in assert.State)
        {
            if (!snapshot.State.TryGetValue(expected.Key, out var actual))
            {
                throw new InvalidOperationException($"Missing state key '{expected.Key}'.");
            }

            if (actual.GetRawText() != expected.Value.GetRawText())
            {
                throw new InvalidOperationException($"State mismatch for '{expected.Key}'. Expected {expected.Value.GetRawText()}, got {actual.GetRawText()}.");
            }
        }
    }

    private async Task CaptureScreenshotAsync(string? executionId, string name, CancellationToken cancellationToken)
    {
        if (_screenshotService is null || executionId is null)
        {
            return;
        }

        var path = await _screenshotService.CaptureMainWindowAsync(executionId, name, cancellationToken).ConfigureAwait(false);
        _executionRecorder?.RecordArtifact(executionId, "screenshot", path, name);
    }
}
