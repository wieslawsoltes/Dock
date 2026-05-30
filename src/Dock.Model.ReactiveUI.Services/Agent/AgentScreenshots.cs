using System.Threading;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services.Agent;

/// <summary>
/// Captures screenshots for execution evidence.
/// </summary>
public interface IAgentScreenshotService
{
    /// <summary>
    /// Captures the main window and returns the artifact path.
    /// </summary>
    /// <param name="executionId">The execution id.</param>
    /// <param name="name">The screenshot name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The artifact path.</returns>
    Task<string> CaptureMainWindowAsync(string executionId, string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Controls automatic screenshot capture around scenario steps.
/// </summary>
/// <param name="BeforeEachStep">Capture before every step.</param>
/// <param name="AfterEachStep">Capture after every step.</param>
/// <param name="OnFailure">Capture on failures.</param>
/// <param name="OnNavigation">Capture after navigation assertions.</param>
/// <param name="OnDialogOpened">Capture when a host maps dialog events to this policy.</param>
public sealed record ScreenshotPolicy(
    bool BeforeEachStep = false,
    bool AfterEachStep = true,
    bool OnFailure = true,
    bool OnNavigation = false,
    bool OnDialogOpened = false);
